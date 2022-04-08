using System;
using System.Collections.Generic;
using System.IO;
#if game
using bsp;
using doru;
#endif
//using bsp;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using kv = System.Collections.Generic.KeyValuePair<int, UnityEngine.Transform>;
public enum Rarity{Common,Normal,Rare}

[SelectionBase]
public abstract class ItemBase : BotPickable, IOnLevelEditorGUI
{

//    public List<kv> sceneMeshes = new List<kv>();
    //public Dictionary<string, object> customDict = new Dictionary<string, object>();
    [FieldAtrStart] 
    public float probabilityFactor=1;
    public bool dragScale;
    public bool alignToGround;
    [FieldAtrEnd]
    public string targetName = "";
    protected int tmp;

    // [FormerlySerializedAs("assetPreview")] 
    // public Texture2D icon;
    public Sprite icon;
    
    public Vector3 offset;
    public virtual void Reset()
    {
        if (!GetComponent<PhotonView>())
            gameObject.AddComponent<PhotonView>();
        targetName = name;
    }
#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
#endif
    public Bounds bounds { get { return new Bounds(boundsTr.position, boundsTr.lossyScale + Vector3.one); } }
    public Transform m_boundsTr;
    public Transform boundsTr
    {
        get
        {
            if (m_boundsTr != null)
            {
#if game
                m_boundsTr.gameObject.hideFlags = this is Ladder || settings && settings.showHidden ? HideFlags.None : Tag.HideInHierarchy;
#endif
                return m_boundsTr;
            }
            m_boundsTr = transform.Find("bounds");
            if (m_boundsTr) return boundsTr;
            m_boundsTr = new GameObject("bounds").transform;
            m_boundsTr.SetParent(transform, false);
            if (!Application.isPlaying) SetDirty();
            return m_boundsTr;
        }
    }
#if game
    public Vector3 size { get { return boundsTr.lossyScale; } }
    
    public new virtual string name { get { return base.name; } set { base.name = value; } }
    internal bool draggable { get { return !isStatic && pernament; } }
    internal bool isStatic { get { return m_isStatic; } set { gameObject.isStatic = m_isStatic = value; } }
    public new Transform transform { get { return base.transform; } }

    //private VarParse2 m_varParseSkin; //do make individual id
    //public virtual VarParse2 varParse { get { return m_varParseSkin ?? (m_varParseSkin = new VarParse2(this, path, DrawAll: IsMine)); } }
    
    
    public void InitTriggers()
    {
        foreach (var a in GetComponentsInChildren<Collider>(true))
//            if(a.isTrigger)
        {
            // if (this is Door d && d.physics && a is MeshCollider m)
                // m.convex = true; // rigidbody fix
            a.gameObject.layer = Layer.trigger;
//            a.enabled = true;
                var t = a.gameObject.GetComponentNonAlloc<Trigger>();
                if (t == null)
                {
                    t = a.gameObject.AddComponent<Trigger>();
                    t.handler = this;
                }
        }
    }

    protected override void OnCreate(bool b)
    {
        Register<ItemBase>(this, b);
        base.OnCreate(b);
    }

    

    //[PunRPC]
    //public void SetProperty(string s, object o)
    //{
    //    print("SetProperty " + s + ":" + o);
    //    customDict[s] = o;
    //    //varParse.SetValue2(s, o);
    //}
    //[HideInInspector]
    //public ItemBase m_prefab;
    public ItemBase prefab
    {
        get
        {
            return Resources2.Load<ItemBase>(path);
        }
    }
    public virtual ItemBase CreateOrPlace(Vector3 pos, Quaternion rot,bool create,bool localOnly)
    {
        if (create) 
            return Instantiate(pos, rot);
//        this.pos = pos;
//        this.rot = rot;
        return this;
    }
    public virtual ItemBase Instantiate(Vector3 pos, Quaternion rot, bool localOnly = false, bool pernament = true, bool sceneObject = true,string name=null)
    {
        //if (gameObject.activeInHierarchy)
        //    Debug.Log("should be prefab");

        //if (photonView) 
        //    photonView.viewID = 0;//will damage current prefab

        ItemBase itemBase = !localOnly && PhotonNetwork.inRoom ? InstantiateSceneObject(path, pos, rot, sceneObject).GetComponent<ItemBase>() : Instantiate(this, pos, rot);
        
        //if (PhotonNetwork.inRoom)
        //Debug2.LogWarning("Created in game " + name, itemBase.gameObject);
        itemBase.name = name ?? this.name;
        itemBase.levelEditorCreated = pernament;
        itemBase.OnInstanciate(this);
        
        return itemBase;
    }
    public virtual void OnInstanciate(ItemBase prefab)
    {
        
    }


    [PunRPCBuffered]
    public void SetBytes(System.ArraySegment<byte> bts, bool fast = false)
    {
        SetBytes2(Temp.GetBinaryReaderFromSegment(bts));
    }
    public void SetBytes2(BinaryReader BinaryReader)
    {
        try
        {
            Load(BinaryReader);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        Load();
    }
    internal bool loaded;
    public bsp.BSPModel bspModel = new bsp.BSPModel();
    public List<string> vdfInfo = new List<string>();
    public Entity vdf; 
//    internal int materialId;
//    internal byte alpha = 255;


    public virtual void Save(BinaryWriter bw)
    {
   
//        bw.Write(materialId);
//        bw.Write(sceneMeshes.Count);
//
//        foreach (var t in sceneMeshes)
//        {
//            bw.Write(t.Key);
//            bw.Write(t.Value.position);
//            bw.Write(t.Value.rotation);
//        }
//        bw.Write(boundsTr.localPosition);
//        bw.Write(boundsTr.localScale);
    }
    public virtual void Load(BinaryReader br)
    {
        if (dontSendDataOnPlConnected && loaded)
        {
            Debug.LogError("loaded already "+name,gameObject);
        }
        loaded = true;
    

//        materialId = br.ReadInt32();

//        boundsTr.localPosition = br.ReadVector3();
//        boundsTr.localScale = br.ReadVector3();
    }
    
    public virtual void Load()
    {
        
        
        //pos += bspOrigin;
    }
    //public static MemoryStream ms = new MemoryStream();
    //public static BinaryReader br = new BinaryReader(ms);
    //public static BinaryWriter bw = new BinaryWriter(ms);
    public System.ArraySegment<byte> GetBytes(bool fast = false)
    {
        Temp.ms.SetLength(0);
        Save(Temp.bw);
        return Temp.ms.ToArrayNonAlloc();
    }
    public virtual bool dontSendDataOnPlConnected { get { return pernamentOrNested; } } //will override In WeaponShop and Zone
    public override void OnPlConnected(PhotonPlayer photonPlayer)
    {
        base.OnPlConnected(photonPlayer);

        if (dontSendDataOnPlConnected || customServer) return;
        CallRPC(SetBytes, GetBytes(), false);
        //CallRPC(SetCreated, editorCreated);
        //foreach (var a in customDict)
        //    CallRPC(SetProperty, a.Key, a.Value);
    }

    
    //internal Vector3 bspOrigin;
    //public float order;
#if UNITY_EDITOR
  
    [ContextMenu("GeneratePreview3")]
    public void CreateAssetPreview3()
    {
        icon=CreateAssetPreviewEditor(new Vector3(-1, -1, -1).normalized, Selection.activeTransform);
    }
    
    [ContextMenu("GeneratePreviewSide")]
    public void CreateAssetPreviewSide()
    {
        icon=CreateAssetPreviewEditor(Vector3.left, Selection.activeTransform);
    }
    [ContextMenu("GeneratePreviewCustom")]
    public void CreateAssetPreviewCustom()
    {
        icon=CreateAssetPreviewEditor(Vector3.zero, Selection.activeTransform);
    }
   
#endif


    //[PunRPC]
    //public void SetCreated(bool Created)
    //{
    //    editorCreated = Created;
    //}


    public bool CanCreate(Vector3 HitposPoint)
    {
        return true;
    }

    public override string ToString()
    {
        return name;
    }
    public override void OnLoadAsset()
    {
        base.OnLoadAsset();
        assetPrefabs.Add(this);
        // if (icon)
        // {
        //     iconSprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
        //     Game.onGameDestroy += () => DestroyImmediate(iconSprite);
        // }
    }


    public bool m_isStatic;
    public override void OnValidate()
    {
        m_isStatic = gameObject.isStatic;
        base.OnValidate();
        //if (photonView != null && (photonView.ObservedComponents.Count == 0 || photonView.ObservedComponents[0] == null))
        //{
        //    photonView.ObservedComponents.Clear();
        //    photonView.ObservedComponents.Add(transform);
        //    photonView.ObservedComponents.Add(rigidbody);
        //    photonView.synchronization = ViewSynchronization.UnreliableOnChange;
        //}

    }
    public override void OnInspectorGUI()
    {
        if (!Application.isPlaying) return;
        GUILayout.Label(IsMine + "");
        if (!GetComponent<PhotonView>())
            LabelError("Missing photonView");
    }
    public float angle;
    public float  pitch;
    public Vector3 origin;
    public Vector3 angles;
    public Vector3 bspModelPos;
    public PosRot posRot { get { return new PosRot(transform); } }

    public virtual void OnLevelEditorGUI()
    {
        var old = offset;
        offset.y = FloatField("height offset", offset.y);
        if (old != offset)
            pos += offset - old;
        alignToGround = Toggle(alignToGround, "Align To Ground");
        dragScale = Toggle(dragScale, "scale");

        //if (isDebug)
        //    foreach (var a in customDict)
        //        LabelC(a.Key, ":", a.Value);
    }

    public virtual void OnStopDrag()
    {
        
    }
    public virtual void OnStartDrag()
    {
    }
    public virtual void CopyFrom(ItemBase b)
    {
        SetBytes(b.GetBytes());
        alignToGround = b.alignToGround;
    }


    public static void RegisterMaxLimit<T>(T item, bool b, int maxCnt) where T : ItemBase 
    {
        
//        if (!item.IsMine || !item.pernament||!respawn) return; //will count per  user
//        
//        if (b)
//        {
//            MaxCountList<T>.list.Add(item);
//            var instances = MaxCountList<T>.list;
//            if (instances.Count > maxCnt && item.IsMine)
//                PhotonNetwork.Destroy(instances[0].gameObject);
//        }
//        else
//            MaxCountList<T>.list.Remove(item);
    }
//    public class MaxCountList<T>
//    {
//        static MaxCountList()
//        {
//            RegisterList.clearInstances += Clear;
//        }
//        public static void Clear()
//        {
//            list.Clear();
//        }
//        public static FastList2<T> list = new FastList2<T>();
//    }

    
    public void OnStopDragging()
    {
    }


    public virtual void ParseBsp()
    {
        transform.position = bspModelPos + origin;
        
    }
#endif
}