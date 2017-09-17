using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileTargetedCard : Card
{
    public int range { get; protected set; }

    public override void OnPlay()
    {
        base.OnPlay();
        //Services.Main.taskManager.AddTask(new TileTargetSelection(this));
    }

    public abstract bool IsTargetValid(Tile tile);

    public abstract void OnTargetSelected(Tile tile);

    protected override void InitValues()
    {
        base.InitValues();
        TargetedCardInfo targetedCardInfo = info as TargetedCardInfo;
        range = targetedCardInfo.Range;
    }

    public override void OnSelect()
    {
        List<Tile> tilesInRange =
            AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, true);
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsTargetable();
        }
    }

    public override void OnUnselect()
    {
        List<Tile> tilesInRange =
            AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, true);
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsUntargetable();
        }
        Services.GameManager.player.ShowAvailableMoves();
    }
}
