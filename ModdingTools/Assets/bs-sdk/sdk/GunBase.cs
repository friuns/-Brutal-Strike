using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using EnumsNET;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using kv = System.Collections.Generic.KeyValuePair<UnityEngine.AnimationClip, UnityEngine.AnimationClip>;



public class GunBase : bsNetwork, IOnLoadAsset, IOnLevelEditorGUI,IDontDisable, IPreLoadAsset,IVarParseValueChanged
{
    
    
//    public new Quaternion rot { get { return pl.CamRnd.rotation; } }
    // public WeaponStats weaponScore = null; 
    public RaycastHit lastHit;

    [FieldAtrStart]
    [Header("       settings")]
    

    
#if game
    [HideInInspector]
    public int version;
#else
    public int version = 1;
#endif
    
#if game
    public bool useEuler ;
#else
    public bool useEuler = true;
#endif
    
    public WeaponSetId weaponSetId = WeaponSetId.a1;
    public SubsetListTeamEnum teams = new SubsetListTeamEnum(Enum<TeamEnum>.values);
    [ContextMenu("FixTeams")]
    public void FixTeams()
    {
        teams = new SubsetListTeamEnum(Enum<TeamEnum>.values);
    }
    public WeaponType weaponType = WeaponType.Rifle; //used in attachments and old weaponshop
    [Validate]
    public string gunName = "";
    public ObscuredInt defCount = 0;
    //[FormerlySerializedAs("defsecondaryCount")]
    [FormerlySerializedAs("maxBullets")]
    public ObscuredInt secondaryCountDef = 0;
    public ObscuredInt maxCount = 1;//to avoid too many grenades
    public ObscuredFloat runSpeed = 1;
    [Tooltip("speed of movement while shooting")]
    public float runSpeedShooting = 1;
    public ObscuredFloat reloadTime = 1;
    public ObscuredFloat drawTime = .5f;
    public ObscuredBool canAimWhileShooting = true;
    public bool canDrop { get { return weaponPickable!=null; } }
    public bool enableLookAtAnimation = true;
    public ObscuredFloat weight = .1f;
    [FormerlySerializedAs("price")]
    public ObscuredInt m_price = 600;
    public bool disabled;
    public bool prematchOnly;
    public float scopeFov = 1;
    public ObscuredInt dropAmmount = 1;
    public ObscuredFloat m_aimSpeed = 16.5f;
    public float gravityFactor=1;
    //public Slot slot = new Slot();
    [FieldAtrEnd]
    public bool allowFriendlyFire;

    [Header("       audio")]
    public AudioClip2 draw;

    public List<AnimationEvents> animationEvents = new List<AnimationEvents>();

    [Header("       models")]
    [Validate]
    public GunSkin gunSkinPrefab;
    [Validate]
    public WeaponPickable weaponPickable;
    [Validate]
    public HandsSkin handsPrefab;

    [Header("       animations")]
    public AnimationDict thirdPersonAnimations = new AnimationDict();
    [Header("       other")]
    [Validate]
    public int id = 0;
    internal HandsSkin Hands;
    internal GunSkin gunSkin;
    public WeaponGroupE groupNumber;
    [Tooltip("Пробиваемость броника")]
    public ObscuredFloat WeaponArmorRatio = 1;
    public ObscuredBool special = false;
    
    
    [ContextMenu("InitPickable")]
    public void InitPickable()
    {
        if (weaponPickable)
        {
            weaponPickable.weapons[id].m_count = dropAmmount;
            weaponPickable.weapons[id].m_secondaryCount = secondaryCountDef;
            weaponPickable.arrayId = id;
            weaponPickable.SetDirty();
        }
    }
#if !game
    public void OnValidate()
    {
        InitPickable();
    }
#endif
    
#if game
    internal new bool selected; //same as pl.curGun==this
    public Sprite weaponIcon { get { return weaponPickable != null ? weaponPickable.icon : this is Knife ? gameRes.knifeIcon : null; } }
    public int price { get { return roomSettings.randomPrice ? new MyRandom(unchecked(_Game.seed+round+id)).Range(m_price/2,m_price*6/4) : (int) m_price; } }

    public bool hide { get { return this is Ammo && roomSettings.unlimitedAmmo; } }

    public new Vector3 pos { get { return pl.CamRnd.pos; } }

    public override AudioSource audio { get { return pl ? pl.playerAudioSource: base.audio; } }
    


    public virtual List<Attachment> attachments { get { return Empty<Attachment>.list; } }

    
    internal bool have;
    public bool canDropAndHave { get { return canDrop && have; } }
    public AudioClip2 zoomSound;

    public new bool enabled { get { return base.enabled; } set { base.enabled = value; } } //warning OnEnable used 
    
    public bool groupSelected { get { return pl.lastWep[(int) groupNumber] == this; } }
    internal bool active { get { return enabled; } set { if(enabled!=value && !value) OnDisable2(); enabled = value; } } //to avoid OnDestroy behaviour  
    public override void OnDestroy()
    {
        if (gunSkin)
            Destroy(gunSkin.gameObject);
        
        base.OnDestroy();
    }

    // private bool callOnce;
    // public Action act1;
    // private int frameCalled;
    public void SetCount2(int value)
    {
        if (value == 0 && info.count != 0 || value != 0 && info.count == 0)
        {
            info.count = value;
            OnCountChangedRefresh();
        }
        else
            info.count = value;
    }
    public virtual void OnCountChangedRefresh(bool fast=false,bool skipCheck=false)
    {
        
        // if (frameCalled == Time.frameCount) return;
        // frameCalled = Time.frameCount;
        var pl = this.pl;

        UpdateCount();
        if(!fast)
            pl.countChanged?.Invoke();
        if (pl.observing)
            _ObsCamera.handsNeedRefresh = true;


        if (gunSkin)
        {
            if (selected && !have && !skipCheck && !pl.dead)
                Debug2.Log(pl + "  dont have weapon but selected " + gunName + " cnt " + Count, pl?.controller?.gameObject);


            var render = (have || selected && !IsMine) && (passive || selected) && !pl.deadOrKnocked;
            gunSkin.transformCache.active = render;
            gunSkin.enabled = render;
            if (render && passive)
                gunSkin.UpdateTextures();
            
            gunSkin.attachTo = render ? placeholder ?? pl.skin?.tr : pl.tr;

        }
    }
    public  void UpdateCount()
    {
        Disabled = !pl.playerClassPrefab.weaponSets.HasAnyFlags(weaponSetId) || prematchOnly && !_Game.waitForPls || disabled || this is Attachment && roomSettings.disableAttachments;
        
        
        have = Count > 0 && !Disabled;
        canSelect = have && !Disabled && !passive; 
        active = !pl.dead && (have && passive || selected && !Disabled); //if use deadOrKnocked muzzleflash bugs, because guns never gets disabled
    }


    public object PrintInfo()
    {
        var max = pl.guns.Where(a => a.have).Max(a => (float)a.weight);
        if (weight > 0)
            return Format("{0} <color=#{1}>{2}</color>kg", info, ColorUtility.ToHtmlStringRGBA(Color.Lerp(Color.green, Color.red, weight / max)), weight);
        else
            return info;
    } 
//    [Cache()] 
    

    public VarParse2 varParse
    {
        get
        {
            return m_varParseSkin ?? (m_varParseSkin = new VarParse2(Game.varManager,this, "weapons/" + id, RoomInfo: room, DrawAll: isMaster));
        }
    } 
    internal VarParse2 m_varParseSkin;

    public Input2Base Input2 { get { return pl.Input2; } }

    // public bool allowed { get { return !Disabled; } }
//    private Cache<bool> m_disabled = new Cache<bool>();
//    public static Action invalidateDisabled = delegate { };
    internal bool Disabled;
    public bool teamBuyDisabled => pl && room.sets.mpVersion >= 17 && !(teamGame ? teams.Contains(pl.team) : true);

    internal bool canSelect;
    
    public int gunPriorty { get { return groupNumber == WeaponGroupE.None ? 999 : (int) groupNumber; } }
    public WeaponGroup weaponGroup { get { return pl.weaponGroups[(int)groupNumber]; } }

    internal bool remoteMouseButtonDown;
    internal float EnableTime;
    // private Player m_pl;
    internal Player pl;//{ get { return m_pl ? m_pl : (m_pl = GetComponentInParent<Player>()); } }
    public virtual bool MouseButton { get { return pl.MouseButton; } }
    internal float reloadingTime = -999;
    
    public virtual bool isReloading { get { return Time.time - reloadingTime < reloadTime; } }
    
    public virtual bool isReloadingOrDraw { get { return Time.time - EnableTime < drawTime || Time.time - reloadingTime < reloadTime; } }

    
    public Transform placeholder { get { return pl.skin?.GetBodyTransform(gunSkin.attachTo2); } }//if optimize make sure that its refreshed when skin detached

    public VirtualTransform cam { get { return pl.Cam; } }
    //internal int cloneOf=-1;
    public override string ToString()
    {
        return gunName;
    }
    public void OnValueChanged(string key, FieldCache fc)
    {
        if (fc.inited && pl) //fix for disabled
            UpdateCount();
    }
    public virtual void OnPreLoadAsset()
    {
        var transforms = transform.ToArray();
        foreach (Transform a in transforms)
        {
            a.transform.parent = transform.parent;
            a.gameObject.SetActive(true);
            // a.GetComponent<IOnLoadAsset>()?.OnLoadAsset();
        }
    }
    public void OnLevelEditorGUI()
    {
        varParse.DrawGui();
    }
    [HideInInspector]
    public bool assetLoaded;
    public virtual void OnLoadAsset()
    {
        
        if (assetLoaded)
        {
            Debug.LogError("asset already loaded", this);
            
            throw new Exception("asset already loaded");
        }
        assetLoaded = true;

        
        
        print(LogTypes.other, "Loading ", gameObject);
        //foreach (Player pl in _Game.players.Concat2(_Game.playerPrefab).ToArray())
        {
            var pl = _Game.playerPrefab;

            if (pl.gunsDict.ContainsKey(id))
                Debug.LogWarning(name + " already exists with " + pl.gunsDict[id].name);
            var gun = pl.gunsDict[id] = Instantiate(this, pl.curGun.parent);
            gun.name = name;

            //gun.OnReset();
            if (weaponPickable)
            {
                weaponPickable.weapons.Add(this, dropAmmount);
                weaponPickable.arrayId = id;
                // gun.weaponPickable.gunInfo.count = gun.dropAmmount;
                // gun.weaponPickable.gunInfo.secondaryCount = gun.secondaryCountDef;
            }

            Game.RegisterOnGameEnabled(() => gun.varParse.UpdateValues());
        }
        if (_Game.loaded)
        {
            foreach (var pl in _Game.playersAll)
            {
                var gun = pl.gunsDict[id] = Instantiate(this, pl.curGun.parent);
                gun.have = true;
                gun.info = new GunInfo(this) {m_count = 9999, m_secondaryCount = 9999};
                gun.Init();
            }

            _Player.SelectGun(this.id);
        }
        
    }

    public new void StopAllCoroutines()
    {
        timer?.RemoveAll();
        base.StopAllCoroutines();
    }

    public override void Awake()
    {
        base.Awake();
        pl = GetComponentInParent<Player>();
        
        if (info == null) info = CreateInfo();
        prints("gunbase awake " + name);
//        if (!gunSkinPrefab)
//            gunSkinPrefab = gameRes.gunSkinPrefab;
        if (!handsPrefab)
            handsPrefab = gameRes.handsPrefab;
        enabled = false;
//        pl = GetComponentInParent<Player>();
        InitAnimationOverride();
        varParse.UpdateValues();
        
        
        //if (IsMine)
        //    InitNetwork();
    }
    public virtual void InitNetwork()
    {
        if (Count != defCount)
            CallRPC(SetCount, Count);
    }
    public virtual void Start()
    {
//        Init();
        
        
    }
    public virtual void OnSell()
    {
        pl.inGameMoney += price;
        Count--;
    }
    [HideInInspector]
    public bool inited;
    public virtual void OnSelectGun(bool selected)
    {
        // print("gun selected ", name, " ", selected);
        if (this.selected == selected) return;
        this.selected = selected;
         
        if (selected && !pl.dead)
        {
            pl.weaponAnimationEventsAudio.PlayOneShot(draw);
            if (!pl.zombie)
                using (Profile("SwitchAnimatorController"))
                    pl.skin.animator.SwitchAnimatorController(animatorOverride.GetCached());
            // pl.skin.animator.parameters[0].type
            Draw();

            if (gunSkin) gunSkin.UpdateTextures();
        }
        OnCountChangedRefresh(true);
    }
    

    static List<kv> clipToclip = new List<kv>();
    public Cache<AnimatorOverrideController> animatorOverride = new Cache<AnimatorOverrideController>();

    private void InitAnimationOverride()
    {
        if (!animatorOverride.inited) animatorOverride.Setup(() =>
        {
            var animatorOverride = pl.skin.animator.runtimeAnimatorController is AnimatorOverrideController ? (AnimatorOverrideController)Instantiate(pl.skin.animator.runtimeAnimatorController) : new AnimatorOverrideController(pl.skin.animator.runtimeAnimatorController);
            
            clipToclip.Clear();
            foreach (KeyValuePair<Anims, AnimationClip> anim in pl.skin.animations)
            {
                AnimationClip clip;
                if (thirdPersonAnimations.TryGetValue(anim.Key, out clip))
                    clipToclip.Add(new kv(anim.Value, clip));
                else
                    clipToclip.Add(new kv(anim.Value, anim.Value));
            }

            animatorOverride.ApplyOverrides(clipToclip);
            return animatorOverride;
        }, () => pl.skin);
    }
    public virtual bool isShooting { get; set; }
    internal float shootTime;
    public virtual void Init()
    {
        if (inited) return;
        inited = true;
        LoadSkin();
        OnCountChangedRefresh();
        
    }

    public void LoadSkin()
    {
        LoadHands(handsPrefab);

        if (gunSkinPrefab)
            LoadGunSkin(gunSkinPrefab);
    }
    public void LoadGunSkin(GunSkin gunSkinPrefab)
    {
        gunSkin = InstanciateAndParent(gunSkinPrefab, pl.transform);
        gunSkin.name = gunSkinPrefab.name; // + " gunskin";
        gunSkin.gunBase = this;
        gunSkin.transformCache.IncludeColliders();
        
        pl.transformCache.Add(gunSkin.transformCache);
        gunSkin.transformCache.active = false;
    }
    public bool LoadHands(HandsSkin handsSkin)
    {
        if (!_ObsCamera.hands.TryGetValue(handsSkin, out Hands))
        {
            _ObsCamera.hands[handsSkin] = Hands = InstanciateAndParent(handsSkin, _ObsCamera.handsPlaceholder.GetChild(0).GetChild(0));
            _ObsCamera.handsNeedRefresh = true;
            Hands.name = handsSkin.name;
            Hands.gameObject.SetActive2(false);
            return true;
        }
        return false;
    }
    public virtual void Update2()
    {
        if (IsMine && !have)
            pl.SelectNextGun();

        if (pl.playerState == PlayerStateEnum.Executing)
            EnableTime = TimeCached.time; 
        //if (!ArrayEqualOrNull(attachments, cachedAttachments.oldValue)) //do
        //    Draw();
    }
    

    private void Draw()
    {
        EnableTime = Time.time;
        pl.SetTrigger(AnimParams.Interrupt);
        PlayAnimation(Anims.draw, playFromBegining: true);
    }

    public void RPCSetMouseButton(bool value)
    {
        if (value != remoteMouseButtonDown)
        {
            var ip = pl.GetDistanceToCursor(out Vector3 dc);
            CallRPC(SetMouseButton, value, pl.hpos, ip,dc);
        }
    }
    internal ObscuredFloat lastMouseButtonDown = 0;
    public MyRandom Random = new MyRandom();
    [PunRPC]
    public virtual void SetMouseButton(bool value, Vector3 hpos,int ip, Vector3 dc)
    {
        Random = new MyRandom(messageFrame);
        if (value)
            lastMouseButtonDown = Time.time;
        remoteMouseButtonDown = value;
    }
    public virtual void Reload()
    {
    }


    
    public int lastBought;
    public int bought;
    public virtual void Reset()
    {
        if (Mission && this is Attachment b && b.attachment == AttachmentType.silencer)
        {
            defCount = 1;
            b.canAttachToWeapon = new SubsetListWeaponType(new[] {WeaponType.Pistol});
        }
        lastBought = bought;
        bought = 0;
        info.count = _Game.waitForPls && !(this is Armor) ? dropAmmount : teamBuyDisabled ? 0 : defCount;
        info.secondaryCount = secondaryCountDef;
        OnCountChangedRefresh();
        if (defCount > 0)
            pl.lastWep[(int) groupNumber] = this;
    }
    public virtual void ResetBullets()
    {
        remoteMouseButtonDown = false;
    }
    public void SetTrigger(AnimParams p)
    {
        pl.SetTrigger(p);
        if (pl.observing)
            Hands.animator.SetTrigger(p);
    }

    public void SetBool(AnimParams p, bool b)
    {
        pl.SetBool(p, b);
        if (pl.observing)
            Hands.animator.SetBool(p, b);
    }
    public void RPCPlayAnimation(Anims anim, float trans = 0)
    {
        CallRPC(PlayAnimation, anim, trans, false, -1);
    }
    [PunRPC]
    public void PlayAnimation(Anims anim, float trans = 0, bool playFromBegining = false, int layer = -1)
    {
        if (pl.observing)
            Hands.Fade(anim.GetName(), trans, playFromBegining, layer);

        if (pl.deadOrKnocked) return;

        pl.animator.Play(anim.ToStringC(), -1, 0);
//        pl.Fade(anim, trans, playFromBegining);

        foreach (var e in pl.observing && Hands.animationEvents.Count > 0 ? Hands.animationEvents : animationEvents)
            if (e.anim == anim /*&& Magnitude(pl.hpos - mainCameraPos) < 20*/)
                MyTimer.DelayCallP(ref timer,(e, pl,this), this_ =>
                {
                    if (this_.Item3.selected && !pl.deadOrKnocked)
                        this_.pl.weaponAnimationEventsAudio.PlayOneShot(this_.e.audioClip);
                }, e.time / Hands.animationSpeed);
                // StartCoroutine(PlayDelayed(a.time / Hands.animationSpeed, a.audioClip));
    }
    private ITimer timer;
    // MyTimerBase timer = new MyTimerBase()
    public virtual void OnDisable() //when deselected
    {
        StopAllCoroutines();
    }
    
    // public IEnumerator PlayDelayed(float time, AudioClip2 clip)
    // {
    //     yield return new WaitForSeconds(time);
    //     pl.weaponAnimationEventsAudio.PlayOneShot(clip); 
    // }
    public void RpcDropWeapon(bool dropAll=false)
    {
        if (!canDrop || !weaponPickable) return;
        try
        {
            WeaponPickable drop = InstantiateSceneObject(weaponPickable, pl.hpos, pl.Cam.localRotation, scene: false); //PhotonNetwork.AllocateViewID() RPC needs to be buffered 

            if (room.sets.mpVersion < 14)
                CallRPC(DropInit, drop.viewId);
            else
            {
                int dr = /*dropAll ? info.count :*/ Mathf.Min(dropAmmount, info.count);
                GunInfo gunInfo = info.Take(dr);
                
                if (this is Weapon w)
                    if (w.ammo.have)
                    {
                        gunInfo.secondaryCount += w.ammoCount;
                        w.ammoCount = 0;
                        // w.ammo.RpcDropWeapon(true);
                    }
                
                CallRPC(DropInit, drop.viewId, gunInfo, info);
                
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }


    }
    
    [PunRPC]
    protected virtual void DropInit(int id, GunInfo gunInfo, GunInfo left)
    {
        info = left;
        var drop = ToObject<WeaponPickable>(id);
        drop.gunBase = this;
        drop.m_dropOwner = dropOwner;
        drop.InitDrop(true, gunInfo, false);
        drop.rigidbody.isKinematic = false;
        drop.rigidbody.AddForce(pl.Cam.forward * 150);
        
        var group = drop.gun.groupNumber; //on drop armor replace with another
        if (pl.lastWep[(int) group]?.id == drop.gun.id && !pl.lastWep[(int) group].have)
            pl.lastWep[(int) group] = pl.FindGun(group,false);
        OnCountChangedRefresh(skipCheck: true);
    }
    

    [PunRPC]
    protected void DropInit(int id) //old
    {
        var drop = ToObject<WeaponPickable>(id);
        var dr = Mathf.Min(dropAmmount, info.count);
        var gunInfo = info.Take(dr);
        drop.gunBase = this;
        drop.InitDrop(true, gunInfo, false);
        OnCountChangedRefresh(skipCheck: true);

        drop.rigidbody.isKinematic = false;
        drop.rigidbody.AddForce(pl.Cam.forward * 150);
        
        var group = drop.gun.groupNumber; //on drop armor replace with another
        if (pl.lastWep[(int) group]?.id == drop.gun.id && !pl.lastWep[(int) group].have)
            pl.lastWep[(int) group] = pl.FindGun(group,false);

    }
    

 
    
    public bool canBuy { get { return price > 0 && !Disabled && !special && !hide && !teamBuyDisabled; } } //if price == 0 gun is disabled for buying
    public bool canTake { get { return CanTakeAmmount()>0; } }

    public bool CanTake(int many, bool auto = false)
    {
        
        return CanTakeAmmount(auto) >= many;
        // return !disabled && (countInGroup + many <= (auto ? weaponGroup.preferedCount : weaponGroup.maxCount) || pl.playerClassPrefab.caryAnything) && Count+many <= maxCount;
    }


    public int CanTakeAmmount(bool auto = false)
    {
        // if (disabled) return 0;
        if (Disabled) return 0;
        if (pl.playerClassPrefab.caryAnything) return dropAmmount;
        return Mathf.Min((auto ? weaponGroup.preferedCount : weaponGroup.maxCount) - countInGroup, maxCount - Count);
    }
    
    public int countInGroup { get { return pl.guns.SumWhere(a => a.Count, a => a.have && groupNumber == a.groupNumber); } }
    
    internal bool passive
    {
        get { return this is IPassive; }
    }

    
    public override void OnPlConnected(PhotonPlayer photonPlayer)
    {
        if (Count != defCount)
            CallRPC(SetCount, Count); //should be before customServer
        if (customServer) return;
        if(selected)
            pl.CallRPC(pl.SelectGun, id);
    }
#if UNITY_EDITOR
    //public virtual void OnValidate()
    //{
    //    if(weight==0)
    //        weight=.1f;
    //    //if (id == 0)
    //    //    id = GetGuid();


    //}

    public virtual void CtxParse2(string key, string value)
    {
        if (key == "WeaponPrice")
            m_price = int.Parse(value);
        if (key == "weight")
            weight = float.Parse(value) / 10f;
    }

    [ContextMenu("CtxParse")]
    public void CtxParse()
    {
#if UNITY_EDITOR
        string txt = File.ReadAllText("ctx/weapon_" + name.ToLower() + ".txt");

        foreach (Match a in Regex.Matches(txt, @"""(.*?)""[\t ]*""?([\w./#]+)"))
        {
            CtxParse2(a.Groups[1].Value, a.Groups[2].Value);
        }

        // ParseAnimation();
#endif

    }
    [ContextMenu("ParseAnimation")]
    public void ParseAnimation()
    {
        RuntimeAnimatorController rc = handsPrefab.GetComponentInChildren<Animator>(true).runtimeAnimatorController;

        AnimatorController ac = rc as AnimatorController;
        var over = rc as AnimatorOverrideController;

        if (over != null)
            ac = over.runtimeAnimatorController as AnimatorController;

        foreach (var a in GetAnims(ac))
        {
            Anims anim;
            if (Enum<Anims>.TryParse(a.name, out anim))
            {
                var clip = a.motion as AnimationClip;
                if (over != null)
                    clip = over[clip];
                ParseAnimEditor(anim, clip);
            }
        }
    }


#endif
    public virtual void ParseAnimEditor(Anims anim, AnimationClip clip)
    {
        if (anim == Anims.draw)
            drawTime = clip.length;
        if (anim == Anims.reload)
            reloadTime = clip.length;
    }

    public virtual void OnDisable2()
    {
//        StopAllCoroutines(); //obsolete we use custom cor
    }
    [ContextMenu("Fix Volume")]
    void FixVolume()
    {
        foreach (var a in animationEvents)
            a.audioClip.volume = 1;

    }
    public string CompareTextSimple(GunBase wep)
    {
        return Concat(gunName,Player.InsertImage(weaponIcon), " replace with ", wep.gunName,Player.InsertImage(wep.weaponIcon));
    }
    public virtual string CompareText(GunBase wep,WeaponPickable pck)  //wep == pck because pck dont have pl
    {
        
        
        var s = Concat(gunName,Player.InsertImage(weaponIcon), " replace with ", wep.gunName,Player.InsertImage(pck.gun.weaponIcon));
//        var wepWeight = wep.weight-weight;
//        if (wepWeight !=0)
//            s = Concat(s, "\nweight: ", ColorText(Concat("+", wepWeight, "kg"), wepWeight > 0 ? Color.red : Color.green));

        s = Append(s, wep.weight,weight, "weight", "kg", "green", "red");

//        if(pck.arrayID == id)
        s = Append(s, pck.gunInfo.secondaryCount,info.secondaryCount, this is Armor ? "armor" : "ammo", "");

        if (pck.gunInfo.count > 1)
            s = t + s + "\nCount: " + pck.gunInfo.count;
        return s;
    }
    protected string Append(string s, float a, float b, string title, string postfix = "%", string red = "red", string green = "green")
    {
        if (a != b)
        {
            float f = postfix == "%" ? (b / a - 1f) * 100 : a - b;
            s = Format("{0}\n<color={3}>{2}: {1:+#;-#;+0}{4}</color>", s, f, title, f < 0 ? red : green, postfix);
        }
        return s;
    }

    public virtual void OnTake(GunInfo info)
    {
        this.info += info;
        if ( /*passive &&*/ info.IsBetterThan(pl.lastWep[(int) groupNumber]?.info))
            pl.lastWep[(int) groupNumber] = this;
        OnCountChangedRefresh();

        if(pl.observing)
            _Hud.CenterTextUpd(GreenColorText(t + "You picked up " + name));
        pl.PlayTakeSound();

    }
    public virtual ObscuredFloat visualInaccuracy { get; set; }
//    public void UpdateAlways()
//    {
//    }
    public virtual bool useless
    {
        get { return false; }
    }
    
    public virtual bool IsBetterThan(GunBase b)
    {
        if (b == null) return true;
        if(price>b.price)
            return true;
        if (weaponPickable?.probability > b.weaponPickable?.probability)
            return true;
        return false;
    }
    
    internal GunInfo info;
    public Player dropOwner;

    internal int Count
    {
        get
        {
            return info.count;
        }
        set //why not executed on drop?
        {
            if (Count != value && PhotonNetwork.connected && IsMine)
                CallRPC(SetCount, value);
            info.count = value;
            OnCountChangedRefresh();
        }
    }
    public virtual GunInfo CreateInfo()
    {
        return new GunInfo(this);
    }
  
    [PunRPCBuffered]
    public virtual void SetCount(int Count) //why not executed on drop or reset?
    {
        //setCount =Count;
        //if(setCount!=Count)SetCount(info.Count) //todo lazy sync
        info.count = Count;
        OnCountChangedRefresh();
    }

    // private static void GetAll()
    // {
    //     var cnt = SceneManager.sceneCount;
    //     for (int i = 0; i < cnt; i++)
    //     {
    //         var d = SceneManager.GetSceneAt(i);
    //         var obs = d.GetRootGameObjects();
    //         foreach (var g in obs)
    //         {
    //             var dds = g.GetComponentsInChildren<GunBase>();
    //         }
    //     }
    // }
    #if UNITY_EDITOR
    [ContextMenu("Swallow")]
    
    public void Swallow()
    {
        GunBase[] gs = Resources.FindObjectsOfTypeAll<GunBase>();
        foreach(GunBase a in gs)
            if (a.gameObject.scene.IsValid())
            {
                if (a.gunSkinPrefab) a.gunSkinPrefab.transform.parent =a. transform;
                if(a.handsPrefab)a.handsPrefab.transform.parent = a.transform;
                if (a.weaponPickable) a.weaponPickable.transform.parent = a.transform;
                
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
    }
 
#endif

    
#else
    public void OnLoadAsset()
    {
    }
#endif
#if game
    public string ToStringWithIcon()
    {
        return t + gunName + Player.InsertImage(weaponIcon);
    }
    #endif
    #if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
#if game
        GUILayout.Label("Have:"+have);
        GUILayout.Label("Disabled:" + Disabled);
        GUILayout.Label("Count:"+(info?.count??0));
#endif
        
        // EditorGUILayout.ObjectField("guskin", gunSkin?.transform, typeof(Transform), true);
        // EditorGUILayout.ObjectField("hands", Hands?.transform, typeof(Transform), true);
    }
#endif
}


