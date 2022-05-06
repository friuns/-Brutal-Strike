using UnityEngine;
using UnityEngine.Playables;

namespace BattleRoyale
{
public partial class Car
{
    public GameObject smokeEffect;
    public GameObject explosionEffect;
    public float damageNitro = .01f;
 #if game   

    private void ProcessImpact()
    {
        Vector3 contactForce = Vector3.zero;
        if (vehicleController.sumImpactVelocity.sqrMagnitude > damageMinForce * damageMinForce)
            contactForce = tr.TransformDirection(vehicleController.sumImpactVelocity) * damageMultiplier * 0.2f;
        else if (vehicleController.localDragVelocity.sqrMagnitude > damageMinForce * damageMinForce)
            contactForce = tr.TransformDirection(vehicleController.localDragVelocity) * damageMultiplier * 0.01f;
        if (contactForce.sqrMagnitude > 0.0f)
        {
            Vector3 contactPoint = tr.TransformPoint(vehicleController.sumImpactPosition);

            // contactForce = contactForce * 10 / Mathf.Max(3, rigidbody.velocity.magnitude);
            
            var damage = contactForce.sqrMagnitude;
            var enemyCar = vehicleController.car.Value;
            Player enemyPl = enemyCar?.plOwner;
            if (enemyPl != null)
            {
                if (plOwner?.IsEnemyOrBot(enemyPl) == false)
                    return;

                damage *= Mathf.Min(4, (enemyCar.minVel.minValue + 1) / (minVel.minValue + 1))*5;
            }


            if (/*enemyPl?.IsMine == true ||*/ plOwner?.IsMine == true)
            {
                
                RPCDamageAddLife(-damage, enemyPl);
                _Game.EmitParticles(contactPoint, -contactForce, _Game.res.spark, maxParticlesPerSecond: 10);
            }

        }
        
    }
    
    bool dead { get { return life < 0; } }
    
    [ContextMenu("DieTEst")]
    public void DieTest()
    {
        SetLife(-1,-1);
    }
    [PunRPC]
    public void SetLife(float nwLife,int plId)
    {
        if (dead) return;
        
        var enemy = ToObject<Player>(plId);
        
        var damage = life - nwLife;
        
        if(enemy?.IsMine == true)
            _Hud.CenterTextDamageDeal("car damage " + (int)damage+1);
        
        if (enemy)
            foreach (var a in pls)
                a.killedBy = enemy;

        if (damage > 0)
        {
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
    private void Die()
    {
        explosionEffect.SetActive3(true);
        foreach (var a in pls.ToArray())
        {
            if (a.gesture.IsValid())
                a.gesture.Destroy();
            a.SetLife(-1, -1, -1, HumanBodyBones.Chest);
            
        }
    }
    public void RPCDamageAddLife(float damage, Player pv = null, GunBase weapon = null, HumanBodyBones colliderId = 0, Vector3 hitPos = default)
    {
        CallRPC(SetLife, life + damage, pv?.viewId ?? -1);    
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