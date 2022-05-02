using System.Collections.Generic;
using EVP;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace BattleRoyale
{
[RequireComponent(typeof(PhotonView))]
public partial class Car : Vehicle,IOnPlayerStay,ISetLife,IOnPlayerEnter
{
    public int price;
    public WeaponSetId setId = WeaponSetId.a1;
    public VehicleController vehicleController;
    public Transform[] seats;
    public Transform exit;
    public AnimationClip seatingClip;
    public float nitro = 10;
    public float lerpMove = 3;
    public float lerpRot = 3;
    public GameObject explosionEffect;
    
    public float minForce = 1.0f;
    public float multiplier = 0.1f;
#if game
    
    public override void OnLoadAsset()
    {
        base.OnLoadAsset();
        _Game.carDict[itemName] = this;
    }
    public VarParse2 varParse { get { return m_varParse ?? (m_varParse = new VarParse2(Game.varManager,vehicleController, "Car/" + itemName, RoomInfo: room)); } } 
    internal VarParse2 m_varParse;
    public override void OnLevelEditorGUI()
    {
        base.OnLevelEditorGUI();
        varParse.DrawGui();
    }
    private void RPCTransferOwnership(int id)
    {
        if (ownerID != id)
            CallRPC(OnOwnerChanged, id);
    }
    public PhotonView[] photonViews;
    [PunRPC]
    [ViaServer]
    private void OnOwnerChanged(int id)
    {
        foreach (var pv in photonViews)
        {
            pv.OwnerShipWasTransfered = true;
            pv.ownerId = id;
            pv.RefreshOwner();
        }
    }

    public override void Awake()
    {
        base.Awake();
        
        vehicleController = GetComponent<VehicleController>();
        photonView.synchronization = ViewSynchronization.Unreliable;
        photonView.ObservedComponents = new List<Component>() { this };
        var m_vehicle = GetComponent<VehicleController>();
        m_vehicle.processContacts = true;
        m_vehicle.onImpact += ProcessImpact;

    }
 
    public override void Start()
    {
        base.Start();
        life = defLife;
        photonViews = GetComponentsInChildren<PhotonView>(true);
        
        
        startPos = new PosRot(transform);
        _Loader.loaderPrefs.optimizePhysics = false;
        Physics.autoSimulation = true;
    }
    private PosRot startPos;

    public override void UpdatePlayer(Player pl)
    {
        var seat = seats.GetClamped(pls.IndexOf(pl));
        pl.SetPosition(seat.position);
        pl.skin.rot = seat.rotation;

        bool left = InputGetKey(KeyCode.A) || InputGetKey(KeyCode.Mouse3);
        bool right = InputGetKey(KeyCode.D) || InputGetKey(KeyCode.Mouse4);
        var brake = InputGetKey(KeyCode.Space);
        var forward = InputGetKey(KeyCode.W) || InputGetKey(KeyCode.Mouse0);
        var back = InputGetKey(KeyCode.S) || InputGetKey(KeyCode.Mouse1);
        (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse1, false);
        (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse3, false);
        (Input2 as Input2)?.ToggleKeyOn(KeyCode.Mouse4, false);

        steerInput = Lerp(steerInput, left ? -1 : right ? 1 : 0, Time.deltaTime) + bs._MobileInput.move.x;

        forwardInput = Lerp(forwardInput, forward ? 1 : 0, Time.deltaTime * 3) + Mathf.Max(bs._MobileInput.move.y, 0);
        reverseInput = Lerp(reverseInput, back ? 1 : 0, Time.deltaTime * 3) + Mathf.Min(bs._MobileInput.move.y, 0);
        handbrakeInput = brake ? 1 : 0;
        
        UpdateInput();
        if (pl.IsMine && !pl.dead && pl.vehicle == this)
            if (pl.Input2.GetKeyDown(KeyCode.F) && enterTime != Time.time)
                pl.RPCSetVehicle(null);
            
    }
    
    private float pingSmooth;
    private Vector3 syncPos;
    private Quaternion syncRot=Quaternion.identity;
    private Vector3 syncVel;
    private Vector3 syncAng;
    private Vector3 offsetPos;
    private Quaternion  offsetRot=Quaternion.identity;
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
                transform.position = syncPos;
                transform.rotation = syncRot;
            }

            syncPos += syncVel * Mathf.Clamp(pingSmooth, 0, .2f);
            Debug.DrawLine(pos, syncPos, Color.yellow, 2);
            rigidbody.velocity = syncVel;
            rigidbody.angularVelocity = syncAng;
            offsetPos = syncPos - pos;
            if (Mathf.Abs(offsetPos.y) < .1f) offsetPos.y = 0;
            offsetRot = syncRot * Quaternion.Inverse(rot);
        }

    }
    public MaxValue minVel = new MaxValue();
    public void FixedUpdate()
    {
        
        if (!ownerPl)
        {
            return;
        }
        if (InputGetKey(KeyCode.LeftShift))
            rigidbody.AddForce(transform.forward * 10 * rigidbody.mass);
        minVel.minValue = rigidbody.velocity.sqrMagnitude;


        if (!IsMine && Time.time - collisionTime > .3f)
        {
            maxLerp = Mathf.Clamp((syncPos - pos).magnitude / 5, 1, 3);
            var mt = Vector3.Lerp(Vector3.zero, offsetPos, Time.deltaTime * lerpMove * maxLerp);
            offsetPos -= mt;
            rigidbody.MovePosition(pos + mt);

            var rt = Quaternion.Slerp(Quaternion.identity, offsetRot, Time.deltaTime *lerpRot * maxLerp);
            
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

    public override void OnReset()
    {
        base.OnReset();
         startPos?.ApplyToTransform(transform);
         photonView.RefreshOwner();
         life = defLife;
         vehicleController.GetComponent<VehicleDamage>().RepairImmediate();
    }

    private Input2Base Input2 => ownerPl.Input2;
    public bool InputGetKey(KeyCode keyCode)
    {
        return ownerPl.InputGetKey(keyCode);
    }
    
    private float Lerp(float SteerValue, int i, float factor)
    {
        var moveTowards = Mathf.MoveTowards(SteerValue, i, factor);
        var clamp = Mathf.Clamp(moveTowards, i < 0 ? -1 : 0, i > 0 ? 1 : 0);
        return Mathf.Lerp(moveTowards, clamp, Time.deltaTime * 20);
    }
    private float handbrakeInput;
    private float forwardInput;
    private float reverseInput;
    private float steerInput;
    
    public void UpdateInput()
    {

        vehicleController.steerInput = steerInput;
        vehicleController.throttleInput = forwardInput;

         float throttleInput = 0.0f;
        float brakeInput = 0.0f;


        float minSpeed = 0.1f;
        float minInput = 0.1f;

        if (vehicleController.speed > minSpeed)
        {
            throttleInput = forwardInput;
            brakeInput = reverseInput;
        }
        else
        {
            if (reverseInput > minInput)
            {
                throttleInput = -reverseInput;
                brakeInput = 0.0f;
            }
            else if (forwardInput > minInput)
            {
                if (vehicleController.speed < -minSpeed)
                {
                    throttleInput = 0.0f;
                    brakeInput = forwardInput;
                }
                else
                {
                    throttleInput = forwardInput;
                    brakeInput = 0;
                }
            }
        }

        throttleInput *= 1.0f - handbrakeInput;
        vehicleController.steerInput = steerInput;
        vehicleController.throttleInput = throttleInput;
        vehicleController.brakeInput = brakeInput;
        vehicleController.handbrakeInput = handbrakeInput;
    }
    private float enterTime;
    public override void OnEnter(Player pl, bool enter)
    {
        base.OnEnter(pl, enter);
        pl.onPlatform = false;
        pl.node.SetParent(null);
        enterTime = Time.time;
        if (!enter)
            pl.SetPosition(exit.position);
        pl.controller.noclip  = enter;
        if (enter)
            pl.gesture = AnimationPlayableUtilities.PlayClip(pl.animator, seatingClip, out _);
        else
        {
            steerInput = forwardInput = reverseInput = handbrakeInput = 0;
            UpdateInput();
            if (pl.gesture.IsValid())
                pl.gesture.Destroy();
        }

        if (ownerPl?.IsMine==true)
            RPCTransferOwnership(ownerPl.ownerID);    
        
    }
    public void OnPlayerStay(Player pl, Trigger other)
    {
        if (!dead && pl.IsMine && pls.Count<seats.Length && enterTime != Time.time && !pl.vehicle && pl.Input2.GetKeyDown(KeyCode.F, "F Enter Vehicle", "Enter Vehicle"))
        {
            pl.RPCSetVehicle(this);
        }

    }
    

#endif


    
}

}


