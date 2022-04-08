using System;
using UnityEngine;
using Object = UnityEngine.Object;

public delegate void Hook(params object[] objects);
[RequireComponent(typeof(TriggerEvent))]
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
        _OnPlayerStay?.Invoke(pl);
        #if game
        if (_OnActionKey != null && (!minePlayerOnly || pl.IsMine) && pl.Input2.GetKeyDown(KeyCode.F, name))
            OnActionKey(pl);
        #endif
    }


    public bool minePlayerOnly; 
    protected Hook _OnActionKey;
    public void OnActionKey(Player pl)
    {
        _OnActionKey?.Invoke(pl);
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

