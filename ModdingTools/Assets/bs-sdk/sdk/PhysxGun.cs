using System.Collections.Generic;
using System.Linq;
#if game
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

public class PhysxGun : GunBase
{
    public GameObject cubePrefab;
    public AudioClip2 metalHit;
    public AudioClip2 shoot;
    public AudioClip2 start;
    internal  float lastPlayTime;
    public float randomScale;
    public int maxItems =5;
    public float spawnInterval = .1f;
    public float shootForce = 100;
    // public List<GameObject> addMeshCollider = new List<GameObject>();
    public AudioClip gravityGun;
    [ContextMenu("Init PhysGun")]
    public void InitPhysGun()
    {

        foreach (Transform a in cubePrefab.transform.GetTransforms())
        {
            a.transform.localScale = Vector3.one / Mathf.Sqrt(a.GetComponent<Renderer>().bounds.size.magnitude);
            var physxGunObj = a.gameObject.Component<PhysxGunObj>();
            physxGunObj.rg = a.GetComponent<Rigidbody>();
            a.gameObject.Component<Trigger>().Reset();
            trs.Add(physxGunObj);
        }
        // base.OnLoadAsset();
    }
    public  List<PhysxGunObj> trs = new List<PhysxGunObj>();
    #if game
    public static FastList2<PhysxGunObj> cubes => GetInstances<PhysxGunObj>();

    private Animation handsAnimation ;
    
    public override void OnSelectGun(bool selected)
    {
        base.OnSelectGun(selected);
        if (!handsAnimation)
            handsAnimation = Hands.GetComponentInChildren<Animation>();
        if(!selected)
            pl.weaponAudioSource.Stop();
    }
  
    private bool oldMouseButton;
    public void Update()
    {
        if (pl.deadOrKnocked) return;
        if (!active) return;
        

        var mouseButton = pl.InputGetKey(KeyCode.Mouse0) ;
        var mouseUp = oldMouseButton && !mouseButton ;
        var mouseDown = !oldMouseButton && mouseButton;
        oldMouseButton = mouseButton;

        if (mouseButton)
        {
            pl.weaponAudioSource.pitch = Mathf.Min(pl.weaponAudioSource.pitch + Time.deltaTime / 2, 3);
            // animationState.time += UnityEngine.Random.value;
            // pl.HitTime = Time.time - .1f / 3;    
        }

        if (mouseDown)
        {
            shootTime = TimeCached.time;
            
            PlayAnimation(Anims.startShoot, .1f);
            pl.PlayOneShot(start);
            if (pl.observing)
            {
                handsAnimation.Play("Start");
                pl.weaponAudioSource.loop = true;
                pl.weaponAudioSource.clip = gravityGun;
                pl.weaponAudioSource.Play();
            }
        }
        if (mouseUp)
        {
            pl.weaponAudioSource.Stop();
            pl.weaponAudioSource.pitch = 1;
            
            PlayAnimation(Anims.shoot2, .1f);
            if (TimeCached.time - shootTime > 1)
            {
                pl.PlayOneShot(shoot);
                if (pl.observing)
                {
                    handsAnimation.Play();
                    // pl.HitTime = TimeCached.time;
                    // _ObsCamera.PlayDamageAnim();

                }
            }
        }
        if (mouseButton||mouseUp)
        {
            
            if(lastCnt<maxItems && TimeElapsed(spawnInterval))
            {
                var normalized = Random.insideUnitCircle.normalized;
                normalized.y = Mathf.Max(normalized.y, 0);
                var plPos = pl.pos + pl.rot * normalized * Random.Range(2, 5)+pl.Cam.forward;
                var cube = Instantiate(trs.Random(), plPos, Random.rotation);
                cube.createTime = TimeCached.time;
                cube.gun = this;

                var component = cube.GetComponent<Trigger>();
                if (!pl.triggerNearby.triggers.Contains(component))
                {
                    component.triggers.Add(pl.triggerNearby);
                    pl.triggerNearby.triggers.Add(component);
                }

                Debug.DrawRay(pl.pos, plPos, Color.red, 10);
                if (randomScale > 0)
                    cube.transform.localScale = Random.insideUnitSphere * randomScale;
            }
            
            int cnt = 1;
     

            foreach (var a in pl.triggerNearby.triggers)
                if (a.handler is PhysxGunObj o)
                {
                    Rigidbody r = o.rg;
                    var v = pl.hpos + pl.Cam.forward * Mathf.Max(5, Mathf.Sqrt(lastCnt)) - r.position;
                    var sqrMagnitude = v.sqrMagnitude;
                    var magnitude = Mathf.Sqrt(sqrMagnitude);

                    var old = TimeCached.time - o.createTime > 3 || o.gun != this;
                    if (TimeCached.time - o.lastAttack < 2 || magnitude > 25 && old) continue;
                    o.lastTime = TimeCached.time;
                    if (magnitude < 5)
                    {
                        cnt++;
                        r.detectCollisions = !(magnitude > 5 && r.velocity.magnitude < 2);
                        o.gun = this;
                    }

                    if (mouseUp)
                    {
                        if (magnitude < 3)
                        {
                            o.gun = this;
                            o.enabled = true;
                            r.detectCollisions = true;
                            o.lastAttack = Time.time;
                            r.AddForce(pl.Cam.forward * shootForce * Mathf.Min(1, TimeCached.time - shootTime), ForceMode.Impulse);
                        }
                    }
                    else
                    {
                        r.velocity = Vector3.ClampMagnitude(r.velocity, 1 + magnitude + sqrMagnitude * 5);
                        r.AddForce(v.normalized * ((!old ? 300 : Mathf.Max(200 - (sqrMagnitude * sqrMagnitude) * .1f, 50)) * Time.deltaTime * 50));
                    }
                    // a.AddExplosionForce(explosionForce, transform.forward*10, radius);
                }
            
            lastCnt = cnt;
        }
        
        
        
    }
    private int lastCnt;
#endif
}