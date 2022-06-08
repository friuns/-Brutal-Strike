using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace BattleRoyale
{
public partial class CarBase : Vehicle, IOnPlayerStay, ISetLife, IOnPlayerEnter
{
    // public new Transform tr;
    public GameObject nitroFx;
    public int price;
    public WeaponSetId setId = WeaponSetId.a1;
    public SubsetListTeamEnum team = new SubsetListTeamEnum(Enum<TeamEnum>.values);
    public Transform[] seats;
    public Transform exit;
    public AnimationClip seatingClip;

    public float nitroDef = 1;
    public float lerpMove = 3;
    public float lerpRot = 3;
    public float armor = 0;
    public float nitroForce = 10;

#if game
    public override void Reset()
    {
        exit = tr;
        seats = new[] { tr };
        base.Reset();
    }
    public override void Load(BinaryReader br)
    {
        base.Load(br);
    }
    public override void Save(BinaryWriter bw)
    {
        base.Save(bw);
    }
    [FieldAtr] public float timeToRespawn = 5;
    [ContextMenu("InitRespawn")]
    public void InitRespawn()
    {
        if (timeToRespawn > 0 && IsMine)
            DelayCall(timeToRespawn, () => { CallRPC(OnReset); });
    }

    public override void OnLoadAsset()
    {
        base.OnLoadAsset();
        _Game.carDict[itemName] = this;
    }

    private void RPCTransferOwnership(int id)
    {
        if (ownerID != id)
            CallRPC(OnOwnerChanged, id);
    }
    public PhotonView[] photonViews;

    public virtual void OnOwnerChanged(int id)
    {
        foreach (var pv in photonViews)
        {
            pv.OwnerShipWasTransfered = true;
            pv.ownerId = id;
            pv.RefreshOwner();
        }
    }

    internal new Rigidbody rigidbody;
    public override void Awake()
    {
        base.Awake();
        tr = transform;
        rigidbody = base.rigidbody;
        photonView.synchronization = ViewSynchronization.Unreliable;
        photonView.ObservedComponents = new List<Component>() { this };

        ResetLife();
        photonViews = GetComponentsInChildren<PhotonView>(true);

        Physics.autoSimulation = true;
    }

    public override void Start()
    {
        base.Start();
        _Loader.loaderPrefs.optimizePhysics = false;
        if (initPos == default)
            initPos = new PosRot(tr);
    }
    
    public override void OnInstanciate(ItemBase prefab)
    {
        base.OnInstanciate(prefab);
        initPos = new PosRot(tr);
    }
    public override void OnStopDrag()
    {
        base.OnStopDrag();
        initPos = new PosRot(tr);
    }
    private PosRot initPos;
    public static float mouseMoveTime;
    public override void UpdatePlayerInput(Player pl)
    {
        base.UpdatePlayerInput(pl);
        var seat = seats.GetClamped(pls.IndexOf(pl));
        pl.SetPosition(seat.position);
        if (pl == plOwner)
        {
            (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse1, false);
            (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse3, false);
            (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse4, false);

            if (plOwner.observing && !_ObsCamera.freeCam)
            {
                if (Time.time - mouseMoveTime > 1)
                    _ObsCamera.mouse = EulerToMouse(Quaternion.Lerp(Quaternion.Euler(MouseToEuler(_ObsCamera.mouse)), Quaternion.Euler(seat.eulerAngles), Time.deltaTime * 10).eulerAngles);
            }
        }
        pl.skin.rot = seat.rotation;

        if (pl.IsMine && !pl.dead && pl.vehicle == this)
            if (!pl.bot && pl.Input2.GetKeyDown(KeyCode.F) && enterTime != Time.time)
                pl.RPCSetVehicle(null);
    }

    private float pingSmooth;
    private Vector3 syncPos;
    private Quaternion syncRot = Quaternion.identity;
    private Vector3 syncVel;
    private Vector3 syncAng;
    private Vector3 offsetPos;
    private Quaternion offsetRot = Quaternion.identity;
    private float maxLerp;
    protected void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //stream.isWriting
        if (!rigidbody) return;
        if (stream.isWriting)
        {
            syncPos = rigidbody.position;
            syncRot = rigidbody.rotation;
            syncVel = rigidbody.velocity;
            syncAng = rigidbody.angularVelocity;
        }
        stream.Serialize(ref syncPos);
        stream.Serialize(ref syncRot);
        stream.Serialize(ref syncVel);
        stream.Serialize(ref syncAng);
        var ping = (float)(PhotonNetwork.time - info.timestamp);
        pingSmooth = Mathf.Lerp(pingSmooth, Mathf.Max(0, ping), .1f);
        if (stream.isReading)
        {
            if (Vector3.Distance(rigidbody.position, syncPos) > 10 && Time.time - collisionTime > 1)
            {
                tr.position = syncPos;
                tr.rotation = syncRot;
            }

            syncPos += syncVel * (Mathf.Clamp(pingSmooth, 0, .2f) + (rigidbody.isKinematic ? Time.deltaTime : 0));

            Debug.DrawLine(pos, syncPos, Color.yellow, 2);
            if (!rigidbody.isKinematic)
            {
                rigidbody.velocity = syncVel;
                rigidbody.angularVelocity = syncAng;
            }

            offsetPos = syncPos - pos;
            if (Mathf.Abs(offsetPos.y) < .1f) offsetPos.y = 0;
            offsetRot = syncRot * Quaternion.Inverse(rot);
        }
    }
    public MaxValue minVel = new MaxValue();
    public virtual void FixedUpdate()
    {
        // if (!plOwner)
            // return;
        if (plOwner&& InputGetKey(KeyCode.LeftShift))
            rigidbody.AddForce(tr.forward * rigidbody.mass);
        minVel.minValue = rigidbody.velocity.sqrMagnitude;


        if (!IsMine && Time.time - collisionTime > .3f)
        {
            maxLerp = Mathf.Clamp((syncPos - pos).magnitude / 5, 1, 3);
            var mt = Vector3.Lerp(Vector3.zero, offsetPos, Time.deltaTime * lerpMove * maxLerp);
            offsetPos -= mt;
            rigidbody.MovePosition(pos + mt);

            var rt = Quaternion.Slerp(Quaternion.identity, offsetRot, Time.deltaTime * lerpRot * maxLerp);

            offsetRot *= Quaternion.Inverse(rt);
            rigidbody.MoveRotation(rt * rot);
        }
    }

    internal float collisionTime;
    public void OnCollisionEnter(Collision collision)
    {
        var other = collision.collider.GetComponentInParent<Car>();
        if (other)
            collisionTime = other.collisionTime = Time.time;
    }
    [ContextMenu("OnReset")]
    [PunRPC]
    public override void OnReset()
    {
        base.OnReset();
        initPos?.ApplyToTransform(tr);
        photonView.RefreshOwner();
        ResetLife();
        smokeEffect.SetActive3(false);
        explosionEffect.SetActive3(false);
    }
    public virtual void ResetLife()
    {
        life = defLife;
    }

    public Input2Base Input2 => plOwner.Input2;
    public bool InputGetKey(KeyCode keyCode)
    {
        return plOwner.InputGetKey(keyCode);
    }


    private float enterTime;
    public override void OnEnter(Player pl, bool enter)
    {
        base.OnEnter(pl, enter);
        pl.SetPlatform(false);
        enterTime = Time.time;
        if (!enter)
        {
            if (!Physics.Linecast(pl.pos + Vector3.up, exit.position + Vector3.up, out RaycastHit h, Layer.levelMask))
                pl.SetPosition(exit.position);
        }
        pl.controller.noclip = enter;
        if (enter)
            pl.gesture = AnimationPlayableUtilities.PlayClip(pl.animator, seatingClip, out _);
        else
        {
            if (pl.gesture.IsValid())
                pl.gesture.Destroy();
        }

        if (plOwner?.IsMine == true)
            RPCTransferOwnership(plOwner.ownerID);
    }
    public void OnPlayerStay(Player pl, Trigger other)
    {
        if (!dead && pl.IsMine && pls.Count < seats.Length && enterTime != Time.time && !pl.vehicle && pl.Input2.GetKeyDown(KeyCode.F, "F Enter Vehicle", "Enter Vehicle"))
        {
            pl.RPCSetVehicle(this);
        }
    }


#endif
}
}