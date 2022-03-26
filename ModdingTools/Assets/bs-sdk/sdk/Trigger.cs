using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// [DisallowMultipleComponent]
// [RequireComponent(typeof(Collider))]
public class Trigger : TriggerBase
{
    public Base handler;
    public void Reset()
    {
        handler = GetComponentsInParent<ITriggerEvent>(true).FirstOrDefault() as Base;
    }
    #if game
    internal new string tag;
    public new bool CompareTag(string other)// can access when removed
    {
        return tag == other;
    }
    public override void Awake()
    {
        base.Awake();
        tag = base.tag;
        
//        gameObject.layer = this is TriggerReceiver ? Layer.playerTrigger : Layer.trigger;
    }
    public virtual void Start()
    {
        (handler as IOnTriggerInit)?.InitTrigger(this);
    }
    
    public virtual void OnValidate()
    {
        if (Application.isPlaying || string.IsNullOrEmpty(gameObject.scene.name)) return;
        if (!(handler is ITriggerEvent) && !bs.isGame)
        {
            Debug.LogError("handler missing", gameObject);
            Reset();
        }
    }
    // internal HashSet<Trigger> triggers = new HashSet<Trigger>();
    internal FastList2<Trigger> triggers = new FastList2<Trigger>(); //todo remove may be expensive
     
    
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        GUILayout.Label("triggers "+triggers.Count);
        foreach (var a in triggers)
            UnityEditor.EditorGUILayout.ObjectField("", a, a.GetType(), true);
        base.OnInspectorGUI();
    }
#endif
    
    
    public override void OnDisable()
    {
        if (bs.exiting) return;
        foreach (Trigger other in triggers)
            if (other is TriggerReceiver r)
                r.OnTriggerExit2(this);
    }


    public virtual void Clear()
    {
    }
#endif
}