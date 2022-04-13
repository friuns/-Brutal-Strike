using System;
using System.Collections.Generic;
using System.Linq;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponShopBase :Pickable , IBotPickable,IOnStartGame
{
    public bool isDefault;
    [FieldAtrStart] 
    public ObscuredInt timeToBuy = 0;
    //{ get { return timeToBuy == -1; } set { if (value) timeToBuy = -1; } }
    public bool timeInfinite { get { return timeToBuy == 0; } }
    
    public bool allWeapons = true;
    public bool randomWeapons;
    public int money = 19000;
    

    [FieldAtrEnd]
    //public Transform attachTo;
    private int tmp23;
    #if game
    public override bool isBotPickable { get { return base.isBotPickable && !isDefault && !bspGenerated; } }
    public override void OnBotReachedTarget(Player pl)
    {
        AutoBuyRandomWeapons(pl);
    }
    public override void OnLevelEditorGUI()
    {
        if (this == defaultWeaponShop)
        {
            var rs = gameSettings;
            rs.nextRoundMoney = IntField("Next Round Money", rs.nextRoundMoney);
            rs.randomBuyWeapons = Toggle(rs.randomBuyWeapons, "Auto buy weapons on Round start");
            rs.startMoney = IntField("Start Money", rs.startMoney);
            room.varParse.UpdateValues();
        }
        
        infinite = Toggle(infinite, "infinite");
        randomWeapons = Toggle(randomWeapons, "randomWeapons");
        allWeapons = Toggle(allWeapons, "all weapons");
        enableMoney = Toggle(enableMoney, "Enable Money");
        if (enableMoney && !allWeapons && randomWeapons)
            money = IntField("Random Weapons budget", money);

        foreach (GunBase gun in _Game.playerPrefab.guns)
        {
            using (BeginHorizontal())
            {
                GUILayout.Label(Tr(gun.name), GUILayout.Width(100));
                if (GUILayout.Button("<"))
                    weapons[gun.id]--;
                GUILayout.Label(this.weapons[gun.id] + "");
                if (GUILayout.Button(">"))
                    weapons[gun.id]++;
            }
        }        
        base.OnLevelEditorGUI();
    }
    protected override void OnCreate(bool b)
    {
        
        Register(this, b);
        base.OnCreate(b);
    }
    public override  bool AvailableTo(Player pl)
    {
        return (!enableMoney || Time.time - pl.spawnTime < timeToBuy || timeInfinite) && !disabled &&
               (isDefault ? bs.roomSettings.canBuyAlways|| InRange(pl.pos-pl.spawnPos,bounds.size.x) : bounds.Contains(pl.mpos));
    }



    public override bool dontSendDataOnPlConnected { get { return false; } }
    
    
    //void Update() 
    //{
    //    if (attachTo)
    //        pos = attachTo.position;
    //}    
    public override void Load()
    {
        if (deadPlayerDrop && this.GetComponentNonAlloc<bl_MiniMapItem>())
        {
            this.GetComponentNonAlloc<bl_MiniMapItem>().SetIcon(!owner.pl._isEnemy || !owner.pl.killedBy || !owner.pl.killedBy._isEnemy ? owner.pl.playerClassPrefab.deadIcon : null);
            //attachTo = owner.pl.skin.hips;
        }
        base.Load();
    }
    

    public override void Save(BinaryWriter bw)
    {
        bw.Write(timeToBuy);
        bw.Write(infinite);
        bw.Write(allWeapons);
        bw.Write(money);
        bw.Write(deadPlayerDrop);
        bw.Write(enableMoney);
        bw.Write(disabled);
        WriteWeapons(bw, weapons);
        base.Save(bw);
    }
    public static void WriteWeapons(BinaryWriter bw, WeaponShopCollection dict)
    {
        bw.Write(dict.Count);
        foreach (KeyValuePair<int, GunInfo> a in dict)
        {
            //Debug.Log("guninfo write "+a.Value);
            bw.Write(a.Key);
            a.Value.Write(bw);
        }
    }

    public override void Load(BinaryReader br) //also serialized by varparse, can be removed
    {
        print("WeaponShop Load");
        timeToBuy = br.ReadInt32();
        infinite = br.ReadBoolean();
        allWeapons = br.ReadBoolean();
        money = br.ReadInt32();
        deadPlayerDrop = br.ReadBoolean();
        enableMoney = br.ReadBoolean();
        disabled = br.ReadBoolean();
        ReadWeapons(br);
        base.Load(br);
    }

    

    public void ReadWeapons(BinaryReader br)
    {
        weapons= new WeaponShopCollection();
        for (int i = br.ReadInt32() - 1; i >= 0; i--)
        {
            var weaponID = br.ReadInt32();
            var gunInfo = GunInfo.Read(br);
            weapons[weaponID] = gunInfo;
        }
    }
    bool dropped;
    public bool deadPlayerDrop;
    public void OnCollisionEnter(Collision c)
    {
        if (dropped || !isMaster || !enableMoney) return;
        for (int i = 0; i < 5; i++)
        {
            var a = InstantiateSceneObject(_Player.money.weaponPickable, pos + ZeroY(Random.insideUnitSphere).normalized * 3, Quaternion.identity, true);
            a.isKinematic = false;
        }

        dropped = true;
    }
    

    public override void OnInspectorGUI()
    {
        GUILayout.Label("total:" + weapons.Sum(a => a.Value.count));
        base.OnInspectorGUI();
    }

    // public bool CanBuy(GunBase gun)
    // {
    //     return  gun.canBuy && weapons[gun.id].count > 0;
    // }
    // public void RebuyLast(Player pl)
    // {
    //     foreach (var a in pl.guns)
    //         if (a.canTake && a.canBuy)
    //         {
    //             for (int i = 0; i < a.lastBought; i++)
    //                 CallRPC(Buy, pl.viewId, a.id);
    //         }
    // }
    
    
    
    private bool CanAutoTake(GunBase gun, int count)
    {
        return gun.canBuy && gun.CanTake(count + 1, true)
                           && weapons[gun.id].count > 0;
    }
    
    
  
    public void AutoBuyAmmo(Player pl)
    {
        //
        // WeaponShopBase shop = this;
        // pl.print("AutoBuyAmmo");
        // List<Weapon> guns = pl.guns.WhereNonAlloc2((Weapon a) => a.have);
        // if (guns.Count == 0) return;
        // float minAmmo = guns.Min(gun => (float) gun.TotalBullets / gun.ammo.dropAmmount);
        // foreach (Weapon gun in guns)
        //     if ((gun.ammo.Count == 0 || minAmmo + .1f >= (float) gun.TotalBullets / gun.ammo.dropAmmount) && gun.ammo.price < pl.inGameMoney && gun.ammoCount<gun.ammo.dropAmmount*1.5f)
        //         shop.RPCBuyOrTake(pl, gun.ammo);
        // gunsToBuy.Add(gun.ammo);
    }
    
  
    public void AutoBuyRandomWeapons(Player pl, int moneyLeft, List<GunBase> gunsToBuy)
    {
        
        gunsToBuy.Clear();
        List<GunBase> weps = TempList<GunBase>.GetTempList();
        for (int i = 0; i < 5; i++) //buy max 5 times
        {
            var guns = pl.getGunsCanBuy();//must be inside loop
//            if (i == 5) Debug.LogError("infinite loop");
            weps.Clear();

            foreach (GunBase gun in guns) //adds all weapons possible to buy all that can buy
            {
                var atachment = gun as AttachmentBase;
                if (atachment != null)
                {
                    //filters out attachments that cannot be attached, (fix for generated atachments)
                    if (!pl.guns.Any(a => a.have && atachment.canAttachToWeapon.Contains(a.weaponType)) &&
                        !gunsToBuy.Any(a => atachment.canAttachToWeapon.Contains(a.weaponType)))
                        continue;
                }


                if (CanAutoTake(gun, gunsToBuy.Count(a => a.groupNumber == gun.groupNumber)) && (gun.price < moneyLeft ))
                    if (i != 0 || gun is Weapon)
                        weps.Add(gun);
            }

            if (weps.Count == 0)
                continue; //if no weapons can buy then stop

            var gunBase = weps.Random();
            gunsToBuy.Add(gunBase);
            moneyLeft -= gunBase.price;
            // if (gun is Weapon)
            //     for (int j = 0; j < 2; j++)
            //         CallRPC(Buy, pl.viewId, (gun as Weapon).ammo.id);
        }
    }
    public void AutoBuyRandomWeapons(Player pl) //old autobuy
    {
        List<GunBase> tmp = TempList<GunBase>.list;
        AutoBuyRandomWeapons(pl, pl.money.Count, tmp);
        foreach(var a in tmp)
            RPCBuyOrTake(pl,a);
            // CallRPC(Buy, pl.viewId, a.id);
        AutoBuyAmmo(pl);
    }

    // public void AutoBuyRandomWeaponsOld(Player pl) //old autobuy
    // {
    //     
    //     AutoBuyAmmo(pl);
    //     List<GunBase> wepsCanBuy = TempList<GunBase>.GetTempList();
    //     for (int i = 0; i < 5; i++) //buy max 5 times
    //     {
    //         //            if (i == 5) Debug.LogError("infinite loop");
    //         wepsCanBuy.Clear();
    //
    //         var guns = pl.getGunsCanBuy();
    //         foreach (GunBase a in guns)
    //             if (a.canTake && (a.price < pl.money.Count || Mission && pl.team == TeamEnum.Terrorists) && CanBuy(a)) //collect all that can buy
    //                 if (i != 0 || a is Weapon)
    //                     wepsCanBuy.Add(a);
    //
    //         if (wepsCanBuy.Count == 0)
    //             continue; //if no weapons can buy then stop
    //
    //         GunBase gun = wepsCanBuy.Random();
    //         CallRPC(Buy, pl.viewId, gun.id);
    //         if (gun is Weapon)
    //             for (int j = 0; j < 2; j++)
    //                 CallRPC(Buy, pl.viewId, (gun as Weapon).ammo.id);
    //     }
    //
    // }

    
    #endif
}
