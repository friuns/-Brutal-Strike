using BattleRoyale;
using UnityEngine;

namespace ChobiAssets.PTM
{
public class Tank : CarBase
{
    #if game
    [PunRPC]
    [ViaServer]
    public override void OnOwnerChanged(int id)
    {
        base.OnOwnerChanged(id);
    }
    public override void Awake()
    {
        base.Awake();
        
        
        OnReset();
    }
    private void Initialize()
    {
        tankControl = copy.GetComponentInChildren<Drive_Control_CS>();
        aimControl = tankControl.GetComponent<Aiming_Control_CS>();
        cannonFire = tankControl.GetComponentInChildren<Cannon_Fire_CS>();
        copy.GetComponentsInChildren<Trigger>().ForEach(a => a.handler = this);
        rigidbody = copy.GetComponentInChildren<Rigidbody>();
        exit = tr;
        seats = new[] { tr };
    }
    
    private new GameObject prefab;
    private Drive_Control_CS tankControl;
    public Aiming_Control_CS aimControl;
    public Cannon_Fire_CS cannonFire;
    public Drive_Control_Input_02_Keyboard_Pressing_CS tankInput => (Drive_Control_Input_02_Keyboard_Pressing_CS)tankControl.inputScript;
    public override void Die()
    {
        GetComponentInChildren<Damage_Control_Center_CS>().MainBody_Destroyed();
        base.Die();
    }
    public void Update()
    {
        if (!plOwner) return;
    
        aimControl.Target_Position = Physics.Raycast(plOwner.Cam.pos+plOwner.Cam.forward*4, plOwner.Cam.forward, out RaycastHit h, 1000, Layer.allmask) ? h.point : plOwner.Cam.pos + plOwner.Cam.forward * 100;
        if (plOwner.IsMine && !cannonFire.Is_Loaded)
            plOwner.unpressKey.Add(KeyCode.Mouse0);
        
        if (plOwner.InputGetKey(KeyCode.Mouse0))
            Shoot();
        
        tankInput.vertical= plOwner.move.z;
        tankInput.horizontal = plOwner.move.x;
        

        // Control the brake.
        tankInput.controlScript.Apply_Brake = plOwner.InputGetKey(KeyCode.Space);

        // Set the "Stop_Flag", "L_Input_Rate", "R_Input_Rate" and "Turn_Brake_Rate".
        tankInput.Set_Values();
    }
    private void Shoot()
    {
        cannonFire.Fire();
    }
    public GameObject copy;
    [PunRPC()]
    public override void OnReset()
    {
        if (!prefab)
        {
            prefab = base.transform.GetChild(0).gameObject;
            prefab.SetActive(false);
        }
        DestroyImmediate(copy);
        copy = GameObject.Instantiate(prefab, base.transform, false);
        copy.SetActive(true);
        tr = copy.transform;
        Initialize();
        base.OnReset();
    }
    
}
public partial class Layer_Settings_CS
{
    const int maxLayersNum = 31;
    public static int Wheels_Layer = Layer.HLOD; // for wheels.
    public static int Reinforce_Layer = Layer.enemy; // for suspension arms and track reinforce objects. (Ignore all the collision)
    public static int Body_Layer = Layer.car; // for MainBody.
    public static int Bullet_Layer = Layer.grenade; // for bullet.
    public static int Armor_Collider_Layer = Layer.ground; // for "Armor_Collider" and "Track_Collider".
    public static int Extra_Collider_Layer = Layer.groundSoundFx; // for Extra Collier.
    public static void Start()
    {
        foreach (var a in new[] { Wheels_Layer, Reinforce_Layer, Body_Layer, Bullet_Layer, Armor_Collider_Layer, Extra_Collider_Layer })
        {
            Physics.IgnoreLayerCollision(a, Layer.player, true);
            Physics.IgnoreLayerCollision(a, Layer.ragdoll,true);
        }
    }
}

public partial class Drive_Control_CS : MonoBehaviour
{
    private void Awake()
    {
        foreach (var a in GetComponentsInChildren<Transform>())
        {
            if (a.gameObject.layer == 9)
                a.gameObject.layer = Layer_Settings_CS.Wheels_Layer;
            
            if (a.gameObject.layer == 10)
                a.gameObject.layer = Layer_Settings_CS.Reinforce_Layer;
            if (a.gameObject.layer == 0)
                a.gameObject.layer = Layer_Settings_CS.Body_Layer;
        }
        Layer_Settings_CS.Start();
        
    }
}
public partial class Bullet_Control_CS
{
      void AP_Hit_Process(GameObject hitObject, float hitVelocity, Vector3 hitNormal)
        {
            
            isLiving = false;

            // Set the collision detection mode.
            This_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (hitObject == null)
            { // The hit object had been removed from the scene.
                return;
            }

            // Get the "Damage_Control_##_##_CS" script in the hit object.
            var damageScript = hitObject.GetComponent<Damage_Control_00_Base_CS>();

            var hitAngle = Mathf.Abs(90.0f - Vector3.Angle(This_Transform.forward, hitNormal));
            var damageValue = Attack_Point * Mathf.Pow(hitVelocity / Initial_Velocity, 2.0f) * Mathf.Lerp(0.0f, 1.0f, Mathf.Sqrt(hitAngle / 90.0f)) * Attack_Multiplier;
            hitObject.GetComponentInParent<ISetLife>()?.RPCDamageAddLife(-damageValue);
            
            if (damageScript != null)
            { // The hit object has "Damage_Control_##_##_CS" script. >> It should be a breakable object.

                // Calculate the hit damage.
                


                // Output for debugging.
                if (Debug_Flag)
                {
                    float tempMultiplier = 1.0f;
                    Damage_Control_09_Armor_Collider_CS armorColliderScript = hitObject.GetComponent<Damage_Control_09_Armor_Collider_CS>();
                    if (armorColliderScript)
                    {
                        tempMultiplier = armorColliderScript.Damage_Multiplier;
                    }
                    Debug.Log("AP Damage " + damageValue * tempMultiplier + " on " + hitObject.name + " (" + (90.0f - hitAngle) + " degrees)");
                }

                // Send the damage value to "Damage_Control_##_##_CS" script.
                if (damageScript.Get_Damage(damageValue, Type) == true)
                { // The hit part has been destroyed.
                    // Remove the bullet from the scene.
                    Destroy(this.gameObject);
                }
                else
                { // The hit part has not been destroyed.
                    // Create the ricochet object.
                    if (Ricochet_Object)
                    {
                        Instantiate(Ricochet_Object, This_Transform.position, Quaternion.identity, hitObject.transform);
                    }
                }

            }
            else
            { // The hit object does not have "Damage_Control_##_##_CS" script. >> It should not be a breakable object.
                // Create the impact object.
                if (Impact_Object)
                {
                    Instantiate(Impact_Object, This_Transform.position, Quaternion.identity);
                }
            }
        }

#endif
}
}
