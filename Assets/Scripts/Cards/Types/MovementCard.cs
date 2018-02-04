using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MovementCard : Card 
{
    public int range { get { return GetRange(); } }
    protected int baseRange;
    protected int lockId;
    private bool queued;

    public override bool CanPlay()
    {
        return AStarSearch.FindAllAvailableGoals(player.currentTile, range).Count > 0;
    }

    public void OnQueue()
    {
        player.movesAvailable += range;
        lockId = Services.UIManager.nextLockID;
        player.DisableNonMovementCards(lockId);
        queued = true;
    }

    public void OnUnqueue()
    {
        player.movesAvailable -= range;
        player.EnableNonMovementCards(lockId);
        queued = false;
    }

    public override void OnSelect()
    {

    }

    public override void OnUnselect()
    {

    }

    public void OnMovementAct()
    {
        player.EnableNonMovementCards(lockId);
    }

    protected virtual int GetRange()
    {
        return baseRange;
    }

    public override TaskTree OnPlay()
    {
        if(!queued) player.movesAvailable += range;
        return base.OnPlay();
    }

    public override TaskTree OnDraw()
    {
        queued = false;
        return base.OnDraw();
    }
}
