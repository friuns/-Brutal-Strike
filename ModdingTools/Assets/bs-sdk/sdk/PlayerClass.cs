
using gui = UnityEngine.GUILayout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
[Flags]
public enum WeaponSetId:int { a1 = 1, a2 = 2, a3 = 4, b1 = 8, b2 = 16, b3 = 32, b4 = 64, b5 = 128 }

public partial class PlayerClass : bs, IOnLoadAsset,IOnLevelEditorGUI
{
    //awake not executed use OnLoadAsset
    public bool alwaysAnimate;
    public bool disableJump;
    // public AnimationSoundDict animationSounds = new AnimationSoundDict();
    
    public BodyDamage bodyDamage = new BodyDamage() {{HumanBodyBones.Head, 2f}};
    
    public float GetBodyDamage(HumanBodyBones bp)
    {
        if (bp == HumanBodyBones.Head)
            return headHitDamage;
        if (IsArm(bp))
            return handHitDamage;
        if (IsLeg(bp))
            return legHitDamage;
        if (bodyDamage.TryGetValue(bp, out float f))
            return f;
        return 1;
    }
    
    public static bool IsArm(HumanBodyBones i)
    {
        return i >= HumanBodyBones.LeftShoulder && i <= HumanBodyBones.RightToes;
//        return new[]
//        {
//            HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
//            HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand
//        }.Contains(i);
    }
    public static bool IsLeg(HumanBodyBones i)
    {
        return i >= HumanBodyBones.LeftUpperLeg && i <= HumanBodyBones.RightFoot;
//        return new[]
//        {
//            HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
//            HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot
//        }.Contains(i);
    }
    
    public Sprite deadIcon;
    public Sprite aliveIcon;
    public Vector2[] weightCurve2;
    //[PropertyAtr]
    //public PlayerSkin[] playerSkins { get { return m_PlayerSkins ?? (m_PlayerSkins = _Game.playerSkins.Values.Where(a => a.playerClassId == id && a.disabled).ToArray()); } }
    //public PlayerSkin[] m_PlayerSkins;

    [FieldAtrStart(inherit = true)]
    public float headHitDamage = 2f;
    public float legHitDamage = .6f;
    public float handHitDamage = .6f;
    
    [FormerlySerializedAs("disabled")]
    public bool hidden;
    
    public ObscuredBool enableKnocked = false; //zombies cannot be rescued
    [EnumFlag]
    public WeaponSetId weaponSets = WeaponSetId.a1; //what weapons allowed for playerClass 
    public ObscuredBool flying = false;
//    public ObscuredBool disableStandInacuracy = true;
    public ObscuredBool caryAnything = false;
    public float targetSpeed = 3.5f;
    public bool enableRopes;
    public float ropeMax=1;
    public float ropePower=1;
    public float ropeCoolDown=1;

    
    [Tooltip("how quick player can change direction")]
    public ObscuredFloat q3StopSpeed = 2;
    public ObscuredFloat q3Decline = 0.9f;
    [Tooltip("minimal velocity")]
    public ObscuredFloat q3minVel = 0.2f;
    public ObscuredInt playerLife = 100;
    //public ObscuredInt runInaccuraty = 1;
    public ObscuredBool canSell = false;
    public ObscuredFloat jump = 6;
    public ObscuredFloat gravity = 1.5f;
    //public ObscuredFloat ClimbFriction = 0;
    [FormerlySerializedAs("weight_Multiplyer")]
    public ObscuredFloat weightMultiplyer = 1;
    public ObscuredFloat fallDamageFactor = 1;
    public ObscuredFloat hitSlowDown = 1;
    
    public bool useThirdPerson;
    public bool zombie;
    public ObscuredInt price = 0;
    public float animatedSpeedFactor = 1;
//    public bool useAnimatedSpeed;
    public SubsetListTeamEnum teams = new SubsetListTeamEnum(new[] { TeamEnum.Terrorists, TeamEnum.CounterTerrorists });
    public bool disabled;
    //public SubsetListGunBase weapons = new SubsetListGunBase(() => bs._Game.playerPrefab.guns) { comparer = AnonymousComparer.Create((GunBase a) => a.id) };
    
    [Validate]
    public int id;
    // public WeaponGroup[] weaponGroups;
    [FieldAtrEnd]
    [Header("       audio")]
    public AudioClip2 hitSound;
    // public AudioClip2 hitScreamSound;
    public AudioClip2 attackScream;
    public AudioClip2 attackScreamSpecial;
    // public AudioClip2 enemyDieSound;
    // public AudioClip2 enemyHitSound;
    public AudioClip2 headShootSound;
    public AudioClip2 dieSound;
    // public AudioClip2 enemyHeadShootSound;
    public AudioClip2 walkSound;
    public AudioClip2 enemyWalkSound;
    public WalkSoundTags walkSoundTags;
    public AudioClip2 fallSound;
    public AudioClip2 jumpSound;
    public AudioClip2 landSound;
    
    public AudioClip2 followMe;
    public AudioClip2 takePoint;
    public AudioClip2 gogogo;
    [FormerlySerializedAs("curve")]
    public AnimationCurve speedToAccuracyCurve;
    
    
    public PlayerState[] playerStates = new PlayerState[Enum<PlayerStateEnum>.values.Length].Fill(a => new PlayerState() { playerState = (PlayerStateEnum)a, Name = ((PlayerStateEnum)a).ToString() });
    [FormerlySerializedAs("useAnimatedSpeed")] public bool useRootMotion;
    public AudioClip2 iGotSuplies;
    [FormerlySerializedAs("botAudio")] 
    // public AudioClip2 botSounds;
    public AudioClip2 botSounds2;
    public AudioClip2 botSounds3;
    public float streffSpeed = .6f;
    [FormerlySerializedAs("gips")] 
    public BodyToTransform gibs = new BodyToTransform();
    public bool EnableKnocked { get { return roomSettings.enableKnocking && enableKnocked; } }
    public static float GetQ3Speed(float targetSpeed, float q3Decline) //returns q3Speed
    {
        var speed = -targetSpeed * (q3Decline - 1) / q3Decline;
         
        speed *= targetSpeed / ((targetSpeed+(targetSpeed + speed * q3Decline))/2); //pw(2)
        
        return speed;
    }

    
#if game
    public QSurfController q3Controller;

    public ObscuredFloat HitSlowDown { get { return Mathf.Max(0.01f, hitSlowDown * roomSettings.hitSlowDown); } }

    // public ObscuredFloat q3Speed { get { return GetQ3Speed(TargetSpeed, q3Decline); } }
    public float TargetSpeed { get { return zombie ? targetSpeed : targetSpeed * roomSettings.playerSpeedFactor; } }
    public static float aliveVsDead { get { return (1f + _Game.playersAll.Count(a => !a.spectator && !a.deadOrZombie )) / (_Game.playersAll.Count(a => !a.spectator  && a.deadOrZombie) + 1f); } }
    public VarParse2 varParse { get { return m_varParse ?? (m_varParse = new VarParse2(Game.varManager,this, "playerClass/" + id, RoomInfo: room)); } } 
    internal VarParse2 m_varParse;
    public AudioClip2 helpSound;
    public AudioClip2 imHit;
    public AudioClip2 thank;
    public AudioClip2 reviveSound;
    public bool useDefaultGibs=true;
    void Start()// expensive calling when instanciating player, check OnLoadAsset
    {
        varParse.UpdateValues();
        
            
    }
    public void OnLoadAsset()
    {
        this.gameObject.SetActive(true);

        if (_Game.playerClasses.ContainsKey(id))
            Debug.LogWarning(_Game.playerClasses[id].name + " is replaced with " + name);
        _Game.playerClasses[id] = this;
        varParse.UpdateValues();
        Game.RegisterOnGameEnabled(() => varParse.roomInfo = room);
        // if (_Game.loaded) //obsolete, for runtime asset loading
        // {
        //     DestroyImmediate(_Player.skin.gameObject);
        //     _Player.playerClassPrefab = this;
        //     _Player.LoadPlayerSkin();
        // }
        OnValidate();
        if (bs.settings.china)
        {
            foreach (var a in dieSound.audioClips)
                Resources.UnloadAsset(a);
            foreach (var a in hitSound.audioClips)
                Resources.UnloadAsset(a);
            foreach (var a in headShootSound.audioClips)
                Resources.UnloadAsset(a);
        }
        if (useDefaultGibs)
            gibs = gameRes.defaultPlayerCLass.gibs;
    }
    public  override void OnValidate() //doesnt work
    {
        base.OnValidate();
        DefineState(PlayerStateEnum.Prone).speed = .3f;
        DefineState(PlayerStateEnum.Prone).controllerHeight = .3f;
        DefineState(PlayerStateEnum.Crouch).speed = .7f;
        DefineState(PlayerStateEnum.Crouch).controllerHeight = .7f;
        // DefineState(PlayerStateEnum.Crouch).controllerOffset = .15f;
        DefineState(PlayerStateEnum.Run).speed = 1.5f;
        DefineState(PlayerStateEnum.InAir).speed = .7f;
        DefineState(PlayerStateEnum.Executing).speed = 0;
        DefineState(PlayerStateEnum.Healing).speed = 0;
        
    }



    [ContextMenu("InitCurve")]
    public void InitCurve()
    {
        var crouch = speedToAccuracyCurve[1];
        crouch.time = .5f;
        var walk = speedToAccuracyCurve[2];
        walk.time = 1f;
        var run = speedToAccuracyCurve[3];
        run.time = 1.5f;
        speedToAccuracyCurve = new AnimationCurve(speedToAccuracyCurve[0], crouch, walk, run);
        SetDirty();
    }
    public float GetAnimationSpeed(PlayerStateEnum state, float speed)
    {
        var maxSpeed = MaxSpeed(state);
        if (maxSpeed == 0) return 1;
        return GetState(state).speedToAnim.Evaluate(speed / maxSpeed); //input always is 0,1 range
    }
    
    public float NormalizeSpeed(float speed)
    {
        float a = Mathf.InverseLerp(0, MaxSpeedD(streffSpeed), speed) / 2f;
        float b = Mathf.InverseLerp(MaxSpeedD(streffSpeed), walkSpeed, speed) / 2f;
        //float c = Mathf.InverseLerp(walkSpeed, MaxSpeed(PlayerStateEnum.Run), speed) / 2f;
        return a + b /*+ c*/;
    }
    internal float walkSpeed { get { return MaxSpeed(PlayerStateEnum.Walk); } }


    public float MaxSpeed(PlayerStateEnum factor)
    {
        var speed = playerStates[(int)factor].speed;
        if (speed == 0) return 0;
        return MaxSpeedD(speed);
//        float maxVel = 0;
//        for (int i = 0; i < 100; i++)
//        {
//            maxVel = Mathf.Lerp(maxVel , maxVel * q3Decline,.5f);
//            maxVel += playerStates[(int)factor].speed* q3Speed;
//            maxVel = Mathf.Lerp(maxVel , maxVel * q3Decline,.5f);
//        }
//        return maxVel;
    }
    
    private float MaxSpeedD(float Speed)
    {
        // if (Speed == 0) return 1;
        return TargetSpeed * Speed;
        // var f = Speed*q3Decline;
        // return -(q3Speed * f) / (f - 1);
    }
    
 


    public PlayerState DefineState(PlayerStateEnum state)
    {
        if (playerStates.Length <= (int)state)
            Array.Resize(ref playerStates, (int)state + 1);

        if (playerStates[(int)state] == null)
            return playerStates[(int)state] = new PlayerState() { Name = state.ToString(), playerState = state };
        return new PlayerState();
    }

    


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // PrintWalkStats();
        //
        //
        // for (int i = 0; i < weightCurve2.Length; i++)
        //     gui.Label("max speed walk "+i+" :" + MaxSpeed(PlayerStateEnum.Walk)*weightCurve2[i].y);
        //
        //
        // SetDirty();
    }
    
    
        
    private void PrintWalkStats()
    {
        // gui.Label("max speed run :" + MaxSpeed(PlayerStateEnum.Run));
        // gui.Label("max speed walk :" + MaxSpeed(PlayerStateEnum.Walk));
        // gui.Label("aiming speed:" + MaxSpeedD(streffSpeed)); 
    }

    public void OnLevelEditorGUI()
    {
        
        var pc = this;
        
        foreach (var skin in _Game.playerSkins.Values.Where(a => a.playerClassId == pc.id))
        {
            if (GuiClasses.BeginVertical2(Base.Concat(skin.name), false))
            {
                skin.varParseSkin.UpdateValues(true);
                gui.EndVertical();
            }
        }
        foreach (var a in FindObjectsOfType<PlayerSkin>())
            a.varParseSkin.UpdateValues();
        PrintWalkStats();
        varParse.DrawGui();
    }
    // public Transform[] randomGibs = new Transform[0];
    
#else
      
    public void OnLoadAsset()
    {
    }
#endif
    public PlayerState GetState(PlayerStateEnum state)
    {
        return playerStates[(int)state];
    } 
}

