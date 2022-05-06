using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
public delegate void Hook(params object[] args);

// public class Hook
// {
//     public Hook(Action<object[]> action)
//     {
//         
//     }
//     
//     public Hook(UnityEvent<object[]> action)
//     {
//         
//     }
//     
//     public void Invoke(params object[] prms)
//     {
//         
//     }
//     public void Add(Hook value)
//     {
//         
//     }
// }

public class TriggerHelper:Trigger,IOnPlayerEnter,IOnPlayerStay,ISetLife
{
    public bool minePlayerOnly; 
    public float updateInterval = 1;
[Expose]
    public void KillPlayer(Player pl)
    {
        #if game
        pl.DieTest();
        #endif
    }
    
    
    protected Hook _OnPlayerStay;
    public void OnPlayerStay(Player pl, Trigger other)
    {
#if game
        if (TimeElapsed(updateInterval))
            _OnPlayerStay?.Invoke(pl);
        
        if (_OnActionKey != null && (!minePlayerOnly || pl.IsMine) && pl.Input2.GetKeyDown(KeyCode.F, GetComponentInParent<TriggerEvent>().itemName))
            OnActionKey(pl);
        #endif
    }


    
    protected Hook _OnActionKey;
    public void OnActionKey(Player pl)
    {
        _OnActionKey?.Invoke(pl);
    }
    protected Hook _Start;
    public override void Start()
    {
        base.Start();
        _Start?.Invoke();
    }
    protected Hook _Update;
    public void Update()
    {
        #if game
        if (TimeElapsed(updateInterval))
            _Update?.Invoke();
        #endif
    }
    protected Hook _AnimationEvent;
    public void AnimationEvent(string s)
    {
        _AnimationEvent?.Invoke(s);
    }
    public void RPCDamageAddLife(float damage, Player pv = null, GunBase weapon = null, HumanBodyBones colliderId = 0, Vector3 hitPos = default)
    {
        
    }
    
    
    protected Hook _OnPlayerEnter;
    public void OnPlayerEnter(Player pl, Trigger other)
    {
        _OnPlayerEnter?.Invoke(pl, other);
    }
    protected Hook _OnPlayerExit;
    public void OnPlayerExit(Player pl, Trigger other) 
    {
        _OnPlayerExit?.Invoke(pl, other);
    }
    
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        if(b)
            OnPlayerEnter(pl,other);
        else
            OnPlayerExit(pl,other);
    }

    


}

