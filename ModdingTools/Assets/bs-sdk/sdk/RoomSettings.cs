using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using UnityEngine;
using UnityEngine.Serialization;
#if game
using ExitGames.Client.Photon;
#endif

interface IAutoUpdate
{
    void AutoUpdate();
}
[Serializable]
public class RoomSettings : IAutoUpdate,IVarParseDraw, IVarParseValueChanged
{
    internal bool inited;
#if game 
    [FieldAtr(readOnly = true,priority=-1,devOnly = true)]
    public MapStat map = new MapStat(); //must be serialized for file save, or server sync
    [FieldAtr(dontDraw = true)] public List<MapStat> mapMods = new List<MapStat>();
#endif

    [FieldAtr] public int maxPlayers = 10;
    
    [RoomPulbic]
    public string password="";
    [FieldAtrStart(readOnly = true)] 
    public string country = "";
    
    [RoomPulbic]
    public int version;
    [RoomPulbic]
    public int mpVersion;
    public bool androidOnly =false;
    [FieldAtrStart]
    [Header("GamePlay")]
    public bool useSurf;
    public float fallDamageFactor=0.7f;
    public ObscuredFloat hitSlowDown = .5f;
    public bool allowManyTeams;
    public bool enableThirdPerson = true;
    public ObscuredFloat weightMultiplier=1;
    public ObscuredInt respawnTime = 3;
    public ObscuredFloat gameSpeed = 1;
    public ObscuredFloat matchTime =0;
    public ObscuredFloat fragLimit = 30; 
    public ObscuredFloat waitForPlayersTime = 0;
    public ObscuredFloat gravity = -9.81f;
    public ObscuredBool useDefaultCamera = false;
    public ObscuredBool resetPlayersOnNextRound;
    public int maxPing = 999;
    public bool enableHostage;
    public bool missionRespawn;
    // public int maxPlayersPerTeams = 100;
    public bool allowSpectator=true;
    public bool validateBulletsTwoWays;
    public float runSpread=1;
    public float shootSpread=1;
    public bool enableRun;
    public bool enableProne = false;
    public bool enablePeek = true;
    public bool selfRevive=false;
    public ObscuredInt knockTime = 20;
    public bool enableKnocking=true;
    public bool showHealth = true;
    public bool forceTeamChat;
    public bool dontKillKnocked =false;
    public bool ignorePlayerCollision;
    public bool disableLootDropOnDie=false;
    public bool disableCrosshair;
    public bool autoShot;
    public bool enableParachute;
    // [FieldAtr(inherit = true, dontDraw = true, gameType = GameType.DeathMatch | GameType.TDM)]
    public float reviveTime = 6;
    public bool canBuyAlways;
    [FieldAtr(dontDraw = true)]
    public int level=1;
    [Header("BotSettings")]
    public int maxRounds=9;
    #if game
    public bool enableBotSupport { get { return map.enableBotSupport; } set { map.enableBotSupport = value; } }
    #endif
    public bool updateNavMesh=false;
    public float botDificulty = 1;
    public bool dynamicBotDificulty=true;
//    public bool increaseDificulty=true;
    public int botTeamCount = 3;
    public int botsPerTeam = 2;
    public bool alliedBots;
    public bool StartAlone;
    public bool hardBots;
    public float botNonAccuracy=6;
//    public bool disableBotKnocking=true;
    public bool botScream=true;
     [FieldAtrStart(gameType = GameTypes.noRespawnTypes)] //2do
    [Header("Zone")]
    public ObscuredBool randomZonePlacement = false;
    public ObscuredBool enablePlayerDrop = false;
    public Vector3 airPlaneOffset = new Vector3(0, 290, -90);
    
    public ObscuredBool shrinkZone = false;
    public ObscuredFloat zoneSize = 1000;

    public ObscuredFloat shrinkSpeed = 1;
    public int zoneDamage = 0;
    public int moneyEachSecond = 0;
    [FieldAtr(preGameOnly = true,inherit = true)]
    public ObscuredBool enableZombies = false;
    public ObscuredBool enableZombiesMission = false;
    public float zombiePowerTime=1;
    public ObscuredBool enableHelicopter =false;
    public float helicopterWaitTime = 120;

    [FieldAtrStart()]
    // public Team[] teams = new Team[Enum<TeamEnum>.values.Length].Fill(a => new Team() {team = (TeamEnum) a, name = ((TeamEnum) a).ToString()});

    [Header("Loot Drop")]
    public ObscuredBool enableAirDrop = false;
    public ObscuredFloat airDropInterval = 60 * 2;
    public float lootRespawnTime=30;
    public ObscuredFloat lootProbability = 1;
    //public GameStats curGameScoreRef = new GameStats();
    public int generateRandomLoot;

     
    

    [Header("WeaponShop")]
    public float scoreMultiplier=1;
    public ObscuredInt startMoney =  800;
    public ObscuredInt nextRoundMoney = 3000;
    public ObscuredBool randomBuyWeapons = false;
    public bool resetMoneyEachRound;
    public bool disableLoot;
    [Header("Weapon")] 
    public float hardRecoil = .5f;
    public float softRecoil = 1f;
    public bool awpOnly;
    public float autoAimDist=1;
    public ObscuredFloat autoAim = 1;
    
    public ObscuredBool canAimWithoutScope = true;
    public bool disableAttachments;
    
    public float spawnProtection=3;
    public bool unlimitedAmmo;
    public ObscuredBool ShootThroughWalls = true;
    public ObscuredFloat bulletPassFactor = 1;
    public ObscuredBool noReloads = false;
    public ObscuredFloat bulletSpeedFactor = 1;
    public ObscuredFloat recoilFactor = 1;
    // public ObscuredBool noCameraRecoil = false;
    public ObscuredFloat bulletGravity = 1;
    public ObscuredFloat bulletDrag = 1;
    public bool stamina=false;
    public bool jumpShoot=true;
    public bool grenadeCooking=true;
    public bool randomPrice;
    [Header("Misc")] 
    public bool disableShadowSampling;
    public bool disableTreesAndGrass;
    public float playerSpeedFactor=1;
    public float playerHorizontalMove = .7f;
    public float playerHealthFactor = 1;
    public float zombieHealthFactor = 3;
//    public float ClimbFriction;
//    public float ClimbFriction2;
    public bool allowSpawnsInUnreachableArea;
    public int spawnAboveHeight;
    public bool enableSliding = true;
    public bool enableSlopeFriction = true;
    public float slidingAngle = .7f;
    public float ClimbFriction3 = .5f;
    public float SlideSpeed = 10;
    public bool teamAutoBalance=true;
    // public bool swapTeams;
    public int imposterCount = 1;
    public bool randomEachTime;
    public float bombTimeSec = 45f;
    public bool allowRespawnOnEnemyTeam;
    public float lifeRegenSpeed = 20;
    
    public int hostUserID=-1;
    
  
    public int seed;
   
    public  float autoAim_Y = 0;
    public bool androidInertia;
    public bool allowPositiveDamage;
    public bool allowFriendlyFire;
    public bool showCarShop = true;
    public float floorLevel;
    [FieldAtrEnd]
    public ObscuredBool specialGun = false;
    
    public bool enableShooting = true;

    public bool searchEnemy = true;
    public bool enableHearing = true;
    public bool coverSystem = true;
    public bool stopOnShoot = true;

#if game
    public GameType gameType { get { return map.gameType; } set { map.gameType = value; } }
    public bool ranked { get { return gameType.IsRanked(); } }
#endif
    public RoomSettings Clone()
    {
        return (RoomSettings) MemberwiseClone();
    }

    public void AutoUpdate()
    {
    }

#if game
    public bool OnVarParseDraw(FieldCache fieldCache)
    {
        return true;
    }
    public void RefreshMaxPing()
    {
        if (maxPing == 999)
            maxPing = Mathf.Max((int) (bs.connectionManager.bestPeer?.ping / .666 ?? 900), 90);
    }
    
    public void OnValueChanged(string key, FieldCache fc)
    {
        Physics.IgnoreLayerCollision(Layer.player, Layer.ignoreRayCast, enablePlayerDrop);
    }
    
#endif
    
}
