




using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
// ReSharper disable IteratorMethodResultIsIgnored
public class Knife : GunBase,IMeleeWeapon
{
    
    [Serializable]
    public class Set
    {
        public ObscuredFloat shootInterval = 0.0955f;
        public float audioDelay = 0;
        public ObscuredFloat damageDelay = 0;
        public ObscuredFloat range = 1;
        public ObscuredInt damage = 60;
        public float progress=.4f;
        public AudioClip2 attackSound;
        public int interationsCount = 1;

        //public HumanBodyBones weaponBone = HumanBodyBones.LastBone;
    }
    [Header("       knife settings")]
    public Set primary;
    public Set secondary;
    public AudioClip2 hitWall2;
    public AudioClip2 hitSound2;
#if game
    private float lastAttack;
    public bool dontMoveWhenAttacking;
    public override void Update2()
    {
        base.Update2();
        var shooting2 = !(Time.time - lastAttack > 0);
        if(dontMoveWhenAttacking && shooting2)
            pl.StopMove();

        if (IsMine && !_Game.preMatch && pl.alive)
        {
            if ((Input2.GetMouseButton(1) || MouseButton) && !pl.knocked && !shooting2 && !isReloadingOrDraw )
                CallRPC2(Stab, !MouseButton);
        }
    }
    // public override bool MouseButton { get { return Input2.InputGetKey(KeyCode.Mouse0); } }

    // private HumanBodyBones lastBp;
    [PunRPC]
    public IEnumerator Stab(bool second)
    {

        var set = second ? secondary : primary;


        lastAttack = Time.time + set.shootInterval;
        PlayAnimation(second ? Anims.shoot2 : Anims.shoot, .0f, true, 1);
        yield return new WaitForSeconds(set.audioDelay);
        pl.weaponAnimationEventsAudio.PlayOneShot(set.attackSound);
        yield return new WaitForSeconds(set.damageDelay-set.audioDelay);

        Vector3 forward = cam.forward; // + Vector3.down * .3f;

        // if (pl.skin.bodyToTransform.TryGetValue(set.weaponBone, out var t))
        Transform trigger = gunSkin?.sword??pl.skin?.bodyWeapon;

        // if (pl.skin.bodyWeapon!=null)
            // trigger = pl.skin.bodyWeapon;
        
        if (trigger)
        {
            Collider[] cc = Physics.OverlapBox(trigger.position, trigger.lossyScale / 2, trigger.rotation, 1<<Layer.playerTrigger, QueryTriggerInteraction.Collide);

            foreach (var c in cc)
            {
                if (c.GetName() == "Trigger")
                {
                    var enemy = c.GetComponentInParent<Player>();
                    if (enemy != null && enemy != pl)
                    {
                        if (IsMine)
                            enemy.RPCDamageAddLife(-set.damage, pl.viewId, id, Random.value<.3f?HumanBodyBones.Head  :HumanBodyBones.Chest);
                        // h.point = enemy.controller.ClosestPoint(pl.Cam.pos);
                        // h.normal = (pl.Cam.pos - h.point).normalized;
                        // enemy.CreateBloodParticles(h);
                        enemy.PlayOneShot(hitSound2);
                    }
                }
            }
            
            // yield break;
        }
        
        bool createOnce = false; 
        for (int i = 0; i < set.interationsCount; i++)
        {
            var damage = set.damage/(float)set.interationsCount;
            
            foreach (var v in new[] {forward, forward + cam.right * .3f, forward + cam.right * -.3f})
            {
                var origin = pl.hpos;
                var dist = set.range;
                foreach (RaycastHit h in Physics.RaycastAll(origin, v, dist, Layer.allmask,QueryTriggerInteraction.Ignore).OrderBy(a => a.distance))
                {
                    var transformRoot = h.transform.root;
                    PlayerSkin sk=null;
                    var enemy = transformRoot.GetComponent<Player>() ?? (sk= transformRoot.GetComponent<PlayerSkin>())?.pl;

                    if (sk && !enemy && !createOnce)
                    {
                        createOnce = true;
                        sk.CreateGib(sk.GetBodyPart(h.transform));
                        // _Game.EmitParticles(h.point, h.normal, _Game.res.bloodParticleBig.Random());
                        Bullet.CreateBlood(h,_Game.res.bloodParticle2.Random(),sk);                        
                        pl.PlayOneShot(hitSound2);
                        yield break;
                    }

                    if (pl.zombie) damage = (int)(damage * (1f - pl.zombiePowerUp / 2));
                    if (enemy != null)
                    {
                        if (!pl.IsEnemyOrBot(enemy) && !allowFriendlyFire)
                            continue;

                        lastHit = h;
                        // if (sk) Bullet.CreateBlood(h, _Game.res.bloodParticle2.Random(), sk);
                        
                        
                         pl.PlayOneShot(hitSound2);

                         if (IsMine)
                         {
                             enemy.RPCDamageAddLife(-(int)damage, pl.viewId, id, enemy.skin.GetBodyPart(h.transform));
                         }
                         continue;
                    }
                    var isetlife = h.transform.GetComponentInParent<ISetLife>();
                    if (isetlife != null && isetlife is Player == false)
                    {
                        if (IsMine)
                            isetlife.RPCDamageAddLife(-(int) damage, pl.viewId, id);
                    }
                    else
                    {
                        if (h.rigidbody && !h.rigidbody.isKinematic)
                            h.rigidbody.AddForce(Random.insideUnitSphere * 500);
                        else
                        {
                            _Game.EmitParticles(h.point, h.normal + Vector3.up, _Game.res.concerneParticle);
                        }

                        if (i == 0) pl.PlayOneShot(hitWall2);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(set.progress/set.interationsCount);
        }
   


    }
    public override bool isShooting { get { return Time.time < lastAttack; } }

    IEnumerable<RaycastHit> Raycast(float dist, Vector3[] vs)
    {
        foreach (var v in vs)
            foreach (var a in Physics.RaycastAll(pl.hpos, v, dist, Layer.allmask).OrderBy(a => a.distance))
                yield return a;
    }
    public override void ParseAnimEditor(Anims anim, AnimationClip clip)
    {
        if (anim == Anims.shoot)
        {
            primary.shootInterval = clip.length * .7f;
            primary.damageDelay = primary.shootInterval / 3;
        }
        if (anim == Anims.shoot2)
        {
            secondary.shootInterval = clip.length;
            secondary.damageDelay = primary.shootInterval / 3;
        }

        base.ParseAnimEditor(anim, clip);
    }

#endif
    public float range => primary.range;
}