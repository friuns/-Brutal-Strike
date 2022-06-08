using ChobiAssets.PTM;
using UnityEngine;
using UnityEngine.Playables;

namespace BattleRoyale
{
public partial class CarBase
{
    public GameObject smokeEffect;
    public GameObject explosionEffect;
    public float damageNitro = .01f;
 #if game   

   
    
    bool dead { get { return life < 0; } }
    
    [ContextMenu("DieTEst")]
    public void DieTest()
    {
        SetLife(-1, -1, -1);
    }
    internal float nitro = 0;
    public override void OnPlConnected(PhotonPlayer photonPlayer)
    {
        base.OnPlConnected(photonPlayer);
        CallRPC(SetLife, life, -1, -1);
    }

    
    [PunRPC]
    public virtual void SetLife(float nwLife, int plId, int weaponID)
    {
        if (dead) return;
        
        var enemy = ToObject<Player>(plId);
        var weapon = ToObject<GunBase>(weaponID);
        
        
        var damage = life - nwLife;

        damage = Mathf.Max(0, damage - armor / weapon?.WeaponArmorRatio ?? 3);
        
        if(enemy?.IsMine == true)
            _Hud.CenterTextDamageDeal("car damage " + (int)(damage+1));
        
        
        if (damage > 0)
        {
            foreach (var a in pls)
            {
                a.killedBy = enemy;
                _ObsCamera.shakeValue = Mathf.Min(2, damage * .1f);
                _ObsCamera.shakeTime = Mathf.Min(.1f, damage * .1f);
                a.HitTime = Time.time;
            }
            
            nitro += damageNitro * .1f * damage;
            if (enemy?.vehicle is Car plcar)
                plcar.nitro += damageNitro * damage;
        }

        life = nwLife;
        if (life < defLife *.4f)
            smokeEffect.SetActive3(true);
        
        if (dead)
        {
            Die();
        }
    }
    public virtual void Die()
    {
        InitRespawn();
        explosionEffect.SetActive3(true);
        foreach (var a in pls.ToArray())
        {
            if (a.gesture.IsValid())
                a.gesture.Destroy();
            if(roomSettings.version< 2719) 
                a.SetLife(-1, -1, -1, HumanBodyBones.Chest); //may desync if player leaves first
            else
                a.SetVehicle(-1);
            
        }
    }
    public void RPCDamageAddLife(float damage, Player pv = null, GunBase weapon = null, HumanBodyBones colliderId = 0, Vector3 hitPos = default)
    {
        if (pv == null || pv.IsMine)
            CallRPC(SetLife, life + damage, pv?.viewId ?? -1, weapon?.viewId ?? -1);    
    }
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        if (plOwner != null && plOwner.IsMine && pl.vehicle == null && pl.IsEnemyOrBot(plOwner) && !pl.onPlatform)
        {
            // var sk = pl.skin;
            if (rigidbody.velocity.magnitude > 10)
            {
                pl.CallRPC(pl.SetVeloticy, rigidbody.velocity + Vector3.up);
                pl.RPCDamageAddLife(-99999,plOwner);
            }
            // sk.rigidbodies.ForEach(a => a.velocity = rigidbody.velocity + Vector3.up);
        }
    }
    
 #endif   
}
}