using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementCard : TileTargetedCard
{
    public override bool CanPlay()
    {
        return AStarSearch.FindAllAvailableGoals(
            Services.GameManager.player.currentTile, range, false).Count > 0;
    }

    public override bool IsTargetValid(Tile tile)
    {
        return (!tile.IsImpassable() &&
            AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile, range, false)
            .Contains(tile));
    }

    public override void OnTargetSelected(Tile tile)
    {
        Services.GameManager.player.movesAvailable += range;
        Services.GameManager.player.MoveToTile(tile);
    }

    public override void OnSelect()
    {
        List<Tile> tilesInRange =
            AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, false);
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsTargetable();
        }
    }

    public override void OnUnselect()
    {
        List<Tile> tilesInRange =
            AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, false);
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsUntargetable();
        }
        Services.GameManager.player.ShowAvailableMoves();
    }
}
