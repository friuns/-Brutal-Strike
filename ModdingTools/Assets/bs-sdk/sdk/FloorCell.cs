using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum FloorCellType
{
    floor,
    ramp,
    wall
}
public class FloorCell : BuildScript, IOnTriggerEnter
{
    public Transform visual;
    public FloorCellType cellType;
//    public Material[] oldMaterials;
    public new Renderer[] renderers;
    #if game
    public new bool selected { set { visual.SetActive(value); } }
    public new Quaternion rot
    {
        get { return inited2 ? base.rot : visual.rotation; }
        set
        {
            if (inited2)
                base.rot = value;
            else
                visual.rotation = value;
        }
    }
    public new Vector3 pos //do not override
    {
        get { return inited2 ? base.pos : visual.position; }
        set
        {
            if (inited2) base.pos = value;
            else visual.position = value;
        }
    }

    public override void OnLoadAsset()
    {
        visual = Instantiate(visual);
        visual.gameObject.SetActive(false);
        foreach (Renderer a in visual.GetComponentsInChildren<Renderer>())
            a.sharedMaterials = new Material[a.sharedMaterials.Length].Fill(_ => gameRes.flickerMaterial);
        base.OnLoadAsset();
    }
    private bool inited2;

    public override void Start()
    {
        base.Start();
        inited2 = true;
        _Game.floorCells.Add(this);
        foreach (var a in GetComponentsInChildren<BuildScript>())
            a.enabled = true;
        foreach (var a in GetComponentsInChildren<Rigidbody>(true))
            Destroy(a);
        foreach (var a in GetComponentsInChildren<Collider>(true))
            a.isTrigger = false;
//        for (int i = 0; i < renderers.Length; i++)
//            renderers[i].sharedMaterial = oldMaterials[i];

//        photonView.didAwake = false;
    }

    public Vector3Int posi { get { return Vector3Int.RoundToInt(pos / Hammer._4); } }

    public override ItemBase Instantiate(Vector3 pos, Quaternion rot, bool localOnly = false, bool pernament = true, bool sceneObject = true, string name = null)
    {
        FloorCell c = (FloorCell) base.Instantiate(pos, rot, localOnly, pernament, sceneObject, name);

        return c;
    }
    public void Create()
    {
        Instantiate(pos, rot);
    }
    public override void OnStartGame()
    {
        if(IsMine)
        PhotonNetwork.Destroy(gameObject);
        base.OnStartGame();
    }
    public override void OnDestroy()
    {
        if (exiting) return;
        base.OnDestroy();
        _Game.floorCells.Remove(this);
    }
    [PunRPCBuffered]
    public override void SetLife(int life, Vector3 hitPos, int id,int pv)
    {
        if (life < -1)
        {
            // GetComponentInChildren<BuildScript>(true).Break();
            var pl = ToObject<Player>(pv);
            if(pl!=null)
                foreach(var a in pl.gunsDict)
                    if (a.Value is Hammer h)
                        h.info.secondaryCount++;
        }
            
        this.life = life;
        base.SetLife(life, hitPos, id,pv);
    }

    public const int _4 = Hammer._4;
    public bool Validate()
    {
        foreach (var a in _Game.floorCells)
            if (a.pos == pos && a.cellType == cellType && (cellType != FloorCellType.wall || a.rot == rot))
                return false;

        if (blockTriggers.Count > 0)
            return false;
        Vector3 v2 = pos;
        v2.y += _4 / 2f;
        if (PhysicsRaycast(v2+Vector3.up*4, Vector3.down, _4+4, 1 << Layer.level))
            return true;

        foreach (FloorCell a in triggers)
        {
            var dir = a.posi - posi;
            if ((dir.x & dir.z) == 0)
                return true;
        }
        return false;
    }
#if UNITY_EDITOR
    public override void OnSceneUpdate(SceneView scene)
    {
        base.OnSceneUpdate(scene);
        Handles.Label(transform.position, posi.ToString());
    }
#endif

    private bool PhysicsRaycast(Vector3 v2, Vector3 down, float dist, int layer)
    {
        var hit = Physics.Raycast(v2, down, dist, layer);
        Debug.DrawRay(v2, down * dist, hit ? Color.blue : Color.red);
        return hit;
    }
    List<FloorCell> triggers = new List<FloorCell>();
    List<FloorCell> blockTriggers = new List<FloorCell>();
    public void OnTriggerEnterOrExit(Trigger _this, Trigger other, bool b)
    {
        if (_this.CompareTag(Tag.fortniteBlockTrigger))
        {
            if (other.CompareTag(Tag.fortniteBlockTrigger))
                blockTriggers.AddOrRemove(other.handler as FloorCell, b);
        }
        else
        {
            if (other.handler is FloorCell f)
                triggers.AddOrRemove(f, b);
        }
    }

    public override void Die()
    {
        if (IsMine)
            Destroy();
    }
    public override void RPCDamageAddLife(float damage, int pv = -1, int weapon = -1, HumanBodyBones collId = 0, Vector3 hitPos = default)
    {
        var knife = _Game.playerPrefab.guns.FirstOrDefault(a=>a is Knife);
        if (weapon == knife.id)
            damage *= 10;

        base.RPCDamageAddLife(damage, pv, weapon, collId, hitPos);
    }
    
#endif
}