using System;
using EVP;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace BattleRoyale
{
[RequireComponent(typeof(PhotonView))]
public class Car : CarBase,ISkinBase
{
    public float damageMultiplier = 1f;
    public float damageMinForce = 1.0f;
#if game
    private bool nitroDown;

    
    public override void UpdatePlayerInput(Player pl)
    {
     
        if (pl == plOwner)
        {
            
            
            
            bool left = InputGetKey(KeyCode.A) || InputGetKey(KeyCode.Mouse3);
            bool right = InputGetKey(KeyCode.D) || InputGetKey(KeyCode.Mouse4);
            var brake = InputGetKey(KeyCode.Space);
            if (nitro < 0)
            {
                pl.unpressKey.Add(KeyCode.Mouse1);
                pl.unpressKey.Add(KeyCode.LeftShift);
            }

            nitroDown = InputGetKey(KeyCode.Mouse1) || InputGetKey(KeyCode.LeftShift);
            var forward = InputGetKey(KeyCode.W) || InputGetKey(KeyCode.Mouse0) || InputGetKey(KeyCode.Mouse1);
            var back = InputGetKey(KeyCode.S) || brake;
            var ownerPlMove =  plOwner.move;
            steerInput = left | right ? (Lerp(steerInput, left ? -1 : 1, Time.deltaTime)) : ownerPlMove.x;
            forwardInput = forward || back ? Lerp(forwardInput, forward ? 1 : 0, Time.deltaTime * 3) : Mathf.Max(ownerPlMove.z, 0);
            reverseInput = forward || back ? Lerp(reverseInput, back ? 1 : 0, Time.deltaTime * 3) : -Mathf.Min(ownerPlMove.z, 0);
            handbrakeInput = bs.Android ? (brake || back ? 1 : -Mathf.Min(ownerPlMove.z, 0)) : brake ? 1 : 0;
            
            UpdateVehicleController();
        }
        base.UpdatePlayerInput(pl);
    }
    public float Lerp(float SteerValue, int i, float factor)
    {
        var moveTowards = Mathf.MoveTowards(SteerValue, i, factor);
        var clamp = Mathf.Clamp(moveTowards, i < 0 ? -1 : 0, i > 0 ? 1 : 0);
        return Mathf.Lerp(moveTowards, clamp, Time.deltaTime * 20);
    }
    
    public override void ResetLife()
    {
        nitro = nitroDef;
        base.ResetLife();
    }
    public override void FixedUpdate()
    {
        if (nitroFx.SetActive3(nitroDown))
        {
            nitro -= Time.deltaTime;
            rigidbody.AddForce(transform.forward*nitroForce * rigidbody.mass);
        }
        base.FixedUpdate();
    }
    [PunRPC]
    public override void OnReset()
    {
        vehicleController.steerInput = 0;
        vehicleController.throttleInput = 0;
        vehicleDamage.RepairImmediate();
        base.OnReset();
     
    }
    public override void OnLoadAsset()
    {
        vehicleController = GetComponent<VehicleController>();
        base.OnLoadAsset();
    }
    public override void Awake()
    {
        vehicleController = GetComponent<VehicleController>();
        tireEffects = GetComponent<VehicleTireEffects>();
        vehicleDamage = GetComponent<VehicleDamage>();
        // vehicleDamage.colliders.Clear(); //breaks triggers
        VehicleAudio = GetComponent<VehicleAudio>();
        vehicleController.processContacts = true;
        vehicleController.onImpact += ProcessImpact;
        
        base.Awake();

    }
    public override void Start()
    {
        base.Start();
        varParse.UpdateValues();
    }
    public VarParse2 varParse { get { return m_varParse ?? (m_varParse = new VarParse2(Game.varManager,vehicleController, "Car/" + itemName, RoomInfo: room)); } } 
    internal VarParse2 m_varParse;
    public override void OnLevelEditorGUI()
    {
        base.OnLevelEditorGUI();
        varParse.DrawGui();
    }
    
    public VehicleController vehicleController;
    public VehicleTireEffects tireEffects;
    public VehicleDamage vehicleDamage;
    public VehicleAudio VehicleAudio;
    internal float lodTime;
    public override void OnEnter(Player pl, bool enter)
    {
  

        if (!enter && pl == plOwner)
        {
            vehicleController.steerInput = 0;
            vehicleController.throttleInput = 0;
        }
        base.OnEnter(pl, enter);
        
        if (enter && pl.IsMine)
            this.UpdateTextures();
    }
    public void Update()
    {
        var mag = (pos - _ObsPlayer.pos).magnitude;
        if (plOwner == _Player || mag < (qualityLevelAndroid > QualityLevel.Low ? 30 : 10))
            lodTime = Time.time;

        // if (lodTime > 150 && life < defLife && IsMine && !plOwner)
            // CallRPC(OnReset);
        
        var enablePhysics = Time.time - lodTime < 3 || rigidbody.velocity.sqrMagnitude > 1;
        if (vehicleController.enabled != enablePhysics)
        {
            vehicleController.wheels.ForEach(a => a.wheelCollider.enabled = enablePhysics);
            vehicleController.cachedRigidbody.isKinematic = !enablePhysics;
        }
        // if (enablePhysics && !plOwner)
        // {
        //     vehicleController.steerInput = 0;
        //     vehicleController.throttleInput = 0;
        //     // vehicleController.brakeInput = 0;
        //     // vehicleController.handbrakeInput = 0;
        // }
        VehicleAudio.enabled = enablePhysics;
        vehicleDamage.enabled = enablePhysics;
        vehicleController.enabled = enablePhysics;
        tireEffects.enabled = enablePhysics && qualityLevelAndroid > QualityLevel.Low;
        
        if (IsMine && life>0)
        {
            RaycastHit h = default(RaycastHit);
            if (Vector3.Dot(transform.up, Vector3.up) > .1f || !Physics.Raycast(pos+Vector3.up, Vector3.down, out h, 3, Layer.levelMask))
                upsideDown = Time.time;
            if (Time.time - upsideDown > 3)
            {
                var f = transform.forward;
                transform.up = h.normal;
                transform.forward = f;
                rigidbody.angularVelocity = Vector3.zero;
                upsideDown = Time.time;
            }

        }
    }
    private float upsideDown;
    private float handbrakeInput;
    private float forwardInput;
    private float reverseInput;
    private float steerInput;
    
    [PunRPC]
    [ViaServer]
    public override void OnOwnerChanged(int id)
    {
        base.OnOwnerChanged(id);
    }
    public void UpdateVehicleController()
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
        if (vehicleController.speed <= minSpeed && bs.Android)
            (throttleInput, handbrakeInput) = (-handbrakeInput + throttleInput, 0);
        throttleInput *= 1.0f - handbrakeInput;
        vehicleController.steerInput = steerInput;
        vehicleController.throttleInput = throttleInput;
        vehicleController.brakeInput = brakeInput;
        vehicleController.handbrakeInput = handbrakeInput;
    }
    
    private void ProcessImpact()
    {
        Vector3 contactForce = Vector3.zero;
        if (vehicleController.sumImpactVelocity.sqrMagnitude > damageMinForce * damageMinForce)
            contactForce = tr.TransformDirection(vehicleController.sumImpactVelocity) * damageMultiplier * 0.2f;
        else if (vehicleController.localDragVelocity.sqrMagnitude > damageMinForce * damageMinForce)
            contactForce = tr.TransformDirection(vehicleController.localDragVelocity) * damageMultiplier * 0.01f;
        if (contactForce.sqrMagnitude > 0.0f)
        {
            Vector3 contactPoint = tr.TransformPoint(vehicleController.sumImpactPosition);

            // contactForce = contactForce * 10 / Mathf.Max(3, rigidbody.velocity.magnitude);
            
            var damage = contactForce.sqrMagnitude;
            var enemyCar = vehicleController.car.Value;
            Player enemyPl = enemyCar?.plOwner;
            if (enemyPl != null)
            {
                if (plOwner?.IsEnemyOrBot(enemyPl) == false)
                    return;

                damage *= Mathf.Min(4, (enemyCar.minVel.minValue + 1) / (minVel.minValue + 1))*5;
            }


            if (/*enemyPl?.IsMine == true ||*/ plOwner?.IsMine == true)
            {
                
                RPCDamageAddLife(-damage, enemyPl);
                _Game.EmitParticles(contactPoint, -contactForce, _Game.res.spark, maxParticlesPerSecond: 10);
            }

        }
        
    }
     public GunBase gunBase { get; set; }
    public Player pl
    {
        get
        {
            return plOwner;
        }
    }
    public Action resetTextures { get; set; }
    public Bundle skinBundle { get; set; }
#endif
   
}
}


