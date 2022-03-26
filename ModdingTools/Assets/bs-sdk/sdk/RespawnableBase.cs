using UnityEngine;
using UnityEngine.Serialization;


public class RespawnableBase : ItemBaseVarParse,IOnTriggerInit
{
    [FieldAtr] public float timeToRespawn;

#if game
    protected void DestroyAfter()
    {
        if (IsMine && respawn && !pernamentOrNested)
            DelayCall(10, Destroy);
    }
    
    protected void InitRespawn()
    {
        if (timeToRespawn > 0 && IsMine)
            DelayCall(timeToRespawn, () =>
            {
                CallRPC(OnReset);
            });
    }
    public TransformCache transformCache;
    public override void OnLoadAsset()
    {
        
        transformCache = transform.Component<TransformCache>();
        transformCache.IncludeColliders();
        base.OnLoadAsset();
    }

    public override void Start()
    {
        base.Start();
       
    }
    public override void OnPlayerNearEnter(Player pl,bool b)
    {
        base.OnPlayerNearEnter(pl, b);
        if (hideWhenFarAway && pl.IsMainPlayer && transformCache)
            transformCache.visible = b;
    }

    public bool hideWhenFarAway = true;
    public void InitTrigger(Trigger t)
    { 
        if(hideWhenFarAway  && transformCache ) //should be rendered at near
            transformCache.visible = false;
    }
#endif

}