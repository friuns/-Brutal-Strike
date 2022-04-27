using System.Collections.Generic;
using EVP;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace BattleRoyale
{
public class Car : Vehicle,IOnPlayerStay
{
    
    public VehicleController vehicleController;
    public Transform[] seats;
    public Transform exit;
    public AnimationClip seatingClip;
    public float nitro = 10;
    public float lerpMove = 3;
    public float lerpRot = 3;
    
#if game
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
    

    public override void Start()
    {
        base.Start();
        photonViews = GetComponentsInChildren<PhotonView>(true);
        vehicleController = GetComponent<VehicleController>();
        photonView.synchronization = ViewSynchronization.Unreliable;
        // photonView.ObservedComponents = new List<Component>() { this };
        startPos = new PosRot(transform);
        
    }
    private PosRot startPos;

    public override void UpdatePlayer(Player pl)
    {
        var seat = seats.GetClamped(pls.IndexOf(pl));
        pl.SetPosition(seat.position);
        pl.skin.rot = seat.rotation;
        
        UpdateInput(vehicleController);
        if (pl.IsMine && !pl.dead && pl.vehicle == this)
        {
            if (pl.Input2.GetKeyDown(KeyCode.F) && enterTime != Time.time)
            {
                pl.RPCSetVehicle(null);
            }        
        }
            
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

    public void FixedUpdate()
    {
        if(!ownerPl)return;
        if (InputGetKey(KeyCode.LeftShift))
            rigidbody.AddForce(transform.forward * 10 * rigidbody.mass);
        //if (!new[] { m_Car.WheelFL, m_Car.WheelFR, m_Car.WheelRR, m_Car.WheelRL }.All(a => a.m_grounded))
        //rigidbody.AddForce(Vector3.down * setting.extraGravity, ForceMode.Acceleration);

        if (!IsMine && Time.time - collisionTime > .3f)
        {
            maxLerp = Mathf.Clamp((syncPos - pos).magnitude / 5, 1, 3);
            var mt = Vector3.MoveTowards(Vector3.zero, offsetPos, Time.deltaTime * lerpMove * maxLerp);
            offsetPos -= mt;
            rigidbody.MovePosition(pos + mt);

            var rt = Quaternion.Slerp(Quaternion.identity, offsetRot, Time.deltaTime *lerpRot * maxLerp);
            //offsetRot = Quaternion.Lerp(offsetRot, Quaternion.identity, 1 - Time.deltaTime * 10);
            offsetRot *= Quaternion.Inverse(rt);
            rigidbody.MoveRotation(rt * rot);
        }


    }
    
    internal float collisionTime;
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<Car>())
            collisionTime = Time.time;
    }

    public override void OnReset()
    {
        base.OnReset();
         startPos.ApplyToTransform(transform);
         photonView.RefreshOwner();
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
    
    public void UpdateInput(VehicleController target)
    {

        bool left = InputGetKey(KeyCode.A);
        bool right = InputGetKey(KeyCode.D);
        var brake = InputGetKey(KeyCode.Space);
        var forward = InputGetKey(KeyCode.W) ;
        var back = InputGetKey(KeyCode.S);

        steerInput = Lerp(steerInput, left ? -1 : right ? 1 : 0, Time.deltaTime);
        forwardInput = Lerp(forwardInput, forward ? 1 : 0, Time.deltaTime * 3);
        reverseInput = Lerp(reverseInput, back ? 1 : 0, Time.deltaTime * 3);
        handbrakeInput = brake ? 1 : 0;
        
      
        target.steerInput = steerInput;
        target.throttleInput = forwardInput;
        
        
        
        
         float throttleInput = 0.0f;
        float brakeInput = 0.0f;


        float minSpeed = 0.1f;
        float minInput = 0.1f;

        if (target.speed > minSpeed)
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
                if (target.speed < -minSpeed)
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


        // Override throttle if specified


        throttleInput *= 1.0f - handbrakeInput;

        // Apply input to vehicle

        target.steerInput = steerInput;
        target.throttleInput = throttleInput;
        target.brakeInput = brakeInput;
        target.handbrakeInput = handbrakeInput;
        
        
        
    }
    private float enterTime;
    AnimationClipPlayable clip;
    public override void OnEnter(Player pl, bool enter)
    {
        base.OnEnter(pl, enter);
        
        enterTime = Time.time;
        if (!enter)
            pl.SetPosition(exit.position);
        pl.controller.noclip  = enter;
        if (enter)
            clip = AnimationPlayableUtilities.PlayClip(pl.animator, seatingClip, out _);
        else
            clip.Destroy();

        if (ownerPl.IsMine)
            RPCTransferOwnership(ownerPl.ownerID);    
        
    }
    public void OnPlayerStay(Player pl, Trigger other)
    {
        if (pl.IsMine && pls.Count<seats.Length && enterTime != Time.time && !pl.vehicle && pl.Input2.GetKeyDown(KeyCode.F, "F Enter Vehicle", "Enter Vehicle"))
        {
            pl.RPCSetVehicle(this);
            
        }

    }
#endif
}

}


