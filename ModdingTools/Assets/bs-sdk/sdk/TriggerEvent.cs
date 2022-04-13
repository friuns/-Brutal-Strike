using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if game
using Slowsharp;
using EditorRuntime;
using EditorGUILayout = EditorRuntime.EditorGUILayout;
#endif
#if UNITY_EDITOR
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
    
        
    public int triggerIndex = -1;
    public SerializedMember triggerMethod { get { return triggerIndex == -1 ? null : methodInfos.Get(triggerIndex); } set { triggerIndex = methodInfos.IndexOf(value); } }
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
                m_typeData = Resources.Load<TypeData>("eventTriggerData");
                if (m_typeData== null)
                {
                    m_typeData= TypeData.CreateInstance<TypeData>();
#if UNITY_EDITOR
                    AssetDatabase.CreateAsset(m_typeData, "Assets/scripts/sdk/Editor/Resources/eventTriggerData.asset");
#endif
                }
            }
            return m_typeData;
        }
    }
    
#if UNITY_EDITOR
    public bool showAll;
    
    

    private void actionsAddRange(IEnumerable<SerializedMember> list)
    {
        if (!showAll)
        {
            var d = list.Where(a => a.exposed).ToList();
            if (d.Count > 0)
                list = d;
        }
        actions.AddRange(list);
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
        int i;
        bw.Write(SceneFinder.GetPath((target as Component)?.transform, out i));
        bw.Write(i);
        bw.Write(code);
        bw.WriteByte((byte)actionParameters.Count);
        foreach (var a in actionParameters)
        {
            bw.WriteValue(a.value);
        }
        base.Save(bw);
    }
    public override void Load(BinaryReader br)
    {
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
        
        base.Load(br);
    }
#endif
    public override void OnInspectorGUI()
    {
        
        // if (Application.isPlaying) return;
        // var d = new DropdownItem<int>(4,"");

        if (bs.HasChanged(ToolBar(methodInfos, triggerIndex, "Trigger", a => (a.DeclaringType.Name??"null") + "/" + MethodName(a)), ref triggerIndex) || actions.Count ==0|| /*(action?.name == null || action.name =="target") &&*/  HasChanged(EditorGUILayout.ObjectField("target", target, typeof(Object)), ref target) )
        {
            actions = new List<SerializedMember>();
            
            if(target is GameObject g)
                foreach(Component o in g.GetComponents<Component>())
                    actionsAddRange(typeData.Get(o.GetType()).Select(a => a.Clone("target", o.GetType())));
            else if(target !=null) 
                actionsAddRange(typeData.Get(target.GetType()).Select(a => a.Clone("target")));

            if (triggerMethod != null)
            {
                foreach (var p in triggerMethod.GetParameters())
                    if (!p.ParameterType.IsValueType)
                        actionsAddRange(typeData.Get(p.ParameterType).Select(a => a.Clone(p.Name)));
                actionsAddRange(typeData.Get(triggerMethod.DeclaringType).Select(a => a.Clone("this")));
            }
            actionsAddRange(typeData.Get(typeof(Game)).Select(a => a.Clone("bs._Game")));
            actionsAddRange(typeData.Get(typeof(Player)).Select(a => a.Clone("bs._Player")));
            actionsAddRange(typeData.Get(typeof(Hud)).Select(a => a.Clone("bs._Hud")));
        }

        // if (action != null)
        {
            trigger = triggerMethod;
            if (bs.HasChanged(ToolBar( actions , action, "Action", a => a.path), ref action)) //Refresh aciton parameters
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
                            var prms = trigger.GetParameters().Where(a => actionParameters[i].type.IsAssignableFrom(a.ParameterType)).Select(a => a.Name).ToArray();
                            if (prms.Length > 0 && bs.HasChanged(TriggerEvent.ToolBar(prms, actionParameters[i].reference, "", null, "None", GUILayout.Width(100)), ref actionParameters[i].reference))
                                GenerateCode();
                        }
                    }
                }
            }
        }

        code = GUILayout.TextArea(code);
        masterOnly = GUILayout.Toggle(masterOnly, "Execute Host Only");

    }
 
    [HideInInspector]
    public List<SerializedMember> actions = new List<SerializedMember>();
    
    #endif
    

    

    public Object target;

    public List<SerializedValue> actionParameters = new  List<SerializedValue>();
    public SerializedMember action;


    

    public static object DefaultValue(Type t)
    {
        
        return t == typeof(string) ? "" : t.IsValueType ? Activator.CreateInstance(t) : null;
    }


    public SerializedMember trigger;

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
    
    private void GenerateCode()
    {
        try
        {
            if (actionParameters == null) return;
            if (action == null) return;
            var expCount = action.indexes.Count;
            var p = string.Join(",", actionParameters.Take(actionParameters.Count - expCount).Select((a, i) => !string.IsNullOrEmpty(a.reference) ? a.reference : (a.value is Object ? $"prms[{i}]" : a.ToString())));
            var path = string.Format(action.code, actionParameters.Skip(expCount).ToArray());
            
            if (triggerMethod == null) return;
            code = $@"
void {MethodName(triggerMethod)}
{{
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
    
    private void CompileCode(bool hook=true) //2do optimize - script storage, returns non static class 
    {
        var Runner = ScriptPool.GetClass("class cs:bs{ public  Object target;public  object[] prms; " + code + "}");

        var methods = Runner.GetTypes()[0].GetMethods();
        foreach (object comp in trigableObjects)
        {
            foreach (var m in methods)
            {
                var f = (comp as Type ?? comp.GetType()).GetField("_" + m.Id, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                
                f?.SetValue(comp, hook && (!masterOnly || isMaster) ? new Hook(
                        delegate(object[] a)
                        {
                            var instance = Runner.GetTypes()[0].Override(Runner.Runner, new HybInstance[0], comp);
                            instance.SetPropertyOrField("target", HybInstance.Object(target));
                            instance.SetPropertyOrField("prms", HybInstance.ObjectArray(actionParameters.Select(b => b.value).ToArray()));

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