using System.Linq;
using UnityEngine;


public class Hostage : ItemBase, IOnPlayerStay
{
    public new static Hostage prefab;
    public Vector3 startPos;
    public TeamEnum team;
    public AudioClip2 hostageResued;
    public AudioClip[] clips;
    public static AudioClip lastPlayed;
#if game
    public bool playerHostage { get { return team != TeamEnum.None; } }
    public override void OnLoadAsset()
    {
        prefab = this;
        base.OnLoadAsset();
    }
    public bool active
    {
        get
        {
            return enabled;
        }
        set
        {
            enabled = value;
            foreach (Transform a in GetTransforms())
                a.gameObject.SetActive(value);
        }
    }
    [PunRPCBuffered]
    public void SetActive(bool b)
    {
        active = b;
    }
    [ContextMenu("sync")]
    public void Sync()
    {
        if (!active)
            CallRPC(SetActive, false);
    }
    
    
    
    
    public override void Start()
    {
        base.Start();
        pos = DownCast2(pos);
        startPos = pos;
        var rand =  new MyRandom(photonView.viewID);
        randomOffset = ZeroY(rand.insideUnitSphere) * .4f;
        transform.forward = ZeroY(rand.insideUnitSphere);
        
        if (playerHostage)
        {
            foreach (var a in GetInstances<Destructable>())
                if (InRange(a.pos - pos, 5))
                    a.active = false;
        }
    }
    protected override void OnCreate(bool b)
    {
        Register(this, b);
        base.OnCreate(b);
    }
    public Vector3 randomOffset;
    public void OnPlayerStay(Player pl, Trigger other)
    {
        
        if (playerHostage)
        {
            var deadPlayer = pl.team2.GetDeadPlayer();
            if (deadPlayer && pl.observing && pl.Input2.GetKeyDownTime(KeyCode.F, 2, Android ? "Rescue" : "Hold {0} to rescue" + deadPlayer.name))
            {
                CallRPC(SetActive, false);
                deadPlayer.RPCReset(true, new PosRot(pos));
            }
        }
        else if ((!follow || follow.deadOrKnocked) && !pl.deadOrKnocked)
        {
            if (pl.team == TeamEnum.CounterTerrorists && pl.Input2.GetKeyDownTime(KeyCode.F, 5, Android ? "Rescue" : "Hold {0} to rescue"))
                CallRPC(SetPlayerFollow, pl.viewId);
        }
    }

    public Player follow;
    [PunRPCBuffered]
    public void SetPlayerFollow(int id)
    {
        follow = ToObject<Player>(id);
        follow.PlayThank();
        toggleChair(false);
    }


    [PunRPCBuffered]
    public void Init(TeamEnum plTeam)
    {
        team = plTeam;
        ((Team) team).hostage = this;
    }
    public override void OnPlConnected(PhotonPlayer photonPlayer)
    {
        base.OnPlConnected(photonPlayer);
        if (follow != null)
            CallRPC(SetPlayerFollow, follow.viewId);
        CallRPC(Init, team);
        Sync();
        // if (customServer) return;
    }
    public override void OnReset()
    {
        follow = null;
        pos = startPos;
        active = false;
        active = true;//animator rewind
        if (isCaller && customServer) Sync();
        toggleChair(true);
        base.OnReset();
    }
    private void toggleChair(bool aEnabled)
    {
        foreach (var a in GetComponentsInChildren<MeshRenderer>(true))
            a.enabled = aEnabled;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
    public void Update()
    {
//        if (IsMine && !_Game.playersAll.Any(a => a.alive && InRange(a.pos - pos, 200)))
//            Destroy();
        if (!active)
        {
            return;
        }

        if (follow != null && !follow.deadOrKnocked)
        {
            var target = follow.pos - follow.forward * .6f + follow.skin.rotation * randomOffset;

            var newPos = Vector3.MoveTowards(Vector3.Lerp(pos, target, 3 * Time.deltaTime), target, 1 * Time.deltaTime);
            
            var fv = newPos - pos;
            pos = newPos;
            animator.SetFloat("veloticy", fv.magnitude / Time.deltaTime);
            if (fv!=Vector3.zero)
                forward = fv;
            if (follow.IsMine && (_Player.spawnPos - pos).magnitude < 20)
                CallRPC(ResqueHostage);
        }
        else
            animator.SetFloat("veloticy", 0);

        if (playerHostage && !audio.isPlaying && bs.TimeElapsedRandom(5))
        {
            audio.volume = volumeFactor;
            audio.clip = clips.Where(a => a != lastPlayed).Random();
            audio.Play();
            lastPlayed = audio.clip;
        }
    }
    [PunRPC]
    private void ResqueHostage()
    {
        active = false;
        _Loader.PlayOneShot(hostageResued);
        follow.IncreaseScore(follow.gameScore.hostageRescue);
    }
    public override void ParseBsp()
    {
        base.ParseBsp();
    }
#endif
}