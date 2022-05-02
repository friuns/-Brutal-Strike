using UnityEngine;
using UnityEngine.Playables;

namespace BattleRoyale
{
public partial class Car
{
 #if game   
    
    private void ProcessImpact()
    {
        Vector3 contactForce = Vector3.zero;
        if (vehicleController.sumImpactVelocity.sqrMagnitude > minForce * minForce)
            contactForce = tr.TransformDirection(vehicleController.sumImpactVelocity) * multiplier * 0.2f;
        else if (vehicleController.localDragVelocity.sqrMagnitude > minForce * minForce)
            contactForce = tr.TransformDirection(vehicleController.localDragVelocity) * multiplier * 0.01f;
        if (contactForce.sqrMagnitude > 0.0f)
        {
            Vector3 contactPoint = tr.TransformPoint(vehicleController.sumImpactPosition);

            // contactForce = contactForce * 10 / Mathf.Max(3, rigidbody.velocity.magnitude);
            
            var damage = contactForce.sqrMagnitude;
            var enemyCar = vehicleController.car;
            Player enemyPl = enemyCar?.ownerPl;
            if (enemyPl != null)
            {
                if (ownerPl?.IsEnemyOrBot(enemyPl) == false)
                    return;

                damage *= Mathf.Min(4, (enemyCar.minVel.minValue + 1) / (minVel.minValue + 1));
            }


            if (enemyPl?.IsMine == true || ownerPl?.IsMine == true)
            {
                if(enemyPl?.IsMine == true)
                    _Hud.CenterTextDamageDeal("car damage " + damage);
                AddLife(-damage);
                _Game.EmitParticles(contactPoint, -contactForce, _Game.res.spark, maxParticlesPerSecond: 10);
            }

        }
        
    }
    public void AddLife(float colliderDamage)
    {
        SetLife(life + colliderDamage);

    }
    bool dead { get { return life < 0; } }
    
    [ContextMenu("DieTEst")]
    public void DieTest()
    {
        SetLife(-1);
    }
    [PunRPC]
    public void SetLife(float nwLife)
    {
        if (dead) return;
        life = nwLife;
        if (dead)
        {
            Die();
        }
    }
    private void Die()
    {
        explosionEffect.SetActive(true);
        foreach (var a in pls.ToArray())
        {
            if (a.gesture.IsValid())
                a.gesture.Destroy();
            a.SetLife(-1, -1, -1, HumanBodyBones.Chest);
            
        }
    }
    public void RPCDamageAddLife(float damage, int pv = -1, int weapon = -1, HumanBodyBones colliderId = HumanBodyBones.Hips, Vector3 hitPos = default)
    {
        
    }
    public void OnPlayerEnter(Player pl, Trigger other, bool b)
    {
        if (pl.IsMine && pl.vehicle == null && ownerPl != null && pl.IsEnemyOrBot(ownerPl) && !pl.onPlatform)
        {
            // var sk = pl.skin;
            if (rigidbody.velocity.magnitude > 10)
            {
                pl.CallRPC(pl.SetVeloticy, rigidbody.velocity + Vector3.up);
                pl.RPCDie();
            }
            // sk.rigidbodies.ForEach(a => a.velocity = rigidbody.velocity + Vector3.up);
        }
    }
 #endif   
}
}