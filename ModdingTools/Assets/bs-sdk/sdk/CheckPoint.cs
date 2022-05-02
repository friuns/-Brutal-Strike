using System.Collections.Generic;
using UnityEngine;

public partial class CheckPoint:ItemBase , IOnPlayerStay
{
    #if game
    public void OnPlayerStay(Player pl, Trigger other)
    {
        if (!pl.lastCheckpoints.Contains(this))
        {
            pl.lastCheckPoint = pos;
            pl.playerAudioSource.PlayOneShot(gameRes.checkPoint);
                    
            
            var checkPointCnt = _Game.baseItems.Count(a => a is CheckPoint);
            
            if (pl.lastCheckpoints.Count > checkPointCnt/ 3)
                pl.lastCheckpoints.RemoveAt(0);
            pl.lastCheckpoints.Add(this);
            pl.RPCIncreaseScore(pl.gameScore.score);
            // if (pl.observing)
                // _Hud.CenterText("Checkpoint!", 1);
        }
        
    }
    #endif
}


