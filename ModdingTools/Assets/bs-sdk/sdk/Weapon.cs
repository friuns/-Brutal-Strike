using Input3=Input2;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq;
using System.Text.RegularExpressions;
#if game
using System.Diagnostics;
using bsp;
#endif
#if UNITY_EDITOR
using System.IO;
using UnityEditor.Animations;
#endif
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using EnumsNET;
// using UnityEditor;
// using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
// using UnityEngine.SocialPlatforms;
// using AnimatorController = UnityEditor.Animations.AnimatorController;
using Debug = UnityEngine.Debug;

//using Slinq;
public enum WeaponType
{
    Pistol = 0,
    Shotgun = 1,
    Rifle = 2,
    Smg = 3,
    Sniper = 4, /*machineGun = 5, */
    Armor = 10,
    Equipment = 7,
    Attachments = 8,
    Ammo = 9
}
// [RequireComponent(typeof(PhotonView))]
public partial class Weapon : WeaponBase
{
     
    public override void OnInspectorGUI()
    {
        if (!handsPrefab) GUILayout.Label("Hands Missing!", GUIStyle.none);
        base.OnInspectorGUI();
    }
    [FieldAtrStart()] 
    [Header("       Weapon settings")]
    public bool reloadOneByOne;
    [Tooltip("if shooting one by one dont hard recoil")]
    public bool firstShootNoHardRecoil=true;     
    public float softRecoilClamp = 999;
    public float hardcoreRecoil = 0;
    public float softRecoil = 1;
    public float recoilKick;
    public ObscuredFloat recoverLerp = 2;
    public float focus=1;
    //public ObscuredInt maxBullets = 0;
    //public ObscuredInt maxClips = 2;
    // public ObscuredInt extraRayCount = 1;
    public ObscuredBool automatic = false;
    public ObscuredFloat shootInterval = 0.1f;
    public ObscuredFloat shootBump = 1.36f;
    
    [FormerlySerializedAs("bulletFalloff")] 
    public AnimationCurve rangeFalloff = AnimationCurve.Constant(0,1,1); 
    
    public ObscuredVector3 shootSpread = new Vector3(0, 2, 1);
    
    public ObscuredFloat bulletSpread = 0;
    [Tooltip("affected by aim or crouch")]
    public AnimationCurve bulletSpreadOverTime = AnimationCurve.Constant(0, 1, 0.001f);

    public ObscuredInt bulletsPerShoot = 1;
    
//    public ObscuredFloat Range = 900;
//    public ObscuredFloat rangeModifier = .7f;

    public ObscuredFloat bulletGravity = .1f;
    public ObscuredFloat bulletDrag = .1f;
    public ObscuredFloat bulletSpeed = 700;

    [Tooltip("bulletSpread * aimRecoilReduce^aimSpread")] 
    public float aimSpreadImprove = 4;
    
    public ObscuredFloat crouchOrProneRecoilReduce = .7f;
    public ObscuredFloat aimRecoilReduce = .9f;
    // [HideInInspector] public ObscuredFloat colldown = .3f;
    [Tooltip("this value added to bulletSpread when running")]
    public ObscuredFloat runStaticInAccuraty = 0.03f;
    //public AmmoType ammoType;
    [FieldAtrEnd()] protected int tmp;
    [Header("       audio")] public AudioClip2 bulletWhistle;
    
    public AudioClip2 dryFire;
    [Validate] public Bullet bulletPrefab;
    internal ObscuredBool automaticMode = false;
    public SubsetListAttachments attachmentAvailable = new SubsetListAttachments(Enum<AttachmentSlots>.values);
    public AudioClip2 shootSound;
    public AudioClip2 silencerSound;
    [Tooltip("Recoil over time")]
    [FormerlySerializedAs("recoilCurve")]
    public AnimationCurve recoilCurveH = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve recoilCurveV = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    
    
#if game
    public ObscuredBool sniper => weaponType == WeaponType.Sniper;
    public ObscuredBool sniperAndAim => weaponType == WeaponType.Sniper && aiming;
    public bool haveScope { get { return sniper || canAimWithoutScope || scope; } }
    public ObscuredInt maxBullets2 { get { return (int)((ExtendedClip? 1.334f:1) *  secondaryCountDef) ; } set { secondaryCountDef = value; } }
    
    
    public float aimSpeed { get { return scope?.m_aimSpeed ?? m_aimSpeed; } }
    public float ScopeFov
    {
        get
        {
            var attachment = scope;
            if (attachment)
                return attachment.scopeFov;
            return scopeFov;
        }
    }
    public bool AttachmentAvailable(AttachmentBase a)
    {
        var at = a as Attachment;
        if (at)
            return attachmentAvailable.Contains(at.attachmentSlot) && a.canAttachToWeapon.Contains(weaponType);
        return a.canAttachToWeapon.Contains(weaponType);
    }

    
    public Attachment scope { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.scope); } }
    public Attachment silencer { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.silencer); } }
    public Attachment flashHider { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.flashHider); } }
    public Attachment HorizontalGrip { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.HorizontalGrip); } }
    public Attachment StockAttachment { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.StockAttachment); } }
    public Attachment VerticalForegrip { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.VerticalForegrip); } }
    public Attachment Compressor { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.Compressor); } }
    public Attachment ExtendedClip { get { return attachments.FirstOrDefault(a => a.attachment == AttachmentType.ExtendedClip); } }
    public Attachment GetAttachmentAtSlot(AttachmentSlots slot)
    {
        return attachments.FirstOrDefault(a => a.have && a.attachmentSlot  == slot);
    }



    public Ammo ammoPrefab;
    public Ammo ammo;
    
    public override void Init()
    {
        base.Init();

        ammo = pl.guns.FirstOrDefault((Ammo a) => !a.disabled && a.canAttachToWeapon.Contains(weaponType));
        ammoPrefab = _Game.playerPrefab.guns.FirstOrDefault((Ammo a) => !a.disabled && a.canAttachToWeapon.Contains(weaponType));
        
    }

    private readonly CacheArray<Attachment> m_attachmentsHave = new CacheArray<Attachment>();
    public List<Attachment> attachmentsHave
    {
        get
        {
            //if (!handsPrefab.codGunPivot) return Empty<Attachment>.list;
            if (!m_attachmentsHave.inited)
                m_attachmentsHave.Setup(() => pl.guns.WhereNonAlloc2((Attachment a) => a.have && a.CanAttachToWeapon(pl.curGun)), () => Many(pl.curGun), ref pl.countChanged);
            return m_attachmentsHave.GetCached();
        }
    }
    
    private readonly CacheArray<Attachment> m_attachments = new CacheArray<Attachment>();
    public override List<Attachment> attachments
    {
        get
        {
            //if (!handsPrefab.codGunPivot) return Empty<Attachment>.list;
            if (!m_attachments.inited)
                m_attachments.Setup(() => pl.guns.WhereNonAlloc2((Attachment a) => a.have && a.AttachedTo.Contains(this)), ref pl.countChanged);
            return m_attachments.GetCached();
        }
    }

    public override ObscuredFloat visualInaccuracy { get { return 
        
        useEuler ?  Quaternion.Angle(pl.CamRnd.localRotation, Quaternion.identity) + (bulletSpread + runSpread * accuracyImprove ):
            Mathf.Pow(1 + Quaternion.Angle(pl.CamRnd.localRotation, Quaternion.identity) + (bulletSpread + runSpread * accuracyImprove) * 30, 2); 
    } }
    [PunRPC]
    public override void SetCount(int Count)
    {
        if (Count == 0)
            foreach (var a in attachments)
                a.AttachedTo.Clear(); //never executed, because SetCount never called
        base.SetCount(Count);
    }
    private float runSpread
    {
        get
        {
            var v = pl.dirNew;
            // v.x /= pl.clampHorizontal;
            float normalizeSpeed = pl.isGrounded ? pl.playerClassPrefab.NormalizeSpeed(v.magnitude) : TimeCached.time == mouseDownTime && gameSettings.jumpShoot ? 0 : 1;

            return pl.playerClassPrefab.speedToAccuracyCurve.Evaluate(normalizeSpeed) * runStaticInAccuraty * roomSettings.runSpread;
        }
    }
  

    private int shootFrame;
    //private ObscuredFloat shootTimeElapsed = 0;


    public override bool isShooting { get { return TimeCached.time - shootTime < shootInterval; } }
    internal  bool canAimWithoutScope;
    internal bool simpleBulletPhysics;

    public override void Start()
    {
        simpleBulletPhysics = userSettings.simpleBulletPhysics && bulletSpeed > 300;
        if (!holeDecal) holeDecal = _Game.res.holeDecal.transform;
        automaticMode = automatic;
        base.Start();
        canAimWithoutScope = Hands && Hands.crosshair && roomSettings.canAimWithoutScope;
        // if (!Hands.crosshair)
            // Hands.crosshair = Hands.muzzleFlashPos;
    }

    //[PunRPC]
    //public override void OnBuy()
    //{
    //    //base.OnBuy(); //not used
    //    if (count == 0)
    //        bullets = maxBullets;
    //    else
    //        totalBullets = maxBullets * maxClips;
    //    count = 1;
    //    bought = 1;  
    //}

    internal bool aiming { get { return pl.aiming && haveScope && !isReloadingOrDraw && (canAimWhileShooting ? true : !isShooting); } }
    public override void OnDisable2()
    {
        base.OnDisable2();
        SetMuzzleFlash(false);
    }
    public void Update()
    {
        bool muzzleFLash = (TimeCached.time - shootTime < .03f || Time.renderedFrameCount - shootFrame < 3) && !flashHider;
        SetMuzzleFlash(muzzleFLash);
        if (pl.observing)
            Hands.animator.SetBool(AnimParams.attachmentUI, attachmentUi);
    }
    public override void Update2()
    {
        base.Update2();

        

//        if (pl.knocked) return;

        //cursorOffset = Mathf.MoveTowards(cursorOffset, 0, TimeCached.deltaTime * cursorRecovery);
        if (IsMine)
        {
            if (!have)
                pl.SelectNextGun();

            if (Input2.GetKeyDown(KeyCode.H) && automatic)
                CallRPC(SetAutomatic, !automaticMode);

            if (Input2.GetMouseButtonDown(0) && TotalBullets == 0)
                pl.PlayOneShot(dryFire);

            if (Input2.GetKey(KeyCode.Mouse0) && !isReloadingOrDraw && bullets <= 0 && ammoCount > 0)
                CallRPC(Reload);

            RPCShowAttachmentUI(!pl.bot && !pl.knocked &&  (!Android || attachmentsHave.Count > 0) && Input2.GetKey(KeyCode.K, KeyState.Key));

            var shoot = !isReloadingOrDraw && pl.playerState!= PlayerStateEnum.Executing && MouseButton && bullets > 0 && !_Hud.showAttachmentUI;
            if (!isShooting || !shoot)
                RPCSetMouseButton(shoot);
        }

        if (remoteMouseButtonDown && (automaticMode || pl.bot ))
            //foreach (var _ in TimeElapsedFixed(shootInterval, lastMouseButtonDown))
            if (TimeElapsed(shootInterval, lastMouseButtonDown))
            {
                int ip = pl.GetDistanceToCursor(out Vector3 dc);

                Shoot(pl.hpos, ip, dc);
                

            }
    }
    private void SetMuzzleFlash(bool muzzleFLash)
    {
        try
        {
            gunSkin.muzzleFlashTC.active = (muzzleFLash && pl.modelVisible);
            if (pl.observing)
                Hands.MuzzleFlashLight.enabled = muzzleFLash;
        }
        catch
        {
        }
    }
    [Expose]
    public void GiveAmmo(int count)
    {
        Count = 1;
        bullets = Mathf.Min(maxBullets2,count);
        ammo.info.count += count-bullets;
        ammo.OnCountChangedRefresh();
        
        if (IsMine && canSelect /* && (pl.curGun is Knife || winActive)*/)
            pl.RPCSelectGun(id);
    }
    public override void OnTake(GunInfo info)
    {
        if (imposterMode && weaponType == WeaponType.Pistol && IsMine)
            pl.RPCSetTeam(TeamEnum.CounterTerrorists);
        
        if (info.secondaryCount > secondaryCountDef)
        {
            int newCnt = (((int)info.secondaryCount-1) / secondaryCountDef) * secondaryCountDef; 
            ammo.info.count += newCnt;
            info.secondaryCount -= newCnt;
            // ammo.info.count += info.secondaryCount - secondaryCountDef;
            // info.secondaryCount = secondaryCountDef;
            ammo.OnCountChangedRefresh();
        }
        base.OnTake(info);
        
        if (IsMine && canSelect /* && (pl.curGun is Knife || winActive)*/)
            pl.RPCSelectGun(id);
    }
    public void RemoveAttachment(Attachment at)
    {
        at.AttachedTo.Remove(this);
        at.RpcSyncAttachments();
        //pl.countChanged?.Invoke();
    }
    
    public void AddAttachment(Attachment at)
    {
        if (!AttachmentAvailable(at)) return;
        Weapon wep = this;
        Attachment prev = wep.attachments.FirstOrDefault(a => a.attachmentSlot == at.attachmentSlot);
        if (prev)
        {
            RemoveAttachment(prev);
        }

        if (at.AttachedTo.Count == at.Count)
            at.AttachedTo.RemoveAt(0);

        at.AttachedTo.Add(wep);
        at.RpcSyncAttachments();
        //pl.countChanged?.Invoke();
    }


    

    [PunRPCBuffered]
    public void SetAutomatic(bool b)
    {
        if (pl.observing)
            _Hud.CenterText(Concat("Automatic Mode:", b), 3);
        automaticMode = b;
    }


    private float accuracyImprove => (aiming ? aimRecoilReduce : 1) * (pl.crouch || pl.prone ? crouchOrProneRecoilReduce : 1) / pl.GetCast(MedkitType.accuracy);
    
    public virtual void Shoot(Vector3 hpos, int plID, Vector3 viewportP)
    {
            
        if (pl.knocked) return;
        var enemy = ToObject<Player>(plID);
        Vector3 forward = viewportP;
        if (enemy.IsAlive())
        {
            if (Player.GetMag(viewportP) < 2)
                enemy.playerBot2?.OnMissByPlayer(pl); 
            forward = pl.GetDirFromView(viewportP,enemy.pos).normalized;
        }

        Vector3 gripsScale = GripsScale * gameSettings.recoilFactor;

        bullets -= 1;

        Vector3 getRandom = Vector3.Scale(Random.insideUnitSphere, gripsScale) * (useEuler ?1:.3f);
        
        Vector3 exec(bool userVerticalCurve) 
        {
            float downTime = TimeCached.time - mouseDownTime;
            
            float vert = userVerticalCurve ? shootBump * recoilCurveV.Evaluate(downTime) : (float) shootBump;
            Vector3 spread = Vector3.Scale(recoilCurveH.Evaluate(downTime) * getRandom, shootSpread);
            
            return (spread + new Vector3(gripsScale.x * -vert, 0, 0)) *accuracyImprove;
        }

        if (!oculus)
        {
            float hardcoreRecoil = this.hardcoreRecoil * roomSettings.hardRecoil; //accuracyImprove included in exec()
            float softRecoil = this.softRecoil * roomSettings.softRecoil; //accuracyImprove included in exec()
            if (pl.owner.stats.reverseRecoil)
            {
                softRecoil *= -1;
                hardcoreRecoil *= -1;
            }

            if (recoilKick > 0)
                pl.iMoveController.veloticy -= forward * recoilKick;

            if (TimeCached.time == mouseDownTime && firstShootNoHardRecoil)
            {
                softRecoil += hardcoreRecoil;
                hardcoreRecoil = 0;
            }

            Vector3 offsetDirEuler = exec(true) * hardcoreRecoil;


            pl.mouse += EulerToMouse(offsetDirEuler);

            if (!pl.bot)
                pl.CamRnd.localRotation = Quaternion.Euler(Vector3.ClampMagnitude(clampAngle(pl.CamRnd.localRotation.eulerAngles), softRecoilClamp) + exec(false) * (softRecoil * gripsScale.z));

            if (pl.observing && !oculus)
                _ObsCamera.camOffset -= forward * Mathf.Min(100,damage) / 1000;
        }

        shootFrame = Time.renderedFrameCount;
        shootTime = TimeCached.time;
        ShootAnim();

        for (int i = 0; i < bulletsPerShoot; i++)
        {

            Vector3 spread;


            if (weaponType == WeaponType.Shotgun|| bulletsPerShoot >1)
                spread = bulletSpread * Random.insideUnitSphere;
            else if (sniperAndAim)
                spread = Vector3.one*runSpread;
            else
                spread = getRandom * (((bulletSpread + bulletSpreadOverTime.Evaluate(TimeCached.time - mouseDownTime)) * Mathf.Pow(accuracyImprove, aimSpreadImprove) + runSpread) * roomSettings.shootSpread);

            Vector3 fv = useEuler ? Quaternion.Euler(spread) * forward : forward + spread;
            
            var transformCache = _Pool.Load(bulletPrefab.transform, hpos - forward * .3f, Quaternion.LookRotation(fv));
            var bullet = transformCache.Component<Bullet>();
            bullet.gun = this;

            StartCoroutine(bullet.Start2());


        }
    }


    private Vector3 GripsScale
    {
        get
        {
            
            Vector3 scale=Vector3.one;
            foreach (var a in attachments)
                scale = Vector3.Scale(scale, a.gripScale);
            return scale;
//            return new Vector3(VerticalForegrip ? .2f : 1, HorizontalGrip ? .7f : 1, HorizontalGrip ? .7f : 1);
        }
    }


    public void RPCReload()
    {
        if (!isReloadingOrDraw && needsReload)
            CallRPC(Reload);
    }
    [PunRPC]
    public override void Reload()
    {
        prints("Reload");
        (pl.Input2 as Input2)?.UntoggleGroup(2);
        if (!gameSettings.noReloads)
        {
            reloadingTime = TimeCached.time;
            PlayAnimation(Anims.reload, .1f, playFromBegining: true);
        }
        if (IsMine)
        {
            DelayCall(gameSettings.noReloads ? 0 : reloadTime - .5f, delegate
            {
                if (!isActiveAndEnabled) return;
                if (reloadOneByOne)
                {
                    if (needsReload)
                    {
                        bullets += 1;
                        ammoCount -= 1;
                    }
                    if (needsReload && !pl.MouseButton)
                        CallRPC(Reload);
                    else
                        CallRPC(EndReload);
                }
                else
                {
                    var take = Mathf.Min(maxBullets2 - bullets, ammoCount);
                    bullets += take;
                    ammoCount -= take;
                }
            });
        }
    }

    public bool needsReload { get { return bullets < maxBullets2 && ammoCount > 0; } }

    [PunRPC]
    public void EndReload()
    {
        PlayAnimation(Anims.endReload, .1f,playFromBegining:true);
    }
    public int shootIndex => (int)info.secondaryCount % Hands.MuzzleFlash2.Length;
    public void ShootAnim()
    {
        if (pl.deadOrKnocked) return;
        
        if (pl.observing) //for aim shoot effect
        {
            _ObsCamera.posOffset +=Vector3.forward*.03f;
        }
        
        
        if(!pl.prone)
            PlayAnimation(Anims.shoot + shootIndex, 0.01f, true, 1);

        var silencer = this.silencer;
        var audioSource = /*silencer ? pl.playerAudioSource :*/ pl.WeaponAudioSource;
        PlayOneShotSpread(silencer ? silencerSound.Fallback(silencer.sound) : shootSound, audioSource, silencer ? .4f : Random.Range(.6f,1));

        if (pl.observing && !_Loader.loaderPrefs.disableBloodAndParticles)
        {
            Hands.ShotAnim();
        }
    }
    
    public void PlayOneShotSpread(AudioClip2 Clip2, AudioSource audioSource, float volume = 1f)
    {
        const float dt = distantSoundStart /3;
        //var copy = audio;

        //source.outputAudioMixerGroup = copy.outputAudioMixerGroup;
        //source.clip = Clip2.sound;
        //source.volume = volume * Clip2.volume * volumeFactor;
        //source.spatialBlend = copy.spatialBlend;
        //source.spread = spread;
        
        if (audioSource.IsSoundTooFar(Clip2, ref volume,1f)) {return; }

        // ext.PitchAudio(audioSource, true);
        
        var vol = volume * Clip2.volume * volumeFactorOld;
        if (Clip2.distantSound && !settings.disableDistantSounds)
        {
            var dist = (mainCameraPos - audioSource.transform.position).magnitude;
            var v2 = Mathf.InverseLerp(dt / 4, dt, dist); //returns value 0 to 1 based on dist
            var v1 = 1f - v2;
            
            if (v2 > 0)
                audioSource.PlayOneShot(Clip2.distantSound, v2 * volume * volumeFactorOld* Clip2.distantSoundVolume*.5f);
            
            vol *= v1;
        }
        if (vol > 0)
            audioSource.PlayOneShot(Clip2.sound, vol);
        //source.spread = 0;
    }
    

    private float mouseDownTime;
    [PunRPC]
    public override void SetMouseButton(bool value, Vector3 hpos, int ip, Vector3 dc)
    {
        if(pl.CamRnd.localRotation.x==0)
            mouseDownTime = TimeCached.time;
        base.SetMouseButton(value, hpos, ip, dc);
//        shootings = 0;
        if (value)
            Shoot(hpos, ip, dc);
    }
    internal ObscuredInt bullets { get { return (int) info.secondaryCount; } set { info.secondaryCount = value; } }
    internal ObscuredInt ammoCount
    {
        get
        {
            return roomSettings.unlimitedAmmo || _Game.waitForPls ? 999 : ammo.info.count;
        }
        set
        {
            ammo.SetCount2(value);
        }
    }
    internal int TotalBullets { get { return bullets + ammoCount; } }
    public Transform holeDecal;

    internal bool wallPenetraiton;
    public override void ResetBullets()
    {
        base.ResetBullets();
        if (have)
        {
            bullets = secondaryCountDef;
            // ammoCount = secondaryCountDef * 2;
            ammo.info.count = secondaryCountDef * 2;
            // ammo.OnCountChangedRefresh();
        }

    }
    public override void Reset()
    {
        remoteMouseButtonDown = false;

        if (imposterMode)
        {
            if (pl.team == TeamEnum.CounterTerrorists && this == pl.guns.FirstOrDefault(a => a.canBuy && a.weaponType == WeaponType.Pistol))
            {
                defCount = 3;
            }
            else
                defCount = 0;
            defaultWeaponShop.disabled = true;
        }


        if (roomSettings.awpOnly)
            if (sniper)
                defCount = 5;
            else if (weaponType != WeaponType.Pistol)
                disabled = true;

        base.Reset();

        if (defCount > 1 && !teamBuyDisabled)
        {
            
            ammo.defCount = ammoCount = (defCount - 1) * maxBullets2;
            info.count = 1;
            
        }

    }
    //protected override void DropInit(WeaponPickable drop)
    //{
    //    drop.CallRPC(drop.InitDrop, true, info, false);
    //    info = new GunInfo();
    //}


#if UNITY_EDITOR

    public override void CtxParse2(string key, string value)
    {
        base.CtxParse2(key, value);
        if (key == "Damage")
            damage = int.Parse(value);
        if (key == "CycleTime")
            shootInterval = float.Parse(value);
        if (key == "FullAuto")
            automatic = int.Parse(value) == 1;

        if (key == "WeaponArmorRatio")
            WeaponArmorRatio = float.Parse(value);
        if (key == "Penetration")
            bulletPass = float.Parse(value);
        if (key == "Range")
            rangeFalloff = new AnimationCurve(new Keyframe(0,1),new Keyframe(float.Parse(value),1));
//            Range = float.Parse(value);
        if (key == "RangeModifier")
            rangeFalloff.AddKey(rangeFalloff[2].time,float.Parse(value));
//            rangeModifier = float.Parse(value);
        
        if (key == "clip_size")
            maxBullets2 = int.Parse(value);
        if (key == "Bullets")
            bulletsPerShoot = int.Parse(value);
        // if (key == "RecoveryTimeStand") 
            // colldown = float.Parse(value);
        if (key == "MaxPlayerSpeed")
            runSpeed = float.Parse(value) / 250f;

        if (key == "AccuracyOffset")
            shootBump = 1 + float.Parse(value);
        if (key == "MaxInaccuracy")
            recoverLerp = 2f / (float.Parse(value) / 1.25f);
        if (key == "InaccuracyStand")
            /*standStaticInAccuraty*/
            bulletGravity = float.Parse(value);
        if (key == "InaccuracyMove")
            runStaticInAccuraty = float.Parse(value);
    }


#endif
    internal bool attachmentUi;
    

    public void RPCShowAttachmentUI(bool value)
    {
        if (value != attachmentUi)
            CallRPC(ShowAttachmentUI, value);
    }
    [PunRPC]
    private void ShowAttachmentUI(bool value)
    {
        attachmentUi = value;
    }
    public override string CompareText(GunBase nGun, WeaponPickable pck)
    {
        var s = base.CompareText(nGun, pck);
        var nWep = nGun as Weapon;
        if (!nWep) return s;
        s = Append(s, maxBullets2, nWep.maxBullets2, "clip size");
        var dps = damage / shootInterval;
        var dps2 = nWep.damage / nWep.shootInterval;

        s = Append(s, dps, dps2, "DPS");
        s = Append(s, WeaponArmorRatio, nWep.WeaponArmorRatio, "armor penetration");


        var nrecoil = nWep.shootBump * nWep.shootInterval;
        var recoil = shootBump * shootInterval;
        s = Append(s, recoil, nrecoil, "Recoil");

        Vector3 nspread = nWep.shootSpread * nWep.shootInterval;
        Vector3 spread = shootSpread * shootInterval;
        s = Append(s, spread.magnitude, nspread.magnitude, "Spread");


        var stability = recoverLerp / shootInterval;
        var stability2 = nWep.recoverLerp / nWep.shootInterval;

        if (stability != stability2)
            s = Append(s, stability, stability2, "stability");
        
        return s;
    }
#endif

}
