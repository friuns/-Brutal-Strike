using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Bullet : BulletBase,IOnLoadAsset 
{
    public new Light light;
    public LineRenderer lineRenderer;
    public Transform[] offsetTransform;
    public ParticleSystem[] bulletTrails;
#if game
    public override void Start()
    {
        
        // transform.Find("bulletsmoke").gameObject.SetActive(false /*bs.qualityLevel >= (Android ? QualityLevel.Ultra : QualityLevel.VeryHigh)*/);
        if (lineRenderer) lineRenderer.SetPosition(1, Vector3.forward * Random.Range(15, 25));
        if (light) light.enabled = qualityLevel > QualityLevel.High;
        
    }
    public Player shooter { get { return wep.pl; } }
//    public bs emit;
    public override bool IsMine { get { return wep.IsMine; } }
    internal float WallScore = 0;
    //public RaycastHit? oldHit = null;
    internal Vector3 velocity;
    // public LineRendererResizer3 resizer3;
    internal new Vector3 pos;
//    public new Transform transform { get { return base.transform; } }
//    public LineRenderer ln;
    //private MyRandom Random;
    private Vector3 startPos;
    private new RoomSettings roomSettings;
    // private TrailRenderer trailRenderer;
    // private new ParticleSystem particleSystem;
    internal Weapon wep=>gun as Weapon;
    
    public IEnumerator Start2()
    {
        // resizer3.Update();
        roomSettings = bs.roomSettings;
        bulletSpeedFactor = gameSettings.bulletSpeedFactor * wep.bulletSpeed * (wep.Compressor ? 2 : 1); 
        startPos=pos = base.pos;
        startTime = Time.time;
        oldDesctructable = null;
        WallScore = 0;

        var transformForward = transform.forward;
        velocity = transformForward * bulletSpeedFactor;
        firstPass = false;
        shooter.IncreaseScore(shooter.gameScore.shoots, 1);
        Vector3 st;
        Vector3 range = velocity * Random.Range( 0, 1f / 30);// transformForward * Random.Range(5, bulletSpeedFactor * 1f / 30);
        var plObserving = wep.pl.observing;

        if (plObserving && _ObsCamera.fpsCam)
        {
            var wp = _ObsCamera.handsCamera.WorldToViewportPoint(wep.Hands.muzzleFlashPos.position);
            // wp += new Vector3(0, wep.aiming ? -1 : 0, 0);
            st = _ObsCamera.camera.ViewportToWorldPoint(wp); // + new Vector3(wep.aiming ? 0 : .2f, wep.aiming ? -.4f : -.2f, 0)
            
            // st = mainCameraPos + (wep.Hands.muzzleFlashPos.position - _ObsCamera.handsCamera.transform.position) + Random.insideUnitSphere * 0.1f;
            // st += _ObsCamera.rot * new Vector3(wep.aiming ? 0 : .2f, wep.aiming ? -.4f : -.2f, 0);
        }
        else
            st = pos;

        if (!oculus)
            foreach (Transform t in offsetTransform)
            {
                if (plObserving && _ObsCamera.fpsCam)
                {
                    t.position = st + range * .5f;
                    // var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // c.transform.localScale *= 0.1f;
                    // c.transform.position = t.position;

                    // t.localEulerAngles = new Vector3(3, 0, 0);
                }
                else
                    t.position = wep.pl.curGun.gunSkin.pos + velocity * Random.Range(0, 1f / 30);
            }
        
        if (!userSettings.disableBloodAndParticles && qualityLevelAndroid > QualityLevel.Medium)
            foreach(var a in bulletTrails)
                a.Emit(new ParticleSystem.EmitParams()
                {
                    position = st - transformForward * Random.Range(1, 5), velocity = transformForward * Random.Range(.1f, .5f),
                    startSize = a.main.startSizeMultiplier, startLifetime = Random.Range(1, 5),startColor = (/*plObserving? Color.white:*/Color.white*.7f)
                },1);
        
        yield return Temp<WaitForEndOfFrame>.value;
        
        if (wep.simpleBulletPhysics) 
            UpdateBulletSimple();//need to be executed after all players update
        
        // trailRenderer.Clear();
        // particleSystem.enableEmission = plObserving;
        
        Update(); // must be at the end of function
        
    }
    private float bulletSpeedFactor;
    float startTime;
    Vector3 move;
    
    void Update()
    {
        try
        {

            move = Vector3.zero;

            float deltaTime = 1 / 60f;
            for (int i = TimeElapsedFixed(deltaTime, startTime - deltaTime) - 1; i >= 0; i--) //fixed update gravity
            {
                velocity += Physics.gravity * (wep.bulletGravity * gameSettings.bulletGravity * deltaTime);

                if (wep.bulletDrag * gameSettings.bulletDrag > 0)
                    velocity += -velocity.normalized * (wep.bulletDrag * gameSettings.bulletDrag * (velocity.sqrMagnitude + velocity.magnitude * 30)) / 1000f * deltaTime;


//            if (emit)
//                _Game.EmitParticles(pos + move, Vector3.up, emit.tr);

                move += velocity * deltaTime;
            }
            if (move == Vector3.zero) return;



            if (!wep || !wep.pl || //if player left
                !wep.simpleBulletPhysics && UpdateBullet() || getdist <= 0 || Time.time - startTime > Mathf.Max(isDebug ? 2500 : 500, _Game.bounds.size.magnitude) / bulletSpeedFactor)
            {
                if (wep.explodeOnCollision)
                {
                    base.pos = pos = hit.point; 
                    Explode();
                }
                Destroy2(gameObject);
                return;
            }
            WhistleEffect(move);

            transform.forward = move;
            if (Base.IsLogging(LogTypes.rayCast))
                Debug.DrawRay(pos, move, Color.red, 3);
            pos += move;


            base.pos = pos;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Destroy2(gameObject);
        }
        // if (frame++ == 3) //should be at the end because still showing when gets destroyed instantly
        //     foreach (Transform t in offsetTransform)
        //         t.localPosition = Vector3.zero;
    }
    
    private void WhistleEffect(Vector3 forwardDir)
    {
        if (!wep.pl.observing && WallScore == 0 && InRange(mainCameraPos- pos,50)) //Plane.Raycast is slow
        {
            var ray = new Ray(pos, forwardDir);
            if (new Plane(wep.pl.hpos - mainCameraPos, mainCameraPos).Raycast(ray, out float enter))
            {
                
                Debug.DrawLine(pos, ray.GetPoint(enter), Color.green, 5);
//                PlayOneShot(wep.bulletWhistle);
                PlayClipAtPoint(wep.bulletWhistle, ray.GetPoint(enter),preset:gameRes.bulletWhistle,pitch:Random.Range(.3f, 1f));
            }
        }
    }

    float getdist { get { return move.magnitude; } }
    Vector3 direction { get { return move.normalized; } }
    Stack<RaycastHit> hits = new Stack<RaycastHit>(10);
    bool firstPass;
    
    public static IComparer<RaycastHit> raycastComparer = AnonymousComparer.Create<RaycastHit>((a, b) => a.distance.CompareTo(b.distance));
    private void UpdateBulletSimple()
    {
        
        var hits = TempArray<RaycastHit>.GetArray(20);
        
        
        var cnt = Physics.RaycastNonAlloc(pos, velocity, hits, 99999, Layer.allmask);
        // else
        // var cnt = Physics.Raycast(pos, velocity, out hits[0], 9999, Layer.allmask) ? 1 : 0;d

        Array.Sort(hits, 0, cnt, raycastComparer);
        RaycastHit? firstHit=null;
        for (var i = 0; i < cnt; i++)
        {
            var h = hits[i];
            var hCollider = h.collider;
            int layer = hCollider.gameObject.layer;
            var level = Layer.levelMask2.Contains(layer);
            if (level)
            {
                HoleAndParticles(h);
                if (firstHit ==null)
                    firstHit = h;
                // continue;
                if (!roomSettings.ShootThroughWalls)
                    return;
            }
            ISetLife isetlife = h.transform.GetComponentInParent<ISetLife>();

            if (isetlife is Destructable destructable && IsMine && oldDesctructable != destructable && destructable.team != shooter.team)
            {
                HoleAndParticles(h);
                
                destructable.RPCDamageAddLife(-damage, shooter.viewId, wep.id, hitPos: h.point);
                oldDesctructable = destructable;
                if (!roomSettings.ShootThroughWalls)
                    return;
            }
            else if (layer == Layer.ragdoll || isetlife is Player)
            {
                if (firstHit!=null && WallScore == 0) //draw hole 
                    if (Physics.Linecast(h.point, firstHit.Value.point, out hit, Layer.levelMask2))
                    {
                        HoleAndParticles(hit);
                        var hitCollider = hit.collider;
                        var wall = hitCollider.GetComponentNonAlloc<Wall>();

                        WallScore = ((firstHit.Value.point - hit.point).magnitude + minThikness) * (wall.IsNotNull() ? wall.density : hitCollider is MeshCollider m && m.convex ? 100 : 1);
                        // Debug.Log("WallScore:" + Math.Round(WallScore, 2));
                        // Debug.DrawLine(firstHit.Value.point, h2.point, Color.red, 10);
                    }

                if (WallScore > wep.bulletPass * gameSettings.bulletPassFactor * 10 )
                    return;
                if (layer == Layer.ragdoll)
                    if (damage > 5 || WallScore == 0)
                        using (Profile("RagDollCheck"))
                            RagDollCheck(h);
            }
            else if (isetlife != null)
            {
                isetlife.RPCDamageAddLife(-damage, shooter.viewId, wep.id, hitPos: h.point);
            }
            
        }
    }
    public  const  float minThikness = .3f;
    private bool UpdateBullet()
    {
//        var cols = Physics.RaycastAll(pos, move, move.magnitude, Layer.allmask); 
//        if (cols.Any(a => a.collider.name.StartsWith("head", StringComparison.OrdinalIgnoreCase)))
//            print("checkRaycast1");
        
        int i = 0;

        float dist;
        while ((dist = getdist) > 0 && RayCastTransparent(new Ray(pos, move), out hit, dist, Layer.allmask, QueryTriggerInteraction.Collide)) //dont use raycastAll because it wont register same object twice 
        {
            bool frontFace = Vector3.Dot(transform.forward, hit.normal) < 0;

            var hCollider = hit.collider;
            int layer = hCollider.gameObject.layer;
            var level = Layer.levelMask2.Contains(layer) ;
            if (level && (!gameSettings.ShootThroughWalls || wep.explodeOnCollision || wep.pl.bot)) { HoleAndParticles(hit); return true; }

            if (i++ > 15) { print("overflow"); return true; }
            if (!hCollider.isTrigger)
            {
                if (level)
                {
                    if (!(hCollider is MeshCollider) && isLoggingDef) Debug2.Log(LogTypes.missing, "only mesh colliders supported", hCollider.gameObject);

                    if (frontFace)
                    {
                        if (layer != Layer.pickable)
                            HoleAndParticles(hit);
                        hits.Push(hit);
                    }
                    else if (hits.Count > 0)
                    {

                        RaycastHit h1 = hits.Pop();
                        RaycastHit h2 = hit;

                        var h1Collider = h1.collider;
                        var wall = h1Collider.GetComponentNonAlloc<Wall>();
                            
                        float wallSize = ((h1.point - h2.point).magnitude+minThikness) * (wall.IsNotNull() ? wall.density : 1) * Random.Range(.3f, 3);
                        
                        if (h1Collider is MeshCollider m && m.convex)
                            wallSize += 100;
                        
                        var wepBulletPass = wep.bulletPass * gameSettings.bulletPassFactor;

                        if (!firstPass)
                        {
                            
                            WallScore += wallSize;
                            var f = Mathf.Min(.7f, wallSize / wepBulletPass);
                            //var rotate = Quaternion.FromToRotation(velocity.normalized, Vector3.Slerp(velocity.normalized, (-h1.normal + Random.insideUnitSphere).normalized, Mathf.Min(.4f, f / 5)));
                            var rotate = Quaternion.Lerp(Quaternion.identity, Random.rotation, Mathf.Min(.4f, f / 5));

                            var oneMinusF = Mathf.Pow(1 - f, 2);
                            velocity = rotate * velocity * oneMinusF;
                            move = rotate * move * oneMinusF;
                            pos = h1.point + direction * .1f;
                            firstPass = true;
                            hits.Push(h1);
                            if (Vector3.Dot(rotate * velocity, h1.normal) > 0 /*&& Mathf.Abs(h1.normal.y) < .5f*/)
                            {
                                Debug2.Log(LogTypes.rayCast, "reflect");
                            }
                            else if (WallScore > wepBulletPass || velocity.magnitude < bulletSpeedFactor / 3)
                            {
                                move = Vector3.zero;
                                return true;
                            }

                            continue;
                        }
                        firstPass = false;

                        hole(h2);

                    }
                    else
                        prints("invisible wall");

                }


                var isetLife = hit.transform.GetComponentInParent<ISetLife>();
                if (isetLife is Destructable destructable && destructable.IsNotNull() && IsMine && oldDesctructable != destructable && destructable.team != shooter.team)
                {
                    // HoleAndParticles(h, true);
                    destructable.RPCDamageAddLife(-damage, shooter.viewId, wep.id, hitPos: hit.point);
                    oldDesctructable = destructable;
                }
                else if (!(isetLife is Player))
                    isetLife?.RPCDamageAddLife(-damage, shooter.viewId, wep.id, hitPos: hit.point);
            }
            // if (layer == Layer.player || layer == Layer.playerTrigger)
            if (layer == Layer.ragdoll)
            {
                using (Profile("RagDollCheck"))
                    if (RagDollCheck(hit) && wep.explodeOnCollision)
                        return true;

            }



//            Debug.DrawLine(h.point, pos, Color.red, 3);
            var dest = pos + move;
            
            pos = hit.point + direction * .005f;
            
            move = dest - pos;
            // if(xr.isActiveAndEnabled)
            //     xr.Update2(xr.transform.position = pos);

        }

        return false;
    }
    
//    private bool isRagdoll(RaycastHit h)
//    {
//        //return h.collider && h.collider.gameObject.layer == Layer.ragdoll;
//        var hCollider = h.collider;
//        var pl = (object)hCollider!=null && hCollider.gameObject.layer == Layer.ragdoll ? hCollider.gameObject.GetComponentInParent<ISetLife>() : null;
//        return pl != null && pl != (ISetLife)wep.pl;
//    }
    private int damage
    {
        get
        {
//            return (int) (wep.damage * Mathf.Pow(velocity.magnitude / bulletSpeedFactor, 2));
            var wepDamage = (wep.damage * wep.rangeFalloff.Evaluate((startPos- pos).magnitude));
//            if(WallScore>0)
                wepDamage *= 1f - Mathf.InverseLerp(0, wep.bulletPass * gameSettings.bulletPassFactor, WallScore);
            return (int) wepDamage;
        }
    }


//    RaycastHit[] hitsAlloc = new RaycastHit[20];
//    RaycastHit[] hitsAlloc2 = new RaycastHit[10];
//    private List<RaycastHit> RayCastWithBackFace(Vector3 pos, Vector3 v, float vmag)
//    {
//        var cnt = Physics.RaycastNonAlloc(new Ray(pos, v), hitsAlloc, vmag, Layer.allmask, QueryTriggerInteraction.Ignore);
//        var cnt2 = Physics.RaycastNonAlloc(new Ray(pos + v, -v), hitsAlloc2, vmag, Layer.allmask, QueryTriggerInteraction.Ignore);
//        List<RaycastHit> hits = TempList<RaycastHit>.GetTempList();
//        hits.Clear();
//        for (int i = 0; i < cnt; i++)
//            hits.Add(hitsAlloc[i]);
//        for (int i = 0; i < cnt2; i++)
//            hits.Add(hitsAlloc2[i]);
//
//        if (comparer == null)
//            comparer = AnonymousComparer.Create<RaycastHit, Bullet>((a, b, t) => (a.point - t.pos).magnitude < (b.point - t.pos).magnitude ? -1 : 1, this);
//        hits.Sort(comparer);
//        return hits;
//    }
//    IComparer<RaycastHit> comparer;


    private void HoleAndParticles(RaycastHit h)
    {
        // if(fxEnabled)
        // if (!Android || CheckRayOrNearAndroid(h.point))
        {
            //_Game.EmitParticles(h.point, h.normal, _Game.res.bloodParticle);
            if (!wep.explodeOnCollision && _Game.EmitParticles(h.point, h.normal + new Vector3(0, .5f, 0), wep.explosionParticle, enableLight: true))
                PlayClipAtPoint(wep.bulletric, h.point);
            if (h.collider.CompareTag(Tag.Glass) && !depthBufferPresent)
                return;
            // if (!wep.explodeOnCollision)
                hole(h);
        }
    }
    Destructable oldDesctructable;


    private bool RagDollCheck(RaycastHit h)
    {
        if (!userSettings.disableRagdoll)
        {
            MyTimer.DelayCall((h.rigidbody, velocity), t =>
            {
                var r = t.rigidbody;
                r.AddForce(500 * r.mass * t.velocity.normalized);
                r.velocity = Vector3.ClampMagnitude(r.velocity, .5f);
            }, frame: 1);
        }


        PlayerSkin enemySkin = h.transform.root.GetComponentNonAlloc<PlayerSkin>();
        var enemy = enemySkin?.pl;
        if (enemy == null) //dead
        {
            
            if(enemySkin)CreateBlood(h, _Game.res.bloodParticle2.Random(),enemySkin); //dead body
            // _Game.EmitParticles(h.point, -mainCameraForward, _Game.res.bloodParticle.Random());
            if (enemySkin != null && (Random.value < .1f|| wep.sniper))
                enemySkin.CreateGib(enemySkin.GetBodyPart(h.transform));
            return false;
        }
        
        
        
        if (!shooter.IsEnemyOrBot(enemy) && !wep.allowFriendlyFire || shooter.SpawnProtection(enemy) || enemy.dead)
            return false;

        wep.lastHit = h;
        wep.wallPenetraiton = WallScore > 0;

        if (wep.pl.botSimple ? enemy.IsMine : roomSettings.validateBulletsTwoWays ? (IsMine || enemy.IsMine) : IsMine) //if bot check on enemy side only
        {
            float damage = (float)this.damage/wep.bulletsPerShoot;

          
            if (!enemy.dead && !(enemy.knocked && roomSettings.dontKillKnocked) && damage !=0)
            {
                HumanBodyBones bp = enemy.skin.GetBodyPart(h.transform);
                var head = bp == HumanBodyBones.Head;


                if (wep.weaponType == WeaponType.Shotgun && bp == HumanBodyBones.Head)
                    bp = HumanBodyBones.Chest;
                
                if (wep.pl.botSimple && head)
                    bp = HumanBodyBones.LeftUpperLeg;
                

                

                // if (enemy.damageDeal.maxDamage == 0)
                 
                
                enemy.RPCDamageAddLife(-damage, wep.pl.viewId, wep.id, bp, h.point);
            }
            
        }
        return true;
        // WallScore++;
    }
    


    public static void CreateBlood(RayCastHitStruct h, Transform particleTransform, PlayerSkin skin)
    {
        if (userSettings.disableBloodAndParticles) return;
        if (!userSettings.simpleBlood)
        {
            if (depthBufferPresent) //blood hole
            {
                TransformCache dd = _Pool.Load(gameRes.bloodHit, h.point, Quaternion.identity);
                foreach(var a in dd.components)
                    if (a is BFX_ShaderProperies d)
                    {
                        d.enabled = false;
                        d.enabled = true;
                    }

                var lc = dd.localScale;
                dd.up= h.normal;
                dd.localScale = lc;
                dd.parent = h.transform;
                
                skin.bloods.Add(dd.transform);
                _Pool.Save(dd.tr, 5);
                  
            }
            
            
            if (Random.value < .1)
            {
                var g = _Pool.Load(gameRes.bloodSquirts.Random(), h.point, Quaternion.identity);
                // Transform g = Instantiate2(gameRes.bloodSquirts.Random(), h.point, Quaternion.identity, true); //blood leak
                g.parent = h.transform;
                g.forward = h.normal;
                // g.position = h.point;
                g.DelayCall(3, () =>
                {
                    g.parent = null;
                    _Pool.Save(g.tr);
                });
            }

            TransformCache pr = _Pool.Load(particleTransform, h.point,Quaternion.identity);
            pr.tr.right = ZeroY(h.normal);

            foreach (Component a in pr.components)
            {
                if (a is BFX_DecalSettings c)
                {
                    
                    c.enabled = true;
                }
                if (a is BFX_ManualAnimationUpdate b)
                {
                    b.BloodSettings.AnimationSpeed = 2 + Random.value;
                    b.enabled = false;
                    b.enabled = true;
                }
            }

            _Pool.Save(pr.tr, 5);
            if (!Android && qualityLevel >= QualityLevel.VeryHigh)
                _Game.EmitParticles(h.point, -mainCameraForward, gameRes.bloodParticle.Random()); //blood particles
            else
                _Game.EmitParticles(h.point, -mainCameraForward, gameRes.bloodDrops); //blood particles
        }
        else
            _Game.EmitParticles(h.point, -mainCameraForward, gameRes.bloodParticleSimple);
    }


    private void hole(RaycastHit h)
    {

        // if (!Android || CheckRayOrNearAndroid(h.point))
            if (h.collider is MeshCollider m && !m.convex || h.collider is TerrainCollider)
            {
                
                _Game.EmitParticles(h.point + h.normal * .005f, h.normal, h.transform.CompareTag(Tag.Glass) ? _Game.res.glassHoleDecal.transform : wep.holeDecal, true, maxParticlesPerSecond: 5);
                //var g = Instantiate2(h.transform.tag == Tag.Glass ? _Game.res.glassHoleDecal : _Game.res.holeDecal, h.point + h.normal * .005f, Quaternion.LookRotation(h.normal));
                //g.hideFlags = HideFlags.HideInHierarchy;
                ////g.transform.SetParent(h.transform, true);
                //Destroy2(g, Android ? 5 : 10);
            }
    }
      public void OnLoadAsset()
    {
        if (bulletTrails == null)
            bulletTrails = gameRes.bulletTrails;
        foreach (var a in bulletTrails)
            a.transform.parent = null;
        
        offsetTransform = transform.GetTransforms().ToArray();
    }
    #endif

  
}