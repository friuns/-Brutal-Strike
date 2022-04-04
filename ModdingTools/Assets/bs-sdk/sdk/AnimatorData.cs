using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Serialization;

public class AnimatorData:bs
{
    public static AnimatorData i;
    public override void Awake()
    {
        i = Resources.FindObjectsOfTypeAll<AnimatorData>().First(a => !a.gameObject.scene.IsValid());
        base.Awake();
    }
    [Serializable]
    public class TransitionsDict:SerializableDictionary<int, float>{}
    
    public TransitionsDict trdict = new TransitionsDict();
    private static int _id;
    public float GetTransition(int id, Animator an)
    {
        _id = id;

        if (!trdict.TryGetValue(id, out var f))
        {
            #if UNITY_EDITOR
            Fill(an);
            #endif
        }
        // else
            // Debug.Log("Found " + f);
        return f;
    }
#if UNITY_EDITOR
    
    void Fill(Animator animator)
    {
        var arc = animator.runtimeAnimatorController;
        var ac = arc is AnimatorOverrideController aoc ?(AnimatorController)  aoc.runtimeAnimatorController : (AnimatorController) arc;

        foreach (var b in ac.layers)
        {
            RecAdd(b.stateMachine);
        }
            
    }
    private void RecAdd(AnimatorStateMachine stateMachine)
    {
        foreach (var state in stateMachine.states)
        {
            foreach (AnimatorStateTransition tr in state.state.transitions)
                if (tr.destinationState != null)
                {
                    // if (trdict.TryGetValue(tr.destinationState.nameHash, out var f) && f > 0)
                    trdict[tr.destinationState.nameHash] = tr.offset;
                }
        }
        foreach (ChildAnimatorStateMachine a in stateMachine.stateMachines)
            RecAdd(a.stateMachine);
    }
    #endif
}