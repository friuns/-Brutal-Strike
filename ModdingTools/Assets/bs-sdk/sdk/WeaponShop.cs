using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class DropBox{}
public class WeaponShop : WeaponShopBase,ItemBaseVarParseEnable
{
    #if game
    public override void Awake()
    {
        if (isDefault)
        {
            var enable = ((Component) defaultWeaponShop == null || gameObject.scene.name != SceneNames.Game);
            disabled = !enable;
            if (enable)
                defaultWeaponShop = this;
        }
        
        base.Awake();
    }
    internal WeaponShopCollection defWeapons;
    public override void Start()
    {
        defWeapons = weapons;
        // if (roomSettings.awpOnly) //settings loads after awake
            // disabled = true;
            
        base.Start();
        CreateWeapons();
    }
    public override void OnPreMatch()
    {
        base.OnPreMatch();
        
//        if (isMaster)
        CreateWeapons();
//            if (inRoom)
//                Sync();
    }
    private void CreateWeapons()
    {
        if (allWeapons)
        {
            weapons = new WeaponShopCollection();
            foreach (GunBase a in _ObsPlayer.guns)
                weapons.Add(a, 1);
        }
        else if (randomWeapons)
        {
            weapons = new WeaponShopCollection();
            var m = money;
            while (true)
            {
                var weps = _ObsPlayer.guns.WhereNonAlloc2(a => a.price < m && a.price > 0);
                if (weapons.Count == 0) weps = weps.WhereNonAlloc2(a => a.groupNumber == WeaponGroupE.Pistol);
                if (weps.Count == 0) break;

                GunBase gun = weps.WeightedRandom(a => a.price);
                if (gun is Weapon w && w.ammoPrefab) //using ammoprefab because guns gets filled when player selects class
                    weapons.Add(w.ammoPrefab, w.ammoPrefab.dropAmmount * 4);
                m -= gun.m_price;

                if (gun is Ammo == false)
                    weapons.Add(gun, gun.dropAmmount);
            }
        }
    }
#endif
    
}