

using System;
using UnityEngine;

public class Database
{

}


public class MyException : Exception
{
    public MyException(string str) : base(str)
    {
    }
}
public static class SceneNames
{
    public static string Menu = "1", bspLoader = "bspMapLoader",modelMap="modelMap",menuCharsZOmbie="menuChars2",menuChars="menuChars",Oculus="oculus";
    public static bool useOld;
    public static string Game { get { return useOld ? "2old" : "2"; } } 
}
public static class Keys
{
    public const KeyCode lookArround = KeyCode.Q;
}
public static class ExecutionOrder
{
    public const int GameInitializer = Loader-4;
    public const int Settings = Loader-1;
    public const int Loader =   -99999; 
    public const int TestSceneLoader = Loader + 1;
//    public const int GlDebug = -200;
    public const int ResourceBundle = -99;
    public const int Input2=-7;
    public const int MobileInput=-6;
    public const int Game =-5;
//    public const int BotSettings = -1;
    public const int Vehicle = -1;
    public const int Default=0;
    public const int ObsCamera = 1005;
    public const int Hud = 1008;
    public const int DeactiveWaitForGame = 1009; //if first in order Awake not called, if last OnEnabled called twice

}
public static class Tag
{
    public const string Lang = "Lang";
    public static readonly int DetailAlbedoMap = Shader.PropertyToID("_DetailAlbedoMap");
    public static readonly int Detail = Shader.PropertyToID("_Detail");
    public static int color = Shader.PropertyToID("_Color");
    public static int mainTexture = Shader.PropertyToID("_MainTex");
    public static char splitChar = '@';
    public const HideFlags HideInHierarchy = HideFlags.HideInHierarchy;
    public const int ZoneID=4020;
    public const int Heal=4021;
    public const string  _LightMap = "_LightMap";
    public const string Glass = "Glass",
        CamOverGui = "CamOverGui",
        Platform = "Platform",
        editorOnly2 = "EditorOnly2",
        editorOnly = "EditorOnly",
        IsMine = "IsMine",
        helmetHolder = "helmetHolder",
        gunPlaceHolder = "gunPlaceHolder",
        Untagged = "Untagged",
        logging = "logging",
        isStatic = "isStatic",
        mapPrefix = "Map/";
    public const string fortniteBlockTrigger = "fortniteBlockTrigger";
}


//public enum HitType
//{
//    Body, HeadShot
//}

#if !NET_4_6
public struct Tuple<T1, T2>
{
    public T1 Item1;
    public T2 Item2;

    public Tuple(T1 Item1, T2 Item2)
    {
        this.Item1 = Item1;
        this.Item2 = Item2;
    }
}
#endif



public class ValidateAttribute : System.Attribute
{

}
public class IStaticCtorResetSetToNull : Attribute //obsolete 
{

}



#if game
public class TriggerBase : Base
{
    public new bool enabled { get { return base.enabled; } set { collider.enabled = base.enabled = value; } }
    public virtual void OnDisable()
    {
    }
}
#endif