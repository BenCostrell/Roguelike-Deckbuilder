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

    public override void BeginTargeting()
    {
        base.BeginTargeting();
        numRequiredTargets = 2;
    }

    public override bool IsTargetValid(Tile tile)
    {
        if (targets.Count == 0)
        {
            return base.IsTargetValid(tile) && tile.containedMonster != null;
        }
        else
        {
            return !tile.IsImpassable() && (tile.coord.Distance(targets[0].coord) <= teleportDist);
        }
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        if (SelectionComplete()) { 
            Services.Main.taskManager.AddTask(
                targets[0].containedMonster.MoveAlongPath(new List<Tile>() { targets[1] }));
        }
        else
        {
            ShowSecondPhaseTargets();
        }
    }

    void ShowSecondPhaseTargets()
    {
        OnUnselect();
        List<Tile> tilesInRange = TilesInRangeOfSecondPhase();
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsTargetable(IsTargetValid(tile));
        }
    }

    List<Tile> TilesInRangeOfSecondPhase()
    {
        return AStarSearch.FindAllAvailableGoals(targets[0], teleportDist, true);
    }
}
