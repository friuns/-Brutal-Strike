using System;
using UnityEngine;
using UnityEngine.Serialization;

public class WeaponBase : GunBase
{
    public AudioClip2 SoundExpl2;
    // [FormerlySerializedAs("SoundExpl2")]
    public AudioClip2 bulletric;

    public Transform explosionParticle;
    

    [FieldAtrStart(inherit = true)]
    
    public bool explodeOnCollision;
    public Int32 damage = 26;
    public float explodeForce=1;
    public float range = 10;
    // public AudioClip2 bulletric;
    [Tooltip("bullet passing through wall in meter/grenade explosion pass")]
    public float bulletPass=1;
    #if game
    public override void Start()
    {
        if (!explosionParticle)
            explosionParticle = _Game.res.concerneParticle;
        if (SoundExpl2.hasSound)
            bulletric = SoundExpl2;
        base.Start();
    }
    #endif
}