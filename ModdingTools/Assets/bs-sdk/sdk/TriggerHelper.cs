using System;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate void Hook(params object[] args);

public class TriggerHelper:Trigger,IOnPlayerEnter,IOnPlayerStay,ISetLife
{
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


    public bool minePlayerOnly; 
    protected Hook _OnActionKey;
    public void OnActionKey(Player pl)
    {
        _OnActionKey?.Invoke(pl);
    }
    public float updateInterval = 1;
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
    public void RPCDamageAddLife(float damage, int pv = -1, int weapon = -1, HumanBodyBones colliderId = HumanBodyBones.Hips, Vector3 hitPos = default)
    {
        
    }
    
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        if(b)
            OnPlayerEnter(pl,other);
        else
            OnPlayerExit(pl,other);
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


}

