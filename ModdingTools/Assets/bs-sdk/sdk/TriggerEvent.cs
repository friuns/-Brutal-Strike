using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if game
using Slowsharp;
#endif
using EditorRuntime;
using EditorGUILayout = EditorRuntime.EditorGUILayout;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

using Object = UnityEngine.Object;
#pragma warning disable 618

// [ExecuteInEditMode]
public class TriggerEvent : ItemBase,IOnLevelEditorGUI,IOnInspectorGUIHide,IOnInspectorGUI,IOnLoadAsset
{

    
    
    // public int triggerIndex = -1;
    
    private List<SerializedMember> m_methodInfos= new List<SerializedMember>();
    public List<SerializedMember> methodInfos => m_methodInfos.Count ==0 ? m_methodInfos = typeData.GetMethodInfos(trigableObjects):m_methodInfos;
    private object[] trigableObjects => GetComponentsInChildren<Base>().Cast<object>().Concat(new[] { typeof(Player), typeof(Game) }).ToArray();
    
    public static TypeData m_typeData;
    public static TypeData typeData
    {
        get
        {
            if (m_typeData == null)
            {
                

                var path = sdk?"Assets/bs-sdk/sdk/eventTriggerData.asset":"Assets/scripts/sdk/eventTriggerData.asset";

#if UNITY_EDITOR
                m_typeData = AssetDatabase.LoadAssetAtPath<TypeData>(path);
#endif
                if (m_typeData== null)
                {
                    m_typeData= TypeData.CreateInstance<TypeData>();
#if UNITY_EDITOR
                    AssetDatabase.CreateAsset(m_typeData, path);
                    // File.Move(path,@"../builds/brutalStrike/ModdingTools/Assets/eventTriggerData.asset");
#endif
                }
            }
            return m_typeData;
        }
    }
    public bool showAll;
    private void actionsAddRange(IEnumerable<SerializedMember> list)
    {
        if (!showAll)
        {
            var d = list.Where(a => a.exposed).ToList();
            if (d.Count > 0)
                list = d;
            actions.AddRange(list);
        }
        else
            actions.AddRange(list.OrderByDescending(a => a.exposed));
    }


    [ContextMenu("Clear")]
    public void Clear()
    {
        typeData.types.Clear();
    }
    public static int ToolBar<T>(IList<T> ts, int t, string label, Func<T, string> @select = null, string def = "None", params GUILayoutOption[] prms)
    { 
        return ts.IndexOf(ToolBar(ts, t == -1 ? default : ts.Get(t), label, select, def, prms));
    }
    static class Cache<T>
    {
        public static Dictionary<IList<T>, string[]> cache = new Dictionary<IList<T>, string[]>();
    }
    public static T ToolBar<T>(IList<T> ts, T t, string label, Func<T, string> select = null, string def = "None", params GUILayoutOption[] prms)
    {
        if (!Cache<T>.cache.TryGetValue(ts, out string[] enumerable))
            Cache<T>.cache[ts] = enumerable = new[] { def }.Concat(ts.Select(select ?? (a => a.ToString()))).ToArray();
        
        var index = EditorGUILayout.Popup(label, ts.IndexOf(t) + 1, enumerable, prms) - 1;
        if (index != -1)
            return ts[index];
        return default;
    }
    public static object DrawObject(object value, string name, Type type)
    {
        if (value is Object || value is null)
            return EditorGUILayout.ObjectField(name, (Object)value, type);
        else if (value is Enum e)
        {
            return EditorGUILayout.EnumPopup(name, e);
        }
        return bs.DrawObject(value, name);
    }


    
#if game
    public override void OnLoadAsset()
    {
        base.OnLoadAsset();
        CompileCode(enabled);
    }
    public override void Awake()
    {
        base.Awake();
        LevelEditor._OpenShowAdminWindow += delegate { CompileCode(enabled); };

    }
    protected override void OnCreate(bool b)
    {
        base.OnCreate(b);
    }
    public override void Save(BinaryWriter bw)
    {
        base.Save(bw);
        int i;
        bw.Write(SceneFinder.GetPath((target as Component)?.transform, out i));
        bw.Write(i);
        bw.Write(code);
        bw.WriteByte((byte)actionParameters.Count);
        foreach (var a in actionParameters)
        {
            bw.WriteValue(a.value);
        }
        bw.Write(Serializer.Serialize(trigger));
        bw.Write(Serializer.Serialize(action));
    }
    public override void Load(BinaryReader br)
    {
        base.Load(br);
        var path = br.ReadString();
        var i2 = br.ReadInt32();
        target = SceneFinder.FindAtPath(path, i2);
        code = br.ReadString();
        actionParameters.Clear();
        
        var cnt = br.ReadByte();
        for (int i = cnt - 1; i >= 0; i--)
        {
            var v = br.ReadValue();
            actionParameters.Add(new SerializedValue(v));
        }
        trigger = Serializer.Deserialize<SerializedMember>(br.ReadString());
        action = Serializer.Deserialize<SerializedMember>(br.ReadString());
        CompileCode(enabled);
    }
#endif
    public override void OnInspectorGUI()
    {
        
        // if (Application.isPlaying) return;
        // var d = new DropdownItem<int>(4,"");

        if (bs.HasChanged(ToolBar(methodInfos, trigger, "Trigger", a => (a.DeclaringType.Name??"null") + "/" + MethodName(a)), ref trigger) || actions.Count ==0|| trigger != null && HasChanged(EditorGUILayout.ObjectField("target", target, typeof(Transform)), ref target))
        {
            actions = new List<SerializedMember>();
            
            if(target is GameObject g)
                foreach(Component o in g.GetComponents<Component>())
                    actionsAddRange(typeData.Get(o.GetType()).Select(a => a.Clone("target", o.GetType())));
            else if(target !=null) 
                actionsAddRange(typeData.Get(target.GetType()).Select(a => a.Clone("target")));

            if (trigger != null)
            {
                foreach (var p in trigger.GetParameters())
                    if (!p.ParameterType.IsValueType && !p.ParameterType.IsPrimitive && !p.ParameterType.IsEnum)
                        actionsAddRange(typeData.Get(p.ParameterType).Select(a => a.Clone(p.Name)));
                actionsAddRange(typeData.Get(trigger.DeclaringType).Select(a => a.Clone("this")));
            }
            actionsAddRange(typeData.Get(typeof(Game)).Select(a => a.Clone("bs._Game")));
            actionsAddRange(typeData.Get(typeof(Player)).Select(a => a.Clone("bs._Player")));
            actionsAddRange(typeData.Get(typeof(Hud)).Select(a => a.Clone("bs._Hud")));
            GenerateCode();
        }

        // if (action != null)
        {
            
            if (trigger != null && bs.HasChanged(ToolBar(actions, action, "Action", a => a.path), ref action)) //Refresh aciton parameters
            {
                if (action == null)
                    actionParameters.Clear();
                else
                {
                    if (action.targetType != null && target is GameObject o)
                        target = o.GetComponent(action.targetType);
                    
                    actionParameters = action.parameters.Select(a => a.ParameterType).Concat(action.indexes.Select(a => a.type)).Select(type => new SerializedValue(DefaultValue(type), type)).ToList();
                }
                GenerateCode();
            }

            if (action != null)
            {
                var parameterInfos = action.parameters;

                for (var i = 0; i < actionParameters.Count; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        if (bs.HasChanged(a => actionParameters[i].value = DrawObject(a, parameterInfos.Get(i)?.Name ?? "index", parameterInfos.Get(i)?.ParameterType), actionParameters[i].value))
                            GenerateCode();
                        if (trigger != null)
                        {
                            string[] prms = trigger.GetParameters().Where(a => actionParameters[i].type.IsAssignableFrom(a.ParameterType)).Select(a => a.Name).ToArray();
                            if (prms.Length > 0 && bs.HasChanged(ToolBar(prms, actionParameters[i].reference, "", null, "None", GUILayout.Width(100)), ref actionParameters[i].reference))
                                GenerateCode();
                        }
                    }
                }
            }
        }

        if(HasChanged(GUILayout.TextArea(code),ref code))
            SetDirty();
        if (!string.IsNullOrEmpty(customCode))
            customCode = GUILayout.TextArea(customCode);
        if(!Application.isPlaying)
            if (HasChanged(GUILayout.Toggle(showAll, "Show all functions"), ref showAll))
                actions = new List<SerializedMember>();
        masterOnly = GUILayout.Toggle(masterOnly, "Execute Host Only");

    }
    public SerializedMember trigger;
    
    private List<SerializedMember> actions = new List<SerializedMember>();
    public Object target;

    public List<SerializedValue> actionParameters = new  List<SerializedValue>();
    public SerializedMember action;


    

    public static object DefaultValue(Type t)
    {
        
        return t == typeof(string) ? "" : t.IsValueType ? Activator.CreateInstance(t) : null;
    }



    public static string cast(string name)
    {
        return name == "String" ? "string" : name == "Int32" ? "int" : name == "Single" ? "float" : name == "Boolean" ? "bool" : name;
    }

    public static string MethodName(SerializedMember info)
    {
        
        var p = info.name + "(";
        var parameters = info.GetParameters();
        foreach (var item in parameters)
            p += $"{cast(item.ParameterType.Name)} {item.Name}, ";
        return p.Trim(' ', ',') + ")"; 
    }
    public bool masterOnly;
    [ContextMenu("Refresh Code")]
    private void GenerateCode()
    {
        try
        {
            if (actionParameters == null) return;
            if (action == null||string.IsNullOrEmpty(action.name)) return;
            var expCount = action.indexes.Count;
            var p = string.Join(",", actionParameters.Take(actionParameters.Count - expCount).Select((a, i) => !string.IsNullOrEmpty(a.reference) ? a.reference : (a.value is Object ? $"prms[{i}]" : a.ToString())));
            var path = string.Format(action.code, actionParameters.Skip(expCount).ToArray());
            
            if (trigger == null) return;
            code = $@"
void {trigger.DeclaringType.Name}_{MethodName(trigger)}
{{  {customCode}
    {path}.{action.name}({p});
}}
";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
#if game
    
    
    [ContextMenu("Start")]
    private void OnEnable()
    {
        CompileCode(true);
    }
    private void OnDisable()
    {
        if (Application.isPlaying)
            CompileCode(false);
    }
    private HybInstance instance;
    private void CompileCode(bool hook) //2do optimize - script storage, returns non static class 
    {
        var Runner = ScriptPool.GetClass("class cs :TriggerEvent{ public  Object caller;public  Object target;public  object[] prms; " + code + "}");
        instance = Runner.GetTypes()[0].Override(Runner.Runner, new HybInstance[0],this);
        var methods = Runner.GetTypes()[0].GetMethods().ToArray();
        foreach (object comp in trigableObjects)
        {
            foreach (SSMethodInfo m in methods)
            {
                Type type = (comp as Type ?? comp.GetType());
                if (!m.Id.StartsWith(type.Name)) continue;
                
                var f = type.GetField(m.Id.Substring(type.Name.Length), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                
                f?.SetValue(comp, hook && (!masterOnly || isMaster) ? new Hook(
                        delegate(object[] a)
                        {
                            
                            instance.SetPropertyOrField("caller", HybInstance.Object(comp));
                            instance.SetPropertyOrField("target", HybInstance.Object(target)); 
                            instance.SetPropertyOrField("prms", HybInstance.ObjectArray(actionParameters.Select(b => b.value).ToArray())); //2do add parameters expose instead

                            var d = m.Invoke(instance, a.Select(HybInstance.Object).ToArray());
                            if (d?.InnerObject is SSEnumerator ss)
                                StartCoroutine(ss);
                        })
                    : null);
            }
            // var d = (Delegate)act;
            // d+= Delegate.CreateDelegate();
        }
        SetDirty();
    }

    public override void OnLevelEditorGUI()
    {
        OnInspectorGUI();
        base.OnLevelEditorGUI();
        // code = GUILayout.TextArea(code);
    }
#endif
    public string customCode="";
    public string code;


  
}
#if game
public static class ScriptPool
{
    private static Dictionary<string, CScript> cache = new Dictionary<string, CScript>();
    public static CScript GetClass(string s)
    {
        if (cache.TryGetValue(s, out var o))
            return o;

        return cache[s] = CScript.CreateRunner(s, new ScriptConfig() { DefaultUsings = new[] { "UnityEngine", "System.Collections", "System.Collections.Generic", "System.Linq" } });
    }
    
}
#endif