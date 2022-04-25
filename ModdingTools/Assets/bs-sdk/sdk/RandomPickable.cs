using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[RequireComponent(typeof(BillboardRenderer))]

public class RandomPickable : ObjectBase, IOnPoolDestroy, IOnPreMatch
{
    public RandomPickableSkin skin;
    public float probabilityFactor = 1;
    public RespawnableBase pickable;
    public string[] weaponIds = new String[0];
    bool inited;
    
#if game
    private float density;
    public override void OnInspectorGUI()
    {
        GUILayout.Label("dens:" + density);
        base.OnInspectorGUI();
    }
    internal bool randomlyGenerated;
    public override void Start()
    {
        base.Start();
        if (!gameLoaded) return;
        cell = Vector3Int.CeilToInt(tr.position / 100);
        _Game.density[cell] += 1;
        // Game.RegisterOnGameEnabled(Init);
        var bx = gameObject.AddComponent<BoxCollider>();
        bx.isTrigger = true;
    }
    private Vector3Int cell;
    public override void Awake()
    {
        base.Awake();
    }
    public override void OnStartGame()
    {
        if (pickable)
            Destroy(pickable.gameObject);
        Init();
        base.OnStartGame();
    }
    [ContextMenu("Init")]
    public void Init()
    {
        if (roomSettings.disableLoot) return;
        
        if (!skin && RandomPickableSkin.prefab)
        {
            skin = Instantiate(RandomPickableSkin.prefab, transform, false);
            skin.localPosition = Vector3.zero;
        }

        
        // Debug.Log("room.sets.seed:"+room.sets.seed);
        MyRandom random = new MyRandom(viewId+room.sets.seed);
        //var r = _LevelEditor.tools.Where(a => a is LootBox || (a is WeaponPickable && ((WeaponPickable)a).gun.price > 0)).Random(random);

        density = randomlyGenerated ? 0 : _Game.density[cell] / 5f;

        var prefab =
            // random.value > .3f ? 
            assetPrefabs.WhereNonAlloc2((WeaponPickable a) => !a.gun.Disabled && a.gun.price>0 && (weaponIds.Length==0|| weaponIds.Contains(a.gun.gunName)) ).WeightedRandom(a => a.Probability + Mathf.Max(0, 1f -density)*10, random);
            // : assetPrefabs.Random(a => a is LootBox,random);
            if (!prefab)
                throw new MyException("coud not find weapon for random pickable");
        bool spawn = gameSettings.lootProbability * probabilityFactor/Mathf.Min(5,density)> random.value;

        PosRot pos = new PosRot(transform, true);
        if (skin)
        {
            skin.gameObject.SetActive(spawn && prefab is WeaponPickable);
            if (skin.place != null) pos = new PosRot(skin.place, true);
        }

        if (spawn)
        {
            
            if (Physics.Raycast(tr.position + Vector3.up * 1, Vector3.down,out var h, 4, Layer.levelMask))
            {
                tr.position = h.point;
                tr.up = h.normal;
            }
            
            prefab.gameObject.SetActive(false);
            pickable = (RespawnableBase) prefab.Instantiate(pos.pos, pos.rot, pernament: false, localOnly: true); //to avoid network overhead, everything initialized locally
            pickable.parent = transform;
            pickable.randomlyGenerated = randomlyGenerated;
            DestroyImmediate(pickable.GetComponentInChildren<Rigidbody>(true));
            prefab.gameObject.SetActive(true);
            pickable.gameObject.SetActive(true);
            if (pickable is Pickable w) w.transformCache.Add(skin.trc);

            bsNetwork[] subs = pickable.GetComponentsInChildren<bsNetwork>();
            int i = 1;
            foreach (bsNetwork bs in subs)
            {
                bs.photonView = photonView;
                bs.siblingID = i++;
            }

            photonView.rpcTarget = subs;

            pickable.randomPickableNested = true;

            if (respawn)
                pickable.timeToRespawn = roomSettings.lootRespawnTime;
        }
    }


    public override void OnDestroy()
    {
        if (bs.exiting) return;
        if (pickable)
            Destroy(pickable.gameObject);
        base.OnDestroy();
    }

    public void OnTriggerEnter(Collider other)
    {
        var cp = other.GetComponentInParent<Player>();
        if (cp)
            SpawnHostage(cp);
    }
    public override void OnReset()
    {
        inited = false;
        base.OnReset();
    }
    public void SpawnHostage(Player pl)
    {
        
        if (!pl.IsMine ||inited || pl.bot||!respawn || !roomSettings.enableHostage || pl.deadOrKnocked || !_Game.started || !_Zone.greenCircle.Inside(pos)) return;
        inited = true;
        
        var deadPlayer = pl.team2.GetDeadPlayer();
        if (deadPlayer && !pl.team2.hostage && Hostage.prefab)
        {
            var f = (Hostage) Hostage.prefab.Instantiate(pos, rot, pernament: false);
            f.CallRPC(f.Init, pl.team);
        }
    }
#else
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
    public void OnPreMatch()
    {
    }
    #endif
}