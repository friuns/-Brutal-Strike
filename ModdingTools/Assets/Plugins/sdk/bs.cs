using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using System.Linq;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using UnityEditor;
using bs = Base;

public class BloodDecalDrop:MonoBehaviour{}
public interface IPosRot
{
    
}


public class FieldAtr:Attribute
{
    public bool inherit;
    public bool dontDraw;
    public bool readOnly { get { return false; } set { } }
    public int priority { get { return 0; } set { } }
    public bool devOnly { get { return false; } set { } }
    public bool preGameOnly { get { return false; } set { } }
    public GameType gameType { get { return (GameType)0; } set { } }
}
// [Serializable]
// public class AnimationDict : SerializableDictionary<Anims, AnimationClip>
// {
//
// }
public enum Anims
{
    shoot, shoot2, reload, endReload, draw, startShoot, endThrow, pressButton, fly,parachute,Executing,idle,Die
}
public interface IOnTriggerInit{}
public interface IOnTriggerEnter{}
public interface IOnPlayerNear
{
    
}
public interface ISkipIdent
{
    
    
}public interface IVarParseSkipIdent { }
public interface IVarParseDraw
{
    
}
[Flags]
public enum WeaponSetId:int { a1 = 1, a2 = 2, a3 = 4, b1 = 8, b2 = 16, b3 = 32, b4 = 64, b5 = 128 }
public class Input2
{
    
}
namespace EnumsNET
{

}

public interface ISkinBase
{
    
}


public class FieldAtrEnd : Attribute
{
    
}
public class PunRPC : Attribute
{
    
}
public class RoomPulbic:Attribute{}
public class FieldAtrStart:Attribute
{
    public bool inherit { get { return false; } set { } }
    public bool readOnly { get { return false; } set { } }
    public GameType gameType;
}
public class GameTypes
{
    public const GameType noRespawnTypes =  GameType.Classic | GameType.Survival | GameType.Mission | GameType.zombieMode ;
}
public interface IOnInspectorGUIHide
{
    void OnInspectorGUI();
}
public interface IOnInspectorGUISearch{}
public interface IOnInspectorGUI
{
    void OnInspectorGUI();
}

public class Tutorial
{
    
}
public interface ISetLife
{
    
}
public interface IPointable
{
    
}
namespace UnityExtensions
{
public class ReorderableList:Attribute
{
    public Type enumType;
}

}

public interface IOnLoadAsset{}

public interface  IOnLevelEditorGUI{}

namespace ExitGames.Client.Photon
{

}
namespace shop{}

public interface IOnPlayerStay:ITriggerEvent
{
    
}
public interface IBotPickable
{}
public interface IDrawInventory
{
    
}
public interface IOnPoolDestroy
{
    
}
public interface ITriggerEvent
{
    
}
public interface IOnPlayerEnter:ITriggerEvent
{
    
}
public interface IOnPlayerExit:ITriggerEvent
{
    
}
public class QualityControllerBase:bs{}
public class TriggerBase : bs
{
    
}
public interface IOnStartGame{}
public interface  IOnPreMatch{}
public interface ItemBaseVarParseEnable{}
public interface IPreLoadAsset{}
public interface IVarParseValueChanged{}
public static class PhotonNetwork
{
    public static string[] playerList = new string[0];

}
public static class Debug2
{
    public static void Log(string applyingSkinFrom)
    {
        Debug.Log(applyingSkinFrom);
    }
}