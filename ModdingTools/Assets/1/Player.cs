













using System;
using UnityEngine;

using TimeCached = UnityEngine.Time;
public partial class PlayerClass
{
    public float TargetSpeed = 1;
}

public class Hud : bs
{
    
}
public class Game : bs
{
    
}
public partial class Player:bs
{
    public void Start()
    {
        
    }
    public PlayerStateEnum playerState;
//    public PlayerStateEnum playerStateToSet;
    public PlayerState PlayerState { get { return playerClassPrefab.GetState(playerState); } }
    
    internal Vector3 vel3;
    internal Vector3 vel;
    internal Vector3 gVel;
    public float totalWeightEvaluted2 = 1;
    public PlayerClass playerClassPrefab;
    public PlayerSkin skin;
    public Vector3 veloticy { get { return vel; } set { vel3 = value; } } //vel set not work on android
    internal float clampHorizontal => !skin.oneDirectionZombie ? .7f: 1;
    private float legHit = 1;
    
    
    public bool InputGetKey(KeyCode c)
    {
        return Input.GetKey(c);
    }
    
    public float Pw(float v, float times = 1)
    {
        return Mathf.Pow(v, TimeCached.deltaTime / (0.02f * times));
    }
    private Vector3 ClampMove(Vector3 v2)
    {
        if (!skin.oneDirectionZombie)
            v2.x *= clampHorizontal;
        return v2;
    }
    
    public void SetJump(bool v)
    {
        gVel = gVel.SetY(playerClassPrefab.jump * totalWeightEvaluted2);
    }
    public Vector3 SetMove(Vector3 Move,Quaternion rot)
    {
        var times = (TimeCached.deltaTime / 0.02f); //dont use deltatime * times combo

        Move = ClampMove(Move);
        
        if (playerClassPrefab.flying)
        {
            if (InputGetKey(KeyCode.Space)) Move += Vector3.up;
            if (InputGetKey(KeyCode.X)) Move += Vector3.down;
        }
        
        
        var pr = playerClassPrefab;

        var slowDown = legHit;
        var q3Decline = pr.q3Decline;
        q3Decline = 1f - (1f - q3Decline) ;

        var prTargetSpeed = pr.TargetSpeed  *  totalWeightEvaluted2 * slowDown;
        // if (pr.zombie)
            // prTargetSpeed *= 1.3f; //bugfix
        
        float q3Speed2 = PlayerClass.GetQ3Speed(prTargetSpeed, q3Decline);
    
        vel *= Pw(q3Decline, 2);

        float speedFactor = PlayerState.speed;
        //var f = crouch ? crouchSpeedFactor : prone ? proneSpeedFactor : !isGrounded ? .7f : running ? 1.5f : 1;

        if (!isGrounded) speedFactor = Mathf.Min(.7f, speedFactor);
//        if (aiming) f = Mathf.Min(playerClassPrefab.streffSpeed, f);


        Vector3 mv = pr.flying ? rot * Move : /* Quaternion.FromToRotation(Vector3.up, groundHitsNormal) **/ ZeroY(rot * Move);
        if(!pr.flying)
            mv = mv.SetY(Mathf.Min(0, mv.y));
        
        if (Move.sqrMagnitude > 0) //1: apply player moviement to vel
        {
            Vector3 q3Speed = times * q3Speed2 * mv * speedFactor;
            if (Vector3.Dot(vel, q3Speed) < 0)
            {
                q3Speed *= pr.q3StopSpeed;
                vel += q3Speed;
                if (Vector3.Dot(vel, q3Speed) > 0)
                    vel = Vector3.zero;
            }
            else
                vel += q3Speed;
        }

        if (Move == Vector3.zero && vel.magnitude / speedFactor < pr.q3minVel * times) vel = Vector3.zero;

        vel *= Pw(q3Decline, Move == Vector3.zero ? 2 / pr.q3StopSpeed : 2);

        vel3 *= Pw(.90f);
//        move *= Pw(.78f);

        //if (vel.magnitude > 0)
        //    vel = vel.normalized*5;

        //________________________________FixedUpdate2___________________________________________


        var gravity = Mathf.Min(0.1f, TimeCached.smoothDeltaTime) * pr.gravity * Physics.gravity;

        if (ControllerIsGrounded && gVel.y < .1f) //to fix jump stuck
            gVel = gravity * 20;
        else
            gVel += gravity;

        var mVel = Vector3.zero;
//        controller.noclip = true;
        if (climb)
            mVel = UpdateClimb(mVel);
        else
        {
//            if(!controller.noclip)                
            mVel += gVel + vel3;
            mVel += vel; //2: add vel to mvel 
//            mVel *= TimeCached.deltaTime;
        }
        
        // if (!IsMine)
            // mVel = playerClassPrefab.flying ? offsetPos : ZeroY(offsetPos);
            
//        if (ccHit!=null&& ccHit.normal.y>0&& ccHit.normal.y < roomSettings.slopeSlide && isGrounded)
//            mVel += ZeroY(ccHit.normal,-1) * roomSettings.slopeSlideForce;
        return mVel;
    }
    public Transform Cam => _ObsCamera.transform;
    protected Vector3 UpdateClimb(Vector3 moveVel)
    {
        //bot does not handled here
        moveVel += 4 * (Cam.forward.y > -.3f ? Vector3.up : -Vector3.up);
        moveVel += vel ;
        gVel = Vector3.zero;
        return moveVel;
    }
    private bool isGrounded=>controller.isGrounded;
    public bool ControllerIsGrounded { get { return controller.isGrounded; } }
    public bool IsMine = true;
    public CharacterController controller;
    private bool climb;
    public void RPCSetClimb(bool b)
    {
        climb = true;
    }
}