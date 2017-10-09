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
        player.movementCardsSelected.Add(this);
        player.movesAvailable += range;
        lockId = Services.UIManager.nextLockID;
        player.DisableNonMovementCards(lockId);
    }

    public override void OnUnselect()
    {
        CleanupUnselection();
        player.movesAvailable -= range;
    }

    public void OnMovementAct()
    {
        controller.selected = false;
        CardController.currentlySelectedCards.Remove(this);
        CleanupUnselection();
    }

    void CleanupUnselection()
    {
        player.movementCardsSelected.Remove(this);
        player.EnableNonMovementCards(lockId);
    }

    protected virtual int GetRange()
    {
        return baseRange;
    }
}
