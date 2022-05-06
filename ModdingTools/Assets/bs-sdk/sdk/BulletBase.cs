using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BulletBase : ObjectBase
{
    
    public Transform explosion;
#if game
    internal Vector3 normal = Vector3.up;
    internal RaycastHit hit;

    public Hook _Explode;
    [PunRPC] 
    public virtual void Explode() //Explode setlife happens on enemy side, unlike bullets 
    {
        try
        {
            _Explode?.Invoke();
            var pos = this.pos;
            foreach (var pl in _Game.playersAll)
            {
                var gToPl = (pl.pos - pos);
                if (pl.IsMine && !pl.dead && gToPl.magnitude < range && (pl.IsEnemy(gun.pl) || pl.bot || pl == gun.pl))
                {

                    float f = 1;
                    if (RayCastLine(pl.hpos, pos + Vector3.up * .2f, out var h1,Layer.levelBoundsMask) && RayCastLine(pos + Vector3.up * .2f, pl.hpos, out var h2,Layer.levelBoundsMask))
                        f = 1f - Mathf.InverseLerp(0, gun.bulletPass * gameSettings.bulletPassFactor, Bullet.minThikness+(h1.point - h2.point).magnitude);
                    f *= f;
                    var damage = gun.damage * f * (1f - gToPl.magnitude / range / 2);
                    
                    if (pl == gun.pl) damage *= .5f;
                    
                    if (pl.bot) damage *= 1.5f;
                    if (damage > gun.damage * .2f)
                    {
                        Debug2.DrawLine(pos, pl.hpos, Color.red, 5f);
                        var armor = (Armor) pl.guns.FirstOrDefault(a => a.have && a is Armor _a && _a.body.Contains(HumanBodyBones.Chest));
                        if (armor && !pl.bot)
                            damage = armor.RPCDamage(damage, gun);

                        if (pl.iMoveController is QSurfController)
                            pl.iMoveController.veloticy += AddExplosionForce(gToPl + Vector3.up, gun.range, gun.explodeForce );
                        else
                            pl.iMoveController.veloticy += (Vector3.ClampMagnitude(gToPl+Vector3.up, 1) ) * damage * .2f * gun.explodeForce;
                        // pl.SetGrounded(false);
                        
                        SetDamage(pl, damage);
                        
                        
                    }
                }
            }
            
            _Game.EmitParticles(this.pos, normal , gun.explosionParticle);
            _Game.ExplodePhysics(this.pos, 2000 * gun.explodeForce);
            transform.position = pos;
            PlayClipAtPoint(gun.bulletric, pos, 10, mixer: _Player.WeaponAudioSource.outputAudioMixerGroup);
            
            // if (this is GrenadeBullet && Physics.Raycast(pos, Vector3.down, out RaycastHit h, 1, Layer.levelMask))
                // Destroy(InstantiateAddRot(gameRes.ExpDecal, h.point + Vector3.up * .001f, Quaternion.LookRotation(h.normal)), 20);
        }
        finally
        {
            Destroy(gameObject);
        }
    }
    public virtual void SetDamage(Player pl, float damage)
    {
        if (pl.observing && damage > 5)
            _ObsCamera.PlayExplodeAnim();
        
        pl.RPCDamageAddLife(-(int) damage, gun.pl, gun, Random.value > .5 ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg); //
    }

    public static Vector3  AddExplosionForce(  Vector3 explosionVec, float explosionRadius, float explosionForce )
    {
        float distance = explosionVec.magnitude; // get distance to explosion
    
        if( distance > explosionRadius ) return Vector3.zero; // we're outside the explosion's radius, ignore it
    
        float forceMult = 1f - ( distance / explosionRadius ); // 1.0 at explosion center, 0.0 at explosion radius
    
        explosionVec /= distance; // normalize the vector
        return explosionVec * explosionForce * forceMult; // add explosion impulse to velocity
    }
    
    public float range { get { return gun.range; } }
    internal WeaponBase gun;


#endif
}