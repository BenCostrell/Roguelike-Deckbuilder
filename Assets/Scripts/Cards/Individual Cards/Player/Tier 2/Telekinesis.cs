using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Telekinesis : TileTargetedCard
{
    private const int teleportDist = 2;

    public Telekinesis()
    {
        cardType = CardType.Telekinesis;
        InitValues();
        targets = new List<Tile>();
    }

    public override void ClearTargets()
    {
        base.ClearTargets();
        numRequiredTargets = 2;
    }

    public override bool IsTargetValid(Tile tile)
    {
        if (targets.Count == 0)
        {
            return base.IsTargetValid(tile) &&
                (tile.containedMonster != null || 
                (tile.containedMapObject != null && !(tile.containedMapObject is HeavyBrush)));
        }
        else
        {
            return tile.containedMonster == null && tile.containedMapObject == null
                && tile != player.currentTile
                //&& tile.coord.Distance(targets[0].coord) <= teleportDist;
                && base.IsTargetValid(tile);
        }
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        if (SelectionComplete()) {
            if (targets[0].containedMonster != null)
            {
                Services.Main.taskManager.AddTask(
                    targets[0].containedMonster.MoveAlongPath(new List<Tile>() { targets[1] }));
            }
            else
            {
                Services.Main.taskManager
                    .AddTask(new MoveMapObject(targets[0].containedMapObject, targets[1]));
            }
        }
        else
        {
            ShowSecondPhaseTargets();
        }
    }

    void ShowSecondPhaseTargets()
    {
        UnhighlightTilesInRange();
        currentTileRange = TilesInRangeOfSecondPhase();
        foreach (Tile tile in currentTileRange)
        {
            tile.controller.ShowAsTargetable(IsTargetValid(tile));
        }
    }

    List<Tile> TilesInRangeOfSecondPhase()
    {
        //return AStarSearch.FindAllAvailableGoals(targets[0], teleportDist, true);
        return AStarSearch.FindAllAvailableGoals(player.currentTile, range, true);
    }
}
