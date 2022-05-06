#define Final
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// namespace doru
// {
public class Door : Destructable, IOnPlayerStay
{
    public new string target = "";
    public ItemBase buttonTarget  ;
    // public bool trigger;
    // public bool trigger;
    public Vector3 move;
    public Vector3 rotate = new Vector3(0, 90, 0);
    public bool teleport;
    public bool twoSideOpen = true;
    public bool physics = true;
    public int damage = 0;
    public Vector3 jumpPadVel = Vector3.zero;

    private bool opened;
#if game
    public Door targetDoor=>buttonTarget as Door;

    private bool isButton { get { return targetDoor; } }
    bool inverseOpen;
    //public bool inverse;
    public Door controlledBy;
    public Vector3 initialPos;
    public Quaternion initialRot;
    public new Rigidbody rigidbody;
    
    [ContextMenu("Reset2")]
    public override void Reset()
    {
        foreach (var a in GetComponentsInChildren<Transform>())
            a.gameObject.layer = Layer.trigger;
        
        animation = GetComponentInChildren<Animation>();
        if (animation && !animation.clip)
        {
            physics = false;
            animation.playAutomatically = false;
            var clip = animation.clip = new AnimationClip() {name = "Door Animation", legacy = true };
            animation.AddClip(clip,clip.name);
        }
        base.Reset();
    }
    public override void Awake()
    {
        base.Awake();
        InitTriggers(); //need to be executed before transformCache
        // gameObject.layer = Layer.door;
    }
    public Vector3 anchor;
    public new Animation animation;
    
    public override void Start()
    {
        base.Start();
        if (!gameLoaded) return;
        if (twoSideOpen && rotate != Vector3.zero)
            AutoSetRotation();

        if (animation)
            physics = false;
        
        if (physics)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            var j = gameObject.AddComponent<HingeJoint>();
            j.axis = Vector3.up;
            j.anchor = anchor;
            j.limits = new JointLimits() {max = 120, min = -120};
            j.useLimits = true;
            var mf = GetComponentInChildren<MeshCollider>();
            if (mf)
                mf.convex = true;
        }

        initialPos = localPosition;
        initialRot = localRotation;

        //OnCreate called in Start also, and _Game.doors not filled
        DelayCall(delegate
        {
            if (!targetDoor && !string.IsNullOrEmpty(target))
                buttonTarget = _Game.baseItems.FirstOrDefault(a => (string.IsNullOrEmpty(a.itemName) ? a.name : a.itemName) == target);
            if (targetDoor)
                targetDoor.controlledBy = this;
        });

    }
    protected override void OnCreate(bool b)
    {
        Register(this, b);
        base.OnCreate(b);
    }



    [PunRPC]
    
    public void OpenDoor(bool open, bool inverseOpen)
    {
        this.inverseOpen = inverseOpen;
        prints("OpenDoor:", open);
        if (open != opened)
        {
            var a = GetComponentInChildren<AudioSource>();
            if (a != null)
            {
                a.volume = volumeFactor;
                a.Play();
            }
            else
            {
                AudioClip2 clip2 = gameRes.openDoorSound;
                PlayClipAtPoint(clip2, pos, 1);
            }


            if (physics)
            {
                rigidbody.isKinematic = !open;
                if (!open)
                    StartCoroutine(TranslateTo(transform, initialRot, initialPos, 1));
            }
            else if (animation)
            {
               
                if (!animation.isPlaying)
                {
                    animation[animation.clip.name].normalizedTime = !open ? 1 : 0;
                    animation[animation.clip.name].speed = !open ? -1 : 1;
                    animation.Play();
                }
            }
            else
                StartCoroutine(AddRotateAdditional(transform, Math2.XAnd(!inverseOpen, open) ? -rotate : rotate, open ? move : -move, 1));

            opened = open;
        }
    }

    public override void Save(BinaryWriter bw)
    {
        bw.Write(target);
        bw.Write(itemName);
        bw.Write(move);
        bw.Write(rotate);
        base.Save(bw);
    }
    public override void Load(BinaryReader br)
    {
        target = br.ReadString();
        itemName = br.ReadString();
        move = br.ReadVector3();
        rotate = br.ReadVector3();
        base.Load(br);
    }
    public override void OnPlConnected(PhotonPlayer photonPlayer)
    {
        base.OnPlConnected(photonPlayer);
        if (opened)
            CallRPC(OpenDoor, opened, inverseOpen);
    }


    [PunRPC]
    public override void OnReset()
    {
        base.OnReset();
        if (animation)
        {
            opened = false;
            // animation.Rewind();
            // animation[animation.clip.name].time = 0;
            animation.clip.SampleAnimation(animation.gameObject,0);
            // animation.Sample();
        }
        else
            OpenDoor(false, inverseOpen);
    }
    // public void OnControllerColliderHit2(Player pl)
    // {
        // OnPlayerStay(pl, null);
    // }
    // private void OnTriggerStay(Collider other)
    // {
    //     if (other is CharacterController == false) return;
    //     var pl = other.gameObject.GetComponentInParent<Player>();
    //     OnPlayerStay(pl, null);
    // }
    public void OnPlayerStay(Player pl, Trigger other)
    {
        // if (collisionTrigger && other != null) return;
        
        // if (!pl.IsMine || controlledBy) return;

        if (teleport)
        {
            pl.SetPosition(buttonTarget.pos + Vector3.up);
        }
        else
        {
            if (damage > 0 && TimeElapsed(1) && pl.IsMine)
                pl.RPCDamageAddLife(-damage);
            if (jumpPadVel != Vector3.zero)
            {
                pl.veloticy = rot * jumpPadVel;
            }

            Door door = isButton ? targetDoor : this;
            if (!door)
                printMissing(Concat(target, " not found"));
            else if (Android||oculus || pl.bot ? !door.opened : pl.Input2.GetKeyDown(KeyCode.F, "Press {0} To Open Door", "Door"))
            {
                door.RPCOpenDoor();
                if (door.physics)
                    door.rigidbody.AddExplosionForce(100, pl.pos, 10);
            }
        }
    }

    [ContextMenu("OpenDoor")]
    private void RPCOpenDoor()
    {
        CallRPC(OpenDoor, !opened, opened ? inverseOpen : twoSideOpen && /*Math2.XAnd(inverse, */transform.GetForward(mainCameraForward) < 0);
    }

    [ContextMenu("Parse Bsp")]
    public override void ParseBsp()
    {
        base.ParseBsp();
        Door door = this;
        door.rotate = Vector3.zero;
        door.twoSideOpen = false;
        door.itemName = vdf["targetname"] ?? "";
        door.target = vdf["target"] ?? "";
        door.teleport = vdf.className == "trigger_teleport";
        door.physics = false;

        if (vdf.func_Door) //lift
        {
            float lift = Mathf.Abs((Quaternion.Euler(vdf.angles) * bspModel.bounds.size).x);
            lift -= vdf.PropertyInteger("lip") * bsp.BspGenerateMapVis.scale;
            door.move = Quaternion.Euler(vdf.angles) * Vector3.right * lift;
        }
        else if (vdf.func_Door_Rotating)
        {
            door.rotate = new Vector3(0, vdf.PropertyInteger("distance"), 0);
            door.physics = true;
        }
        life = lifeDef = 100;
        door.InitTriggers();

    }
    [ContextMenu("SetupRotation")]
    public void AutoSetRotation()
    {
        var r = GetComponentInChildren<Renderer>();
        if (r)
        {
            var inverse = transform.GetForward(ZeroY(r.bounds.center - transform.position)) < 0;
            rotate = inverse ? -rotate : rotate; //new Vector3(0, inverse ? -90 : 90, 0);
        }
    }
#endif
}


// }