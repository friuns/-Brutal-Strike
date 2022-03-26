#if UNITY_EDITOR && game
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Cysharp.Threading.Tasks;
// using UnityDropdown.Editor;
// using UnityDropdown.Editor;
// using Cysharp.Threading.Tasks;
// using TNRD.ExtendedEvent;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class TriggerEvent : Trigger,IOnPlayerEnter,IOnPlayerNear,IOnPlayerStay
{
    

    private List<MethodInfo> m_methodInfos;
    private List<MethodInfo> methodInfos
    {
        get
        {
            if (m_methodInfos == null)
            {
                m_methodInfos = new List<MethodInfo>();
                foreach (var a in GetComponents<Base>())
                    GetMethods(a.GetType(), m_methodInfos, typeof(TriggerAttribute));
            }
            return m_methodInfos;
        }
    }
    private List<ExtendedEvent2> parameters = new List<ExtendedEvent2>();
    public override void Awake()
    {
        base.Awake();
        OnValidate();
    }
    public override void OnValidate()
    {
        base.OnValidate();
        if (triggerMethod != null)
        {
            var infos = triggerMethod.GetParameters().Where(a => !a.ParameterType.IsValueType).Select(a => new ExtendedEvent2(a)).ToList();
            infos.Add(new ExtendedEvent2(() => bs._Game));
            infos.Add(new ExtendedEvent2(() => bs._Player));
            infos.Add(new ExtendedEvent2(() => bs._Hud));
            parameters = infos;
        }
    }

    public MethodInfo triggerMethod { get { return triggerIndex == -1 ? null : methodInfos.ArrayGetValue(triggerIndex); } set { triggerIndex = methodInfos.IndexOf(value); } }
    
    public int triggerIndex=-1;
    public ExtendedEvent2 actionParametr{ get { return actionParametrIndex == -1 ? null : parameters[actionParametrIndex]; } set { actionParametrIndex = parameters.IndexOf(value); } }
    
    public int actionParametrIndex=-1;
    public MethodInfo actionMethod;
    public ExtendedEvent2 action = new ExtendedEvent2();
    
    public override void OnInspectorGUI()
    {
        // var d = new DropdownItem<int>(4,"");
        base.OnInspectorGUI();

        if (bs.HasChanged(ToolBar(methodInfos, triggerIndex, "Trigger", a => a.DeclaringType + "/" + a), ref triggerIndex))
        {
            OnValidate();
       
        }

        if (triggerMethod != null)
        {
            List<ExtendedEvent2> prms = parameters;
            
            
            // GUILayout.Label("condition");
            if (bs.HasChanged(ToolBar(prms, actionParametrIndex, "Actor"), ref actionParametrIndex))
            {
                action = actionParametr;
            }
        }
        if(action!=null)
            action.OnInspectorGUI();
            
        

    }
    
    
    public void Invoke(string s, params object[] o)
    {
        action.Invoke(o[actionParametrIndex]);

    }

    [Trigger]
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        Invoke("OnPlayerEnter", pl, other, b);
    }
    [Trigger]
    public void OnPlayerNearEnter(Player pl, bool enter)
    {
        Invoke("OnPlayerNearEnter", pl,enter);
    }
    
    [Trigger]
    public void OnPlayerStay(Player pl, Trigger other)
    {
        Invoke("OnPlayerStay", pl, other);
    }

    public static int ToolBar<T>(IList<T> ts, int t, string label,Func<T,string> select=null)
    {
        return ts.IndexOf(ToolBar(ts, t == -1 ? default : ts[t], label, select));
    }
    public static T ToolBar<T>(IList<T> ts,T t, string label,Func<T,string> select=null)
    {
        var enumerable = ts.Select(select ?? (a => a.ToString())).ToList();
        enumerable.Insert(0, "None");
        var index = EditorGUILayout.Popup(label, ts.IndexOf(t) + 1, enumerable.ToArray()) - 1;
        if (index != -1)
            return ts[index];
        return default;
    }
    
  
}

#endif






