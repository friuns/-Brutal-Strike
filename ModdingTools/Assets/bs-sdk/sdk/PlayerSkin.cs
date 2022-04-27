using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using cakeslice;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using EnumsNET;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
#pragma warning disable 618

// ReSharper disable RedundantCast.0

public class PlayerSkin : ObjectBase,ISkinBase, IOnLoadAsset, IDontDisable
{
    // public ForwardEvents forwardEvents;
    public Base glasses;
    public Sprite icon;
    public List<Sprite> icons = new List<Sprite>();
    public Quaternion cameraPreviewRotation;
    public Vector3 cameraPreviewPosition;
    public float cameraPreviewFov;

    
    // public bool loading { get; set; }
#if UNITY_EDITOR && game 

    
    [ContextMenu("CreatePreviewWithCords")]
    public void CreatePreview2()
    {
        var cam = SceneView.lastActiveSceneView.camera;
        var camT = cam.transform;
        cameraPreviewPosition = headTransform.InverseTransformPoint(camT.position) ;
        cameraPreviewRotation = Quaternion.Inverse(headTransform.rotation) * camT.rotation;
        cameraPreviewFov = cam.fieldOfView;
        CreatePreview("characters/"+gameObject.name);
    }

    [ContextMenu("CreatePreview")]
    public Sprite CreatePreview()
    {
        return CreatePreview("characters/" + gameObject.name);
    }
    public Sprite CreatePreview(string assetName)
    {
        var rnd = Quaternion.Euler(Random.Range(-30, 30), 0, Random.Range(-10, 10));
        var head = TempTransform.instance;
        head.SetPositionAndRotation(headTransform.position,headTransform.rotation);
        head.rotation *= rnd;
        icon = CreateAssetPreviewEditor(head.TransformPoint(cameraPreviewPosition), transform, head.rotation * cameraPreviewRotation, orto: false, fov: cameraPreviewFov, assetName: assetName);
        return icon;
    }
#endif
    
    //
    // [ContextMenu("RuntimeIcon")]
    // private void RuntimeIcon()
    // {
    //     var rnd = Quaternion.Euler(Random.Range(-30, 30), 0, Random.Range(-10, 10));
    //     var head = TempTransform.instance;
    //     head.SetPositionAndRotation(headTransform.position,headTransform.rotation);
    //     head.rotation *= rnd;
    //     icon = CreateAssetPreview(head.TransformPoint(cameraPreviewPosition), transform, head.rotation * cameraPreviewRotation, orto: false, fov: cameraPreviewFov, size: 128);
    // }
    
    //public AnimatorControllerParameter[] m_AnimatorParameters;
    //public AnimatorControllerParameter[] AnimatorParameters { get { return m_AnimatorParameters ?? (m_AnimatorParameters = animator.parameters); } }
    // [HideInInspector]
    // public int[] parameterToHash;
    
    [Header("       transforms")]
    [Validate]
    [SerializeField]
    public Transform headTransform;
//    public ObscuredTransform headTransform2;
    //public Transform hips { get { return animator.GetBoneTransform(HumanBodyBones.Hips); } }
    public Transform gunPlaceHolder;
    public Transform helmetPlaceHolder;
    public Transform bodyWeapon;
    public BodyToTransform bodyToTransform = new BodyToTransform();
    public Dictionary<Transform, HumanBodyBones> transformToBody = new Dictionary<Transform, HumanBodyBones>();

    public Transform GetBodyTransform(HumanBodyBones bone)
    {
        Transform t;
        if (!bodyToTransform.TryGetValue(bone, out t))
            return null;
        return t;
    }
    [Header("       other")]


    //[FormerlySerializedAs("id")]
    [Validate]
    public int playerClassId;
    [Validate]
    public int id;
    public Collider[] colliders;
    public Rigidbody[] rigidbodies;
    public CharacterJoint [] joints;
    public new SkinnedMeshRenderer[] renderers;
    public TransformCache transformCache;
    public TransformCache transformCacheOutline;
    public SubsetListTeamEnum teams = new SubsetListTeamEnum(Enum<TeamEnum>.values);

    internal bool oneDirectionZombie;
    // [Validate]
    // public AnimationCurve animWalkFactorCurve;

    [FieldAtrStart]
    public bool disabled;
    public Vector3 cameraTargetOffset = new Vector3(0, 0.4f, -3f);

    // public bool disableArmorSkins = true;
    [FieldAtrEnd]
    public AnimationDict animations = new AnimationDict();
    // public AnimationSoundDict animationSounds = new AnimationSoundDict();
    public Vector3 thirdPersonRotationOffset;
    public Vector3 thirdPersonRotationOffsetProne;
    public bool  bot;
#if game
    public override PhotonPlayer owner { get { return pl?.owner; } }

    // public new Vector3 forward { get { return transform.forward; } set { transform.forward = value; } }
    // public new Vector3 up { get { return transform.up; } set { transform.up = value; } }

    public new Vector3 pos { get { return tr.localPosition; } set { transform.localPosition = value; } }

    public new bool enabled { get { return base.enabled; } set { base.enabled = value; } }
    public Player pl { get; set; }
    public void SetPlayer(Player newPl,bool setVisible)
    {
        seed = Random.insideUnitCircle;
        
        transformCache.visible = setVisible; //after death stay invisible
        pl = newPl;
        if (newPl)
        {
            foreach (var a in destroyedHands)
                animator.GetBoneTransform(a).localScale=Vector3.one;
            destroyedHands.Clear();
            if(glasses)
                glasses.gameObject.SetActive(newPl.bot);
        }
    }
    public VarParse2 varParseSkin { get { return m_varParseSkin ?? (m_varParseSkin = new VarParse2(Game.varManager,this, "playerSkin/" + id, RoomInfo: room)); } }
    internal VarParse2 m_varParseSkin;

    
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
//        if (_Game)
//            foreach (var a in _Game.objectItems)
//                if (a is PlayerSkin)
//                    if (GUILayout.Button(a.name))
//                        Selection.activeGameObject = a.gameObject;
        if ( animator && !animator.runtimeAnimatorController )
            LabelError("Please add animator controller");
        if ( animator && animator.GetComponents<ForwardEvents>().Length == 0)
            LabelError("Please add forward events to animator");
        
        EditorGUILayout.ObjectField("player", pl?.transform, typeof(Transform));
    }
#endif
    internal Dictionary<(int, int), Quaternion> spineCache = new Dictionary<(int, int), Quaternion>();
//    public BzRagdoll bzRagdoll;
    public void OnLoadAsset()
    {
//        Optimize();
        _Game.playerSkins[id] = this;
//        headTransform2 = headTransform.Component<ObscuredTransform>();
//        bzRagdoll = gameObject.AddComponent<BzRagdoll>();
        varParseSkin.UpdateValues();
        Game.RegisterOnGameEnabled(() => varParseSkin.roomInfo = room);
        if(userSettings.disableRagdoll)
            DisableDestroyRagdollsPhysics();


        colliders = GetComponentsInChildren<Collider>(true).Where(a => a.transform.parent != transform.transform).ToArray();
        base.renderers = renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        joints = GetComponentsInChildren<CharacterJoint>(true);
        rigidbodies = GetComponentsInChildren<Rigidbody>(true);
        
            

        
        transformCache = transform.gameObject.AddComponent<TransformCache>();
        transformCache.Populate();
        transformCacheOutline = transform.gameObject.AddComponent<TransformCache>();
        transformCacheOutline.components.Clear();
        transformCacheOutline.autoPopulate = false;

        foreach (SkinnedMeshRenderer a in renderers)
            transformCacheOutline.components.Add(a.Component<OutlineRenderer>());
        transformCache.Add(transformCacheOutline);
        // foreach(Rigidbody a in rigidbodies)
            // a.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; //fix warning for iskinematic
        ActiveColliders(false);
        // if (animationSounds.Count == 0)
            // animationSounds = _Game.playerClasses[playerClassId].animationSounds;

    }
    
    public void ActiveColliders(bool value)
    {
        // hips.gameObject.SetActive(value);
    }
    private void DisableDestroyRagdollsPhysics()
    {
        foreach (var a in GetComponentsInChildren<CharacterJoint>(true))
            DestroyImmediate(a); 
        foreach (var a in GetComponentsInChildren<Rigidbody>(true))
            DestroyImmediate(a);
    }

    public bool isRagdoll;
    
    public void SetRagdoll(bool die)
    {
        
        //if(!ragdoll && !blend)  bzRagdoll._state = BzRagdoll.RagdollState.Animated;
        if (!die)
        {
            foreach (var a in bloods)
                Destroy2(a.gameObject);
            bloods.Clear();
        }

        if(die)
            pl.modelVisible = true;
        if (isRagdoll == die) return;
        isRagdoll = die;
        
        
        if (userSettings.disableRagdoll)
        {
            if(pl)
                pl.animator.enabled = true;
            if (die)
            {
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                DelayCall(2, () =>
                {

                    if (!pl) //check if not reused
                    {
                        if (!IsVisible)
                        {
                            var tr = animator.transform;
                            var oldPos = tr.localPosition;
                            var oldRot = tr.localRotation;
                            animations[Anims.Die].SampleAnimation(animator.gameObject, 222);
                            tr.localPosition = oldPos;
                            tr.localRotation = oldRot;
                        }
                        animator.enabled = false;
                        
                    }
                });
            }
            else
            {
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
                // animator.enabled = true;
            }
        }
        else
        {
            // DelayCall(2, () =>
            // {
            //     if (!pl) //check if not reused
            //     {
            //         if (!IsVisible)
            //         {
            //             foreach (var a in rigidbodies)
            //                 a.isKinematic = true;
            //         }
            //     }
            // });
            print(t + "SetRagdoll() " + GetInstanceID() + " died ", die);

            //        bzRagdoll.SetRagdoll(ragdoll, blend);
            foreach (CharacterJoint a in joints)
            {
                a.autoConfigureConnectedAnchor = false;
                a.enableProjection = true;
            }

//        foreach (var a in colliders)
//            a.enabled = ragdoll;

            foreach (var a in rigidbodies)
            {
                a.SetKinematic(!die);
                // a.detectCollisions = die;
                a.interpolation = die ? RigidbodyInterpolation.Extrapolate : RigidbodyInterpolation.None;

            }

            animator.enabled = !die;

            // animatorOptimizer.enabled = !ragdoll;


            foreach (var a in rigidbodies)
                if (die)
                    a.velocity += pl.vel + pl.gVel.SetY(Mathf.Max(pl.gVel.y,0));
            foreach (var a in lockPositions)
                a.enabled = die;
        }
    }

    
    public List<LockPosition> lockPositions = new List<LockPosition>();
    public override void Start()
    {
        base.Start();
        if (icons.Count > 0)
        {
            var seed = (int) (pl?.seed ?? 0) % icons.Count;
            // var seed = Random.Range(0, icons.Count);
            RandomizeSkin(seed);
            icon = icons[seed];
        }
        base.renderers = renderers = GetComponentsInChildren<SkinnedMeshRenderer>(false); //skinrandomizer may destroy stuff
        foreach (var a in rigidbodies)
            a.sleepThreshold = 10;
        
        InitBlood();
    }
    private Vector2 seed;    
    private void InitBlood()
    {
        WaitForChangeLoop(() => ValueTuple.Create(pl?.BloodLevel), delegate
        {
            foreach (var a in renderers)
            foreach (var m in a.materials)
            {
                var texture2D = gameRes.bloodLevels.GetClamped((int) (pl?.BloodLevel ?? 1));
                m.SetTexture(Tag.DetailAlbedoMap, texture2D);
                m.SetTexture(Tag.Detail, texture2D);
                if (pl)
                {
                    m.SetTextureOffset(Tag.DetailAlbedoMap, seed);
                    m.SetTextureOffset(Tag.Detail, seed);
                    // m.SetTextureScale(Tag.DetailAlbedoMap, new Vector2(8, 8));
                    // m.SetTextureScale(Tag.Detail, new Vector2(8,8));
                }
                if (_ObsPlayer.BloodLevel > 1)
                    m.EnableKeyword("_DETAIL_MULX2");
            }
        }, 1);
    }
    
    #if UNITY_EDITOR
    // [ContextMenu("Generate Avatars")]
    public void GenerateSkins()
    {
        icons.Clear();
        for (int i = 0; i < 20; i++)
        {
            RandomizeSkin(i);
            // yield return new WaitForEndOfFrame();
            
            icons.Add(CreatePreview("characters/"+gameObject.name+i));
        }
        
    }
    #endif
    [ContextMenu("Randomize Skin")]
    public void RandomizeSkin()
    {
        RandomizeSkin(Random.Range(0, 999));
    }
    
    public void RandomizeSkin(int i)
    {
        var rand = new MyRandom(i);
        foreach (var a in GetComponentsInChildren<SkinRandomizer3>(true))
            a.Execute(rand);
    }
    public override void Awake()
    {
        base.Awake();
        if (animator.runtimeAnimatorController == null || animator.runtimeAnimatorController.name == gameRes.playerAnimController.name)
        {
            animator.runtimeAnimatorController = gameRes.playerAnimController;
            animations = gameRes.defAnimations;
        }
        animator.cullingMode = AnimatorCullingMode.CullCompletely;
        
        
        // parameterToHash = new int[AnimParams.dict.Count + 10];
        // foreach (AnimParams p in AnimParams.dict.Values)
        // {
        //     AnimatorControllerParameter p2 = animator.parameters.FirstOrDefault(a => a.nameHash == p);
        //     if (p2 != null)
        //         parameterToHash[(int)p] = p2.nameHash;
        // }

        
        oneDirectionZombie = !animator.parameters.Any(a => a.nameHash == AnimParams.Horizontal);
        if (ambientAudio)
            ambientAudio.volume *= volumeFactor;

        foreach (var a in Enum<HumanBodyBones>.values)
            if (a != HumanBodyBones.LastBone)
            {
                Transform boneTransform = animator.GetBoneTransform(a);
                if (boneTransform != null)
                {
                    if (!bodyToTransform.ContainsKey(a))
                        bodyToTransform[a] = boneTransform;
                    transformToBody[boneTransform] = a;
                }
            }
        canCrouch = animator.parameters.Any(a => a.nameHash == AnimParams.crouch);
        canProne = animator.parameters.Any(a => a.nameHash == AnimParams.prone);
        foreach (var a in rigidbodies.Skip(1))
            lockPositions.Add(a.gameObject.AddComponent<LockPosition>());
        
//        foreach (var a in rigidbodies)
//            a.isKinematic = true;
        isRagdoll = true; SetRagdoll(false);
        
        
    }
    
    public  Transform spine { get { return animator.GetBoneTransform(HumanBodyBones.Spine); } }
    public  Transform hips  { get { return animator.GetBoneTransform(HumanBodyBones.Hips)??transform; } }
    [ContextMenu("OptimizeTest")]
    private void Optimize()
    {
        print(animator.isOptimizable);

        var exposedTransforms = new[] {gunPlaceHolder.parent, headTransform, spine,hips};
        foreach (var a in exposedTransforms)
        {
            
        }
        AnimatorUtility.OptimizeTransformHierarchy(animator.gameObject, exposedTransforms.Select(a => a.name).ToArray());
        
    }
    
    
    internal bool canCrouch;
    internal bool canProne;

    public HumanBodyBones GetBodyPart(Transform t)
    {
        HumanBodyBones h;
        if (transformToBody.TryGetValue(t, out h))
            return h;
        return HumanBodyBones.LastBone;
    }




    #endif


    [ContextMenu("Init")]
    void Reset()
    {
        animator = null;
        if (!animator.isHuman) Debug.LogWarning("animator need to be set to humanoid", gameObject);
        else
        {
            if (!headTransform)
                headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
//            InitDamage();
            animator.Component<ForwardEvents>();
        }


        if (gunPlaceHolder)
            bodyToTransform[HumanBodyBones.RightHand] = gunPlaceHolder;



        SetDirty();
    }
    
//    [ContextMenu("InitDamage")]
//    private void InitDamage()
//    {
        
//        foreach (var b in new[] {HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg})
//        {
//            var boneTransform = animator.GetBoneTransform(b);
//            if (boneTransform)
//                foreach (var a in boneTransform.GetComponentsInChildren<Collider>())
//                    bodyDamage[a] = .6f;
//        }
//        bodyDamage[headTransform2.GetComponent<Collider>()] = 2;
//    }
    [ContextMenu("fixHeadSize")]
    public void FixHeadSize()
    {
        var c = headTransform.GetComponent<SphereCollider>();
        c.radius = .15f / c.transform.lossyScale.x;
//        c.center = c.center.normalized * .15f / c.transform.lossyScale.x;
    }
    //[ContextMenu("SetupRagdollMaster")]
    //private void BodyTotr()
    //{
    //    helmetPlaceHolder = FindTransform(Tag.helmetHolder);
    //    gunPlaceHolder = FindTransform(Tag.gunPlaceHolder);
    //    headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
    //    if (helmetPlaceHolder)
    //        bodyToTransform[HumanBodyBones.Head] = helmetPlaceHolder;
    //    if (gunPlaceHolder)
    //        bodyToTransform[HumanBodyBones.RightHand] = gunPlaceHolder;
    //    foreach (var a in bodyToTransform.ToListNonAlloc())
    //        if (!a.Value)
    //            bodyToTransform.Remove(a.Key);
    //    SetDirty();
    //}
    //public Transform FindTransform(string name)
    //{
    //    return GetComponentsInChildren<Transform>().FirstOrDefault(a => a.name == name);
    //}
#if UNITY_EDITOR

    [ContextMenu("GeneratePlaceHolders")]
    public void GenerateGunPlaceholder()
    {
        DestroyImmediate(gunPlaceHolder);
        DestroyImmediate(helmetPlaceHolder);
        bodyToTransform.Clear();

        WriteDefaultPose();


        {
            var t = new GameObject(Tag.helmetHolder).transform;
            var boneTransform = animator.GetBoneTransform(HumanBodyBones.Head);
            t.SetParent(boneTransform, false);
            t.localPosition += boneTransform.GetComponent<SphereCollider>().center;
            t.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            bodyToTransform[HumanBodyBones.Head] = helmetPlaceHolder = t;
        }

        WriteAimPose();

        {
            var t = new GameObject(Tag.gunPlaceHolder).transform;
            t.SetParent(animator.GetBoneTransform(HumanBodyBones.RightHand), false);
            t.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            bodyToTransform[HumanBodyBones.RightHand] = gunPlaceHolder = t;
        }
        
        WriteDefaultPose();
        SetDirty();
    }
    [ContextMenu("WriteAimPose")]
    private void WriteAimPose()
    {
        if(animations.Count==0)
            FillAnims();
        animations[Anims.shoot].SampleAnimation(animator.gameObject, 0);
    }
    [ContextMenu("WriteDefaultPose")]
    private void WriteDefaultPose() 
    {
        animator.GetType().GetMethod("WriteDefaultPose", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(animator, null);
    }

    [ContextMenu("fixSkinnedMeshSize")]
    public void FixSkinnedMeshSize()
    {
        var sks = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var sk in sks)
        {
            sk.updateWhenOffscreen = true;
            EditorApplication.delayCall += () =>
            {
                var b = new Bounds(sk.rootBone.InverseTransformPoint(sk.bounds.center), Vector3.zero);
                b.Encapsulate(sk.rootBone.InverseTransformPoint(sk.bounds.min));
                b.Encapsulate(sk.rootBone.InverseTransformPoint(sk.bounds.max));
                sk.localBounds = b;
                sk.updateWhenOffscreen = false;
            };
        }

    }
    
    [ContextMenu("fillAnims")]
    void FillAnims()
    {
        animator = null;
        animations.Clear();

        AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
        if (ac == null)
            ac = (animator.runtimeAnimatorController as AnimatorOverrideController).runtimeAnimatorController as AnimatorController;
        foreach (var layer in ac.layers)
            FillAnimations(layer.stateMachine);
    }
    private void FillAnimations(AnimatorStateMachine sm)
    {
        AnimatorOverrideController over = animator.runtimeAnimatorController as AnimatorOverrideController; 
        foreach (var a in sm.stateMachines)
            FillAnimations(a.stateMachine);

        foreach (ChildAnimatorState a in sm.states)
            if (a.state.motion is AnimationClip && Enum<Anims>.TryParse(a.state.name, true, out Anims ha))
            {
                animations[ha] = over ? over[(AnimationClip) a.state.motion]??(AnimationClip) a.state.motion : (AnimationClip) a.state.motion;
            }

        
    }
#endif
#if game

    private AudioSource m_Audio;
    // public List<HumanBodyBones> destroyedHands = new List<HumanBodyBones>();
    
    public AudioSource ambientAudio 
    {
        get
        {
            return m_Audio ?? (m_Audio = GetComponent<AudioSource>());
        }
    }

    public override void OnReset()
    {
        base.OnReset();

    }
    public override void OnStartGame()
    {
        
        // print("OnReset");
        if (!pl) Destroy2(gameObject);

        base.OnStartGame();
    }

    public override void OnPoolSpawn(bool b) //newer called anyway because of idontdisable
    {
    }
    
    public void Update()
    {
        
        
        if ((object) pl == null && userSettings.disableRagdoll && animator.enabled && IsVisible)
        {
            tr.rotation *= animator.deltaRotation;
            
            var ray = new Ray(tr.position + Vector3.up * 1, animator.deltaPosition);
            Debug.DrawRay(ray.origin,ray.direction.normalized,Color.red,10);
            
            if (!Physics.Raycast(ray, out RaycastHit h, 1, Layer.levelMask)) //wall check 
                tr.position += animator.deltaPosition;
            else
                tr.position += h.normal * (Time.deltaTime * 3);
            
            // if(GetFloor(tr.position,out h,2)) 
            if (Physics.Raycast(tr.position + Vector3.up * 2, Vector3.down, out h, 4, Layer.levelMask))//ground check
            {
                tr.position = tr.position.SetY(h.point.y);
                var newRot = Quaternion.FromToRotation(transform.up, h.normal) * tr.rotation;
                tr.rotation = Quaternion.RotateTowards(transform.rotation, newRot, Time.deltaTime * 100); //align to ground
            }
            else
                tr.position += Physics.gravity * (2 * Time.deltaTime);

        }
        
        
        
    }
    public List<RigVec> rigVecs = new List<RigVec>();
    private bool IsVisible => renderers[0].isVisible;
    public HumanBodyBones? GetChild(HumanBodyBones t, bool full)
    {
        if (t == HumanBodyBones.RightUpperLeg) return HumanBodyBones.RightLowerLeg;
        if (t == HumanBodyBones.RightUpperArm) return HumanBodyBones.RightLowerArm;
        if (t == HumanBodyBones. LeftUpperLeg) return HumanBodyBones. LeftLowerLeg;
        if (t == HumanBodyBones .LeftUpperArm) return HumanBodyBones. LeftLowerArm;

        if (full)
        {
            if (t == HumanBodyBones.LeftLowerArm) return HumanBodyBones.LeftHand;
            if (t == HumanBodyBones.LeftLowerLeg) return HumanBodyBones.LeftFoot;
            if (t == HumanBodyBones.RightLowerArm) return HumanBodyBones.RightHand;
            if (t == HumanBodyBones.RightLowerLeg) return HumanBodyBones.RightFoot;
        }
        
        return null;
    }

  
    internal List<HumanBodyBones> destroyedHands = new List<HumanBodyBones>();
    internal List<Transform> bloods = new List<Transform>();
    public void CreateGib(HumanBodyBones bodyPart)
    {
        if (!userSettings.gibs /*|| Random.value < .1f */|| !IsVisible|| userSettings.disableBloodAndParticles)
            return;
        using(Profile("CreateGib"))
        CreateGib(bodyPart, gibs);
    }
    BodyToTransform gibs => _Player.playerClassPrefab.gibs;
    protected override void OnCreate(bool b)
    {
        Register<PlayerSkin>(this, b);
        base.OnCreate(b);
    }
    [ContextMenu("CreateGibsExplode")]
    private void CreateGibsExplode()
    {
        CreateGibsExplode(pos);
    }
    public void CreateGibsExplode(Vector3 v)
    {
        // bool once = false;

        foreach (KeyValuePair<HumanBodyBones, Transform> a in gibs.OrderBy(a => (a.Value.position - v).magnitude).Take(2))
        {
            if (bodyToTransform.TryGetValue(a.Key, out var d))
            {
                var pos = d.position;
                _Game.EmitParticles(pos + Vector3.up * .5f, (v - pos), _Game.res.bloodParticleBig.Random());
                CreateGib(a.Key, gibs);
            }
        }
       
    }

    
    public void CreateGib(HumanBodyBones bodyPart,BodyToTransform gibs,bool pool=true)
    {
        var orig = bodyPart;
        var child = GetChild(bodyPart, false);
        
        bodyPart = child ?? orig;

        
        if (gibs.TryGetValue(bodyPart, out Transform prefab))
        {
            // if (child != null && destroyedHands.Contains(child.Value))
            //     prefab = gibs2[bodyPart];


            Transform bodyPartTr = animator.GetBoneTransform(bodyPart);
            if (bodyPartTr == null|| prefab==null) return;
            Bullet.CreateBlood(new RaycastHit() {point = bodyPartTr.position}, _Game.res.bloodParticle2.Random(), this);
            if (bodyPartTr.localScale.x == 0)
                if (child != null)
                {
                    bodyPart = orig;
                    gibs.TryGetValue(bodyPart, out prefab);
                    bodyPartTr=animator.GetBoneTransform(bodyPart);
                }
                else
                    return;
            if (!destroyedHands.Contains(bodyPart))
            {
                destroyedHands.Add(bodyPart);
                var g = Instantiate2(prefab, bodyPartTr.position, bodyPartTr.rotation, pool);
                var gib = g.GetComponentInChildren<Gib>();
                PlayClipAtPoint(gameRes.flesh, bodyPartTr.position);
                // Transform childT = null;
                var tip = GetChild(bodyPart, true);

                g.rotation = tip.HasValue ? Quaternion.LookRotation(animator.GetBoneTransform(tip.Value).position - bodyPartTr.position, bodyPartTr.up) : bodyPartTr.rotation;

                var rig = gib.rigids[0];
                foreach(var a in gib.rigids)
                    a.velocity = Vector3.zero;
                rig.AddForce((Random.insideUnitSphere * .5f + Vector3.up) * 350 * rig.mass);
                rig.AddTorque(Random.insideUnitSphere * 50 * rig.mass);
                bodyPartTr.localScale = Vector3.zero;
                _Pool.Save(g, 5);
            }
        }
    }
      public GunBase gunBase { get; set; }
    public Action resetTextures { get; set; }
    public Bundle skinBundle { get; set; }
#endif



}



