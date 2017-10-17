using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovementCard : Card 
{
    public int range { get { return GetRange(); } }
    protected int baseRange;
    protected int lockId;

    public override bool CanPlay()
    {
        return AStarSearch.FindAllAvailableGoals(player.currentTile, range).Count > 0;
    }

    public override void OnSelect()
    {
        player.movesAvailable += range;
        lockId = Services.UIManager.nextLockID;
        player.DisableNonMovementCards(lockId);
    }

    public override void OnUnselect()
    {
        player.movesAvailable -= range;
        player.EnableNonMovementCards(lockId);
    }

    public void OnMovementAct()
    {
        player.EnableNonMovementCards(lockId);
    }

    protected virtual int GetRange()
    {
        return baseRange;
    }
}
