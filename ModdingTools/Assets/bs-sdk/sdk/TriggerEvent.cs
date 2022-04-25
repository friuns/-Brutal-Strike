using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

using Object = UnityEngine.Object;
#pragma warning disable 618

// [ExecuteInEditMode]
public class TriggerEvent : ItemBase,IOnLevelEditorGUI,IOnInspectorGUIHide,IOnInspectorGUI,IOnLoadAsset
{
  
    // public int triggerIndex = -1;
        
    private List<SerializedMember> m_methodInfos= new List<SerializedMember>();
    public List<SerializedMember> methodInfos => m_methodInfos.Count ==0 ? m_methodInfos = typeData.GetMethodInfos(trigableObjects):m_methodInfos;
    private object[] trigableObjects => GetComponentsInChildren<MonoBehaviour>().Cast<object>().Concat(new[] { typeof(Player), typeof(Game) }).ToArray();
    
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


    [ContextMenu("Clear Types")]
    public void Clear()
    {
        typeData.types.Clear();
    }
    [ContextMenu("Clear Triggers")]
    public void ClearTriggers()
    {
        typeData.triggers.Clear();
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
        #if game
        if (type == typeof(string))
            return TextField(name, (string)value, int.MaxValue);
        #endif
        if (value is Object || value is null)
            return EditorGUILayout.ObjectField(name, (Object)value, type);
         if (value is Enum e)
            return EditorGUILayout.EnumPopup(name, e);

        
         
        return bs.DrawObject(value, name);
    }


    
#if game
  [ContextMenu("Test")]
    public void Test()
    {
        
        CScript r = CScript.CreateRunner( "class cs :TriggerEvent{ public  Object caller; " + code + "}",new ScriptConfig() { DefaultUsings = new[] { "UnityEngine","System.Collections","System.Collections.Generic","System.Linq" } });
        
        var  hybType = r.GetTypes()[0].Override(r.Runner,new HybInstance[0],this);
        var ssMethodInfo = hybType.GetMethods("Test2")[1];
        ssMethodInfo.Invoke(HybInstance.Object(this));
    }

    public override void OnLoadAsset()
    {
        base.OnLoadAsset();
        // CompileCodeStart(enabled);
    }
    public override void Awake()
    {
        base.Awake();
        
        LevelEditor._OpenShowAdminWindow += delegate
        {
            CompileCodeStart(enabled);
        };

    }
    protected override void OnCreate(bool b)
    {
        base.OnCreate(b);
    }
    public override void Save(BinaryWriter bw)
    {
        base.Save(bw);
        bw.WriteComponent(target);
        bw.Write(code);
        bw.WriteByte((byte)exposedParams.Count);
        foreach (var a in exposedParams)
        {
            bw.Write(a.name);
            bw.WriteValue(a.value);
        }
        bw.Write(Serializer.Serialize(trigger));
        bw.Write(Serializer.Serialize(action));
    }
    public override void Load(BinaryReader br)
    {
        base.Load(br);
        target = br.ReadComponent();
        code = br.ReadString();
        exposedParams.Clear();
        
        var cnt = br.ReadByte();
        for (int i = cnt - 1; i >= 0; i--)
        {
            var name = br.ReadString();
                var v = br.ReadValue();
            exposedParams.Add(new SerializedValue(v){name = name});
        }
        trigger = Serializer.Deserialize<SerializedMember>(br.ReadString());
        action = Serializer.Deserialize<SerializedMember>(br.ReadString());
        CompileCodeStart(enabled);
    }
    public override void CopyFrom(ItemBase b) //breaks object references
    {

    }
#endif
    
    public override void OnInspectorGUI()
    {
        
        // if (Application.isPlaying) return;
        // var d = new DropdownItem<int>(4,"");
        
        if (bs.HasChanged(ToolBar(methodInfos, trigger, "Trigger", a => (a.DeclaringType.Name??"null") + "/" + MethodName(a)), ref trigger) || actions.Count ==0|| trigger != null &&  HasChanged(EditorGUILayout.ObjectField("target", target, typeof(GameObject)), ref target))
        {
            actions = new List<SerializedMember>();

            if (target is GameObject g)
            {
                foreach (var o in g.GetComponents<Component>().Cast<Object>().Concat2(g))
                    actionsAddRange(typeData.Get(o.GetType()).Select(a => a.Clone("target", o.GetType())));
            }
            else if(target !=null) 
                actionsAddRange(typeData.Get(target.GetType()).Select(a => a.Clone("target")));

            if (trigger?.name != null)
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

        
        if (trigger != null && bs.HasChanged(ToolBar(actions, action, "Action", a => a.path), ref action)) //Refresh aciton parameters
        {
            if (action?.targetType != null && target is GameObject o && action.targetType != typeof(GameObject))
                target = o.GetComponent(action.targetType);
            GenerateCode();
            #if game
            CompileCodeStart(false,true);
#endif
        }

        if (action != null)
            foreach (var exp in exposedParams)
                exp.value=DrawObject(exp.value, exp.name, exp.type);
        
        using (GuiEnabled(customCode))
            if (HasChanged(GUILayout.TextArea(code), ref code))
                SetDirty();

        customCode = GUILayout.Toggle(customCode, "custom code");
        if(!Application.isPlaying)
            if (HasChanged(GUILayout.Toggle(showAll, "Show all functions"), ref showAll))
                actions = new List<SerializedMember>();
        masterOnly = GUILayout.Toggle(masterOnly, "Execute Host Only");

        if (GUILayout.Button("Compile"))
        {
            GenerateCode();
            #if game
            CompileCodeStart(false,true);
            Sync();
#endif
        }
        if (exception != null)
            LabelError(exception.Message);

    }
    public SerializedMember trigger;
    
    private List<SerializedMember> actions = new List<SerializedMember>();
    public Object target;

    // public List<SerializedValue> actionParameters = new  List<SerializedValue>();
    
    [FormerlySerializedAs("actionParameters")] 
    public List<SerializedValue> exposedParams = new  List<SerializedValue>();
    public SerializedMember action;


    

    public static object DefaultValue(Type t)
    {
        
        return t == typeof(string) ? "" : t.IsValueType ? Activator.CreateInstance(t) : null;
    }



    public static string GetTrueName(Type t)
    {
        var name = t.Name;
        return name == "String" ? "string" : name == "Int32" ? "int" : name == "Single" ? "float" : name == "Boolean" ? "bool" : name;
    }

    public static string MethodName(SerializedMember info)
    {
        
        var p = info.name + "(";
        var parameters = info.GetParameters();
        foreach (var item in parameters)
            p += $"{GetTrueName(item.ParameterType)} {item.Name}, ";
        return p.Trim(' ', ',') + ")"; 
    }
    public bool masterOnly;
    // [ContextMenu("Refresh Code")]
    private void GenerateCode()
    {
        try
        {
            if (customCode||action == null||string.IsNullOrEmpty(action.name)) return;
            
            StringBuilder sb = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();
            foreach (var a in action.parameters)
            {
                sb.AppendLine(GetTrueName(a.type) + " " + a.Name + ";");
                sb2.Append(","+a.Name);
            }
            for (var i = 0; i < action.indexes.Count; i++)
                sb.AppendLine(GetTrueName(action.indexes[i].type) + " index" + i + ";");

            var path = string.Format(action.code, Enumerable.Range(0, action.indexes.Count).Select(a => "index" + a).Cast<object>().ToArray());
            
            if (trigger == null) return;
            code = $@"
Object target;
{sb}
void {trigger.DeclaringType.Name}{trigger.Prefix}{MethodName(trigger)}
{{  
    {path}.{action.name}({sb2.ToString().Substring(Mathf.Min(sb.Length,1))});
}}
";
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    public Exception exception;
    
#if game
    
    
    [ContextMenu("Start")]
    private void OnEnable()
    {
        CompileCodeStart(true);
    }
    private void OnDisable()
    {
        if (Application.isPlaying)
            CompileCodeStart(false);
    }
    private HybInstance instance;
    private Dictionary<object, Hook> hooks = new Dictionary<object, Hook>();
    private static Dictionary<Hook, UnityAction> hook2Action = new Dictionary<Hook, UnityAction>();
    private void CompileCodeStart(bool add,bool refreshExposed=false)
    {
        try
        {
            CompileCodeStart2(add,refreshExposed);
        }
        catch (Exception e)
        {
            exception = e?.InnerException??e;
            throw;
        }
    }
    // public bool showTarget = true;
    private void CompileCodeStart2(bool add,bool refreshExposed=false) //2do optimize - script storage, returns non static class 
    {
        var Runner = ScriptPool.GetClass("class cs:bs { public  Object caller;public  object[] prms;  " + code + "}");
        instance = Runner.GetTypes()[0].Override(Runner.Runner, new HybInstance[0],this);
        // instance = HybInstance.Object(this);
        
            
        
        var methods = Runner.GetTypes()[0].InterpretKlass.GetMethods();
        var fields = Runner.GetTypes()[0].InterpretKlass.GetFields().Where(a => a.Id != "caller" && a.Id != "prms" && a.Id != "target").ToArray();
        if (refreshExposed)
        {
            var old = exposedParams.ToArray();
            exposedParams.Clear();
            // showTarget = false;
            for (var i = 0; i < fields.Length; i++)
            {
                var serializedValue = new SerializedValue(fields[i].GetValue(instance).InnerObject) { name = fields[i].Id, type = fields[i].FieldType.CompiledType };
                if (i < old.Length && serializedValue.type == old[i].type)
                    serializedValue.value = old[i].value;
                exposedParams.Add(serializedValue);
            }
        }
        foreach (object comp in trigableObjects)
        {
            foreach (var  m in methods)
            {
                Type type = (comp as Type ?? comp.GetType());
                

                FieldInfo f = type.GetField(m.Id.StartsWith(type.Name) ? m.Id.Substring(type.Name.Length) : "_" +m.Id, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                if (f != null)
                {
                    
                    object hk = f.GetValue(comp);

                    var key = (comp,f);

                    hooks.TryGetValue(key, out Hook oldhook2);
                        
                    var newHook=hooks[key] = new Hook(delegate(object[] prms)
                        {

                            instance.SetPropertyOrField("caller", HybInstance.Object(comp));
                            instance.SetPropertyOrField("prms", HybInstance.ObjectArray(exposedParams.Select(b => b.value).ToArray()));//old 
                            instance.SetPropertyOrField("target", HybInstance.Object(target));
                            foreach (var a in exposedParams)
                                instance.SetPropertyOrField(a.name, HybInstance.Object(a.value));
                            
                            // {
                            //     instance.SetPropertyOrField("prms", HybInstance.ObjectArray(exposedParams.Select(b => b.value).ToArray())); //2do add parameters expose instead    
                            // }
                            // instance.SetPropertyOrField("prms", HybInstance.ObjectArray(exposedParams.Select(b => b.value).ToArray())); //2do add parameters expose instead
                            try
                            {
                                
                                var d = m.Invoke(instance, prms.Select(HybInstance.Object).ToArray());
                                if (d?.InnerObject is SSEnumerator ss)
                                    StartCoroutine(ss);
                            }
                            catch (Exception e)
                            {
                                Debug.LogException(exception = e);
                            }
                        });


                    add = add && (!Application.isPlaying || !masterOnly || isMaster);
                    
                    
                    
                    if (hk is UnityEvent u)
                    {
                        if (oldhook2!=null && hook2Action.TryGetValue(oldhook2, out UnityAction aa))
                            u.RemoveListener(aa);

                        hook2Action[newHook] = aa = (() => newHook());
                        if (add)
                            u.AddListener(aa);
                    }
                    else
                    {
                        var h = (Hook)hk;
                        h -= oldhook2;
                        if (add)
                            h += newHook;
                        f.SetValue(comp, h);    
                    }

                    
                }
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
    public bool customCode;
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