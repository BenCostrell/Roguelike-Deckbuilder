using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementCard : Card 
{
    public int range;
    protected int lockId;

    public override bool CanPlay()
    {
        return AStarSearch.FindAllAvailableGoals(
            Services.GameManager.player.currentTile, range, false).Count > 0;
    }

    public override void OnSelect()
    {
        Services.GameManager.player.movementCardsSelected.Add(this);
        Services.GameManager.player.movesAvailable += range;
        lockId = Services.UIManager.nextLockID;
        Services.GameManager.player.DisableNonMovementCards(lockId);
    }

    public override void OnUnselect()
    {
        CleanupUnselection();
        Services.GameManager.player.movesAvailable -= range;
    }

    public void OnMovementAct()
    {
        controller.selected = false;
        CardController.currentlySelectedCards.Remove(this);
        CleanupUnselection();
    }

    void CleanupUnselection()
    {
        Services.GameManager.player.movementCardsSelected.Remove(this);
        Services.GameManager.player.EnableNonMovementCards(lockId);
    }
}
