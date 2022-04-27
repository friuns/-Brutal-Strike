using System;
using System.Collections.Generic;
using ObscuredFloat = System.Single;using ObscuredBool = System.Boolean;using ObscuredInt = System.Int32;using ObscuredVector3 =UnityEngine.Vector3;using ObscuredQuaternion =UnityEngine.Quaternion;using ObscuredString  = System.String;
#if game
using ExitGames.Client.Photon;
using shop;
#endif
using UnityEngine;


[Serializable]
public class GunInfo
{
    public int arrayId = -1; //{ get { return arrayId; } }
    public int m_count;
    public float m_secondaryCount;
    #if game
    public GunBase gun
    {
        get { return bs.gunsDict.TryGet(arrayId); }
        set
        {
            if (value == null)
                arrayId = -1;
            else
                arrayId = value.id;
        }
    }
    public GunInfo()
    {
        
    }
    public GunInfo(GunBase Gun)
    {
        gun = Gun;
        if ((object) gun != null)
            secondaryCount = gun.secondaryCountDef;
    }
    public virtual IEnumerable<GunInfo> GetAll()
    {
        yield return this;
    }
    public virtual void Add(GunInfo b)
    {
        secondaryCount = b.secondaryCount;
        count += b.count;
    }
    
    //static GunInfo()
    //{
    //    PhotonPeer.RegisterType(typeof(GunInfo), (byte)'G', SerializeGunInfo, DeserializeGunInfo);
    //}

    public virtual int count { get { return m_count;} set { m_count = value; } }
    public virtual float  secondaryCount { get { return m_secondaryCount;} set { m_secondaryCount = value; } }
    public override string ToString()
    {
        if (secondaryCount == 0)
            return bs.t + count;
        return bs.t + count + "," + secondaryCount;

        //return bs.Many(count,secondaryCount).ToString();
    }
    //public ObscuredInt totalBullets = 0;
    public virtual GunInfo Take(int Count)
    {
        var g = (GunInfo) MemberwiseClone();
        g.count = Count;
        count -= Count;
        return g;
    }
    public GunInfo Clone()
    {
        return (GunInfo)MemberwiseClone();
    }

    public static GunInfo operator +(GunInfo a, GunInfo b)
    {
        a.Add(b);
        return a;
        // b.count += a.count;
        // return b;
    }



    public static GunInfo operator -(GunInfo a, int b)
    {
        a.count -= b;
        return a;
    }
    
    public static GunInfo operator +(GunInfo a, int b)
    {
        a.count += b;
        return a;
    }


    public static GunInfo operator ++(GunInfo a)
    {
        a.count++;
        return a;
    }


    public static GunInfo operator --(GunInfo a)
    {
        a.count--;
        return a;
    }

    public void Write(BinaryWriter bw)
    {
        bw.Write(gun.id);
        bw.Write(count);
        bw.Write(secondaryCount);
    }
    public static GunInfo Read(BinaryReader br)
    {
        var g = new GunInfo();
        g.gun = bs._Game.playerPrefab.gunsDict[br.ReadInt32()];
        g.count = br.ReadInt32();
        g.secondaryCount = br.ReadInt32();
        return g;
    }

    const int structSize = 3 * 4;
    public static readonly byte[] memVector3 = new byte[structSize];
    public static short SerializeGunInfo(StreamBuffer outStream, object customobject)
    {
        GunInfo vo = (GunInfo)customobject;
        int index = 0;
        lock (memVector3)
        {
            Protocol.Serialize(vo.gun.id, memVector3, ref index);
            Protocol.Serialize(vo.count, memVector3, ref index);
            Protocol.Serialize((int) vo.secondaryCount, memVector3, ref index);
            outStream.Write(memVector3, 0, structSize);
        }

        return structSize;
    }
    public static object DeserializeGunInfo(StreamBuffer inStream, short length)
    {
        GunInfo vo = new GunInfo();
        int count, bullets,id;
        lock (memVector3)
        {
            inStream.Read(memVector3, 0, structSize);
            int index = 0;
            Protocol.Deserialize(out id, memVector3, ref index);
            Protocol.Deserialize(out count, memVector3, ref index);
            Protocol.Deserialize(out bullets, memVector3, ref index);
        }
        vo.count = count;
        vo.gun = bs._Game.playerPrefab.gunsDict[id];
        //vo.totalBullets = totalBullets;
        vo.secondaryCount = bullets;
        return vo;
    }
    public override int GetHashCode()
    {
        return arrayId;
    }
    
    public virtual bool IsBetterThan(GunInfo b)
    {
        if (gun is Armor)
        {
            if (b == null)
                return secondaryCount > gun.secondaryCountDef / 3;
            if (secondaryCount != b.secondaryCount)
                return secondaryCount > b.secondaryCount;
        }
        if (b == null) return true;
            
        return gun.IsBetterThan(b?.gun);
    }
 #endif   
}