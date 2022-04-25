using System;

using UnityEngine;
#if game
using ExitGames.Client.Photon;
#endif
[DefaultExecutionOrder(ExecutionOrder.Default)]
public class GameSettingsInGame:bs,IOnLoadAsset
{
    public new RoomSettings gameSettings = new RoomSettings (); //gamesettings are set onJoinRoom
    // public void Update()
    // {
    //     if(_Game && _Game.enabled && gameSettings != bs.gameSettings)
    //         LogScreen("game settings doesnt match");
    //     
    // }
    #if UNITY_EDITOR && game
    public override void OnInspectorGUI()
    {
        
//        if(!gameObject.CompareTag(Tag.editorOnly))
//            LabelError("tag should be editor only");

        var enumPopup = (GameType)UnityEditor.EditorGUILayout.EnumPopup(gameSettings.gameType);
        if (gameSettings.gameType != enumPopup)
        {
            gameSettings.gameType = enumPopup;
            gameSettings.map.SelectGameTypeChangedLocal(gameSettings);
            SetDirty();
        }
        base.OnInspectorGUI();
    }
    
    #endif
#if game
    public void OnLoadAsset()
    {
        // var gameSettingsInGame = FindObjectOfType<GameSettingsInGame>();
        // if (gameSettingsInGame && room.sets.version >= 2512)
        // {
        
            
        Serializer.DeepCopy(gameSettings, room.sets);
        gameSettings = room.sets; //for inspector
        room.varParse.UpdateValues(ForceRead: true);
        if (Application.isEditor)
            room.sets.mpVersion = settings.mpVersion;
        //     // refreshGameValues?.Invoke();
        // }
    }
    #endif
}