









using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Input3 = Input2;
public interface IMeleeWeapon
{
    float range { get; }
}
public partial class Sword : GunBase,IMeleeWeapon
{
    
    

        
    public float damage;
    // public AudioClip2 blockSound;
    // public AudioClip2 hitSound;
    // public AudioClip2 dodge;
    public AudioClip2 missSound;
    public AudioClip2 swingSound;
    public float range => 2;
    [FormerlySerializedAs("effectPrefab")] 
    public GameObject defaultEffectPrefab;

    public float attackSensivity=10;
    
#if game
    private AnimatorStateInfo state;
    // private AnimatorClipInfo clip;

    private BoxCollider bx => gunSkin.bx;
    private bool idleState;
    // private float oldDF;
    // public KeyCode[] keysToSend {}
    public override void OnSelectGun(bool selected)
    {
        base.OnSelectGun(selected);
        foreach (var a in Input3.keys)
            a.forceDisableToggle = true;
        if (selected)
        {
            animator = pl.skin.animator;
            animator.Component<ForwardEvents>().instantiateEffect = OnAnimationEvent;
            // animator.enabled = !pl.IsMainPlayer;

        }
        
    }
    private float lastTimeUsed;
    public override void Update2()
    {
        base.Update2();
        Input2.GetKey(KeyCode.Mouse3);
        Input2.GetKey(KeyCode.Mouse4);
        Input2.GetKey(KeyCode.Mouse1);

        animator = pl.skin.animator;
        
        
        // animator.fireEvents = false;

        AnimatorStateInfo oldState = state;
        var ti = animator.GetAnimatorTransitionInfo(0);
        state = pl.animationState;
        
        foreach (var a in animator.parameters)
        {
            if (a.name.EndsWith("Tag"))
                animator.SetBool(a.nameHash, state.tagHash == a.nameHash);
        }
        
        // Debug.Log(state.normalizedTime;)
        
        var oldIdle = idleState;
        idleState =  state.IsTag("idle");
        
        if (oldIdle && !idleState && IsMine)
            pl.skin.forward = ZeroY(pl.Cam.forward);

        if (Time.time - pl.lastKeyTime > .5f)
            UnsetLastKey();
            
        if (state.fullPathHash != oldState.fullPathHash || oldState.normalizedTime>state.normalizedTime)
        {
            landed.Clear();
            pl.PlayOneShot(pl.playerClassPrefab.walkSound);
            // if (lastKey != -1 && pl.InputGetKey((KeyCode) lastKey))
                // animator.SetInteger(AnimParams.lastKey.GetName(), -1);

            specialsColdown.TryGetValue(state.fullPathHash, out  lastTimeUsed);
            specialsColdown[state.fullPathHash] = Time.time;
            
            
            if (IsMine)
            {
                var fx = ti.durationUnit == DurationUnit.Fixed;
                var transitionTime = AnimatorData.i.GetTransition(state.shortNameHash, animator);
                if (state.length == 0)
                    Debug.LogError("zero detected");
                pl.RPCPlayAnim(state.shortNameHash, fx ? ti.duration : ti.duration / state.length, fx ? transitionTime * state.length : transitionTime);
            }
        }

        // foreach(var key in Player.useKeys)
        // if (pl.InputGetKey(key))

        animator.speed = 1;


        isShooting = state.IsTag("");

        // animator.speed = 1;
        if (IsMine)
        {
            float deltaTimeLeft = Time.deltaTime;
            const float interval = .023f;
            if (isShooting)
            {
                if (IsMine)
                    if (_MobileInput.touchPad.time == Time.time && Time.time != pl.lastKeyTime || Input2.GetKeyDown(KeyCode.A) || Input2.GetKeyDown(KeyCode.D) || Input2.GetKeyDown(KeyCode.W) || Input2.GetKeyDown(KeyCode.S))
                        CallRPC(Interrupt);

                while (true)
                {
                    damageFactor = animator.GetFloat(AnimParams.DamageCurve) + 1;

                    if ((lastPos - swordPosition).magnitude / (deltaTimeLeft % interval) > attackSensivity * damageFactor)
                        CheckHit();
                    lastPos = swordPosition;

                    if (pl.IsMainPlayer)
                    {
                        if (deltaTimeLeft > interval)
                        {
                            deltaTimeLeft -= interval;
                            // var clip = animator.GetCurrentAnimatorClipInfo(0).FirstOrDefault().clip;
                            // clip?.SampleAnimation(animator.gameObject, state.normalizedTime * clip.length- deltaTimeLeft);
                            // animator.transform.localPosition = Vector3.zero;

                            animator.speed = 1;
                            animator.Update(interval);
                            pl.ControllerMove(animator.deltaPosition);
                            animator.speed = deltaTimeLeft / Time.deltaTime;
                            continue;
                        }
                    }
                    break;
                }
            }
            lastPos = swordPosition;
        }
    }
    
    private void UnsetLastKey()
    {
            // pl.unpressKey.Add((KeyCode) pl.lastKey);
        // pl.SetKey((KeyCode) lastKey, false);
        
        animator.SetInteger(AnimParams.lastKey, (int) (pl.lastKey= 0));
    }
    private float damageFactor;

   
    private void OnAnimationEvent(object objectReferenceParameter)
    {

        if (objectReferenceParameter is EventType2 t)
        {
            if (!pl.bot && IsMine)
                pl.PlayOneShot(t == EventType2.Scream ? pl.playerClassPrefab.attackScream : pl.playerClassPrefab.attackScreamSpecial);
            pl.PlayOneShot(swingSound);    
            return;
        }
        if (objectReferenceParameter is AudioClipCollection cc)
        {
            pl.PlayOneShot(cc.play.Random());
            return;
        }

        if (objectReferenceParameter is AudioClip c)
        {
            pl.PlayOneShot(c);
            return;
        }

        // Debug.Break();
        if (!IsMine) return;
            
        SwordParticles sp = ((GameObject) objectReferenceParameter ?? defaultEffectPrefab).GetComponent<SwordParticles>();
        if (Time.time - lastTimeUsed < sp.coldown) return;
        
        CallRPC(CreateEffect, Effects.IndexOf(sp.gameObject), sp.spawnAtSword ? swordPosition : pl.pos, sp.spawnAtSword ? bx.transform.rotation * defaultEffectPrefab.transform.rotation : pl.skin.rot);
    }
    [PunRPC]
    private void CreateEffect(int index, Vector3 plPos, Quaternion rot)
    {
        var ef = Instantiate((index == -1 ? defaultEffectPrefab : Effects[index]).GetComponent<SwordParticles>(), plPos, rot);
        
        if (IsMine)
            foreach (Collider hit in Overlap(ef.trigger.position, ef.trigger.lossyScale / 2f, ef.trigger.rotation))
                Hit(new RayCastHitStruct() {collider = hit, transform = hit.transform, point = hit.transform.position}, true);
    }
    private static Collider[] Overlap(Vector3 pos, Vector3 size, Quaternion rot)
    {
        ExtDebug.DrawBox(pos,size,rot,Color.red,1);
        return Physics.OverlapBox(pos, size, rot);
    }


    
    // private Vector3 swordPosition => bx.transform.TransformPoint(bx.center);
    private Vector3 swordPosition => animator.deltaPosition+placeholder.TransformPoint(gunSkinPrefab.transform.InverseTransformPoint(gunSkinPrefab.bx.transform.TransformPoint(gunSkinPrefab.bx.center)));
    private Vector3 lastPos;


    
    private HashSet<object> landed = new HashSet<object>();
    public void CheckHit()
    {
        Debug.DrawLine(lastPos, swordPosition, Color.red, 5);

        var direction = swordPosition - lastPos;
        
        foreach (RaycastHit h in BoxCastAll(lastPos, bx.size / 2, direction.normalized, bx.transform.rotation, direction.magnitude))
            if(h.point!=Vector3.zero)
                Hit(h);
    }
    private Dictionary<int, float> specialsColdown = new Dictionary<int, float>();
    
    
    private void Hit(RayCastHitStruct h, bool special = false)
    {
        if (damageFactor <= 0)
            return;
        
        Player enemy = h.transform.root.GetComponent<PlayerSkin>()?.pl;
        if (!enemy || enemy == pl || !enemy.IsEnemy(pl))
        {
            if (Layer.levelMask.Contains(h.transform.gameObject.layer) && !special)
            {
                if (landed.Add(h.transform))
                    pl.PlayOneShot(missSound);
                _Game.EmitParticles(h.point, h.normal + Vector3.up, _Game.res.concerneParticle);
            }
            return;
        }

        if (!landed.Add((enemy, special))) return;
        
        // lastHit = h2;
        
        var dir = ZeroY(enemy.pos - pl.pos).normalized;
        if (enemy.curGun is Sword enemySword && /*enemy.IsMine ||*/ IsMine)
        {
            if (special && enemySword.state.IsTag("dodgeTag"))
                return;

            if (!special && enemySword.idleState && !pl.running && Vector3.Dot(ZeroY(enemy.Cam.forward), ZeroY(pl.skin.forward)) < 0 && (pl.bot || !enemy.bot /*|| Random.value < .1 || roomSettings.hardBots*/))
            {
                _Game.EmitParticles(h.point, h.normal + Vector3.up, _Game.res.metalParticle);
                enemy.RPCPlayAnim(Animator.StringToHash(nameof(Anims.block)), .1f, -dir); //will execute second rpc on animation sync
            }
            else
            {
                enemy.CreateBlood(h);
                enemy.RPCPlayAnim(Animator.StringToHash(nameof(Anims.Damage)), .1f, -dir);
                enemy.RPCDamageAddLife(-(int) damage * damageFactor, pl, this, enemy.skin.GetBodyPart(h.transform));
            }
        }
    }


    [PunRPC]
    private void Interrupt()
    {
        UnsetLastKey();
        pl.Fade(Anims.idle,.1f);
    }
    private RaycastHit[] BoxCastAll(Vector3 start, Vector3 size, Vector3 dir, Quaternion rot, float dist)
    {
        var boxCastAll = Physics.BoxCastAll(start, size, dir, rot, dist);
        ExtDebug.DrawBoxCastBox(start, size, rot, dir, dist, Color.black, 2.5f);
        return boxCastAll;
    }
    #endif
}