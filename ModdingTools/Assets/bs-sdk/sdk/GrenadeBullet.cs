using UnityEngine;
using System.Collections;
using System.Linq;
using EnumsNET;

public class GrenadeBullet : BulletBase,ISetLife,IOnPlayerEnter
{
#if game
    
    internal float timeStart;
    
    internal new Grenade wep=>base.gun as Grenade;
    [PunRPC]
    public override void Explode()
    {
        foreach (Destructable g in GetInstances<Destructable>())
            if ((g.pos - pos).magnitude < range)
            {
                var gToPl = (g.pos - pos);
                g.RPCDamageAddLife(-gun.damage*(1f - gToPl.magnitude / range / 2));
            }
        
        base.Explode();
    }
    public void FixedUpdate()
    {
        if (Physics.Raycast(pos, rigidbody.velocity, out var h, rigidbody.velocity.magnitude*Time.deltaTime, Layer.levelMask))
        {
            var glass = h.collider.GetComponent<Glass>();
            if (glass)
                glass.Die();
            else if (wep.explodeOnCollision)
            {
                normal = h.normal;
                Explode();
            }
        }
    }
    //int frame;
    public virtual void Update()
    {

        //if (frame++ == 3)

        foreach (Transform t in transform)
            t.localPosition = Vector3.MoveTowards(t.localPosition, Vector3.zero, TimeCached.deltaTime * 1); //lerp from hands to actual pos

        if (Time.time - timeStart > wep?.time && timeStart != 0)
        {
            Explode();
            timeStart = 0;
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        PlayOneShot(wep.grenadeHit, .3f);
        normal = collision.GetContact(0).normal;
    }
    
    public override void OnDestroy()
    {
        PhotonNetwork.UnAllocateViewID(GetComponent<PhotonView>().viewID);
        base.OnDestroy();
    }
    public override void OnReset()
    {
        Destroy(gameObject);
        base.OnReset();
    }
    public void RPCDamageAddLife(float damage, Player pv = null, GunBase weapon = null, HumanBodyBones colliderId = 0, Vector3 hitPos = default)
    {
        if (pv?.IsMine==true)
            CallRPC(Explode);
    }
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        if (Time.time - timeStart > 1)
            if ((settings.mpVersion < 18 || IsMine) && mine && wep.pl.IsEnemy(pl))
            {
                CallRPC(Explode);
            }
    }
#endif
    public bool mine;
  
    
}
