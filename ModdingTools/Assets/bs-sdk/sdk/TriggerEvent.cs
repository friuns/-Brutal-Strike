#if UNITY_EDITOR && game
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// using Cysharp.Threading.Tasks;
// using TNRD.ExtendedEvent;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


public class TriggerEvent : Trigger,IOnTriggerEnter,IOnPlayerEnter,IOnPlayerNear,IOnPlayerStay
{
    private MethodInfo trigger;
    private ParameterInfo actionParametr;
    public MethodInfo actionMethod;
    public ExtendedEvent ext;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var methodInfos = GetMethods(GetType());

        trigger = ToolBar(methodInfos, trigger, "Trigger");
        
        if (trigger != null)
        {
            var prms = trigger.GetParameters();
            GUILayout.Label("condition");
            GUILayout.Label("action");
            
            actionParametr = ToolBar(prms, actionParametr, "actionParametr");
            if (actionParametr != null)
            {
                actionMethod = ToolBar(GetMethods(actionParametr.ParameterType), actionMethod, "Actor");
                if (actionMethod != null)
                {
                    foreach (ParameterInfo par in actionMethod.GetParameters())
                    {
                        EditorGUILayout.ObjectField(par.Name, null, par.ParameterType);

                    }
                }
                

            }



        }

    }
    private List<MethodInfo> GetMethods(Type type)
    {
        
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(a=>a.DeclaringType == GetType()).ToList();
    }


    public void OnTriggerEnterOrExit(Trigger _this, Trigger other, bool enter)
    {
        
    }
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        
    }
    public void OnPlayerNearEnter(Player pl, bool enter)
    {
    }
    public void OnPlayerStay(Player pl, Trigger other)
    {
    }
    public static T ToolBar<T>(IList<T> ts,T t, string label)
    {
        var index = EditorGUILayout.Popup(label, ts.IndexOf(t), ts.Select(a => a.ToString()).ToArray());
        if (index != -1)
            return ts[index];
        return default;
    }
  
}

#endif