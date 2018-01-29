using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Swap : TileTargetedCard
{
    public Swap()
    {
        cardType = CardType.Swap;
        InitValues();
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) &&
            tile.containedMapObject != null && tile.containedMapObject is LightBrush;
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        if(player.currentTile.containedMapObject != null)
        {
            Services.Main.taskManager
                    .AddTask(new MoveMapObject(player.currentTile.containedMapObject, 
                    tile));
        }
        List<Tile> playerToTarget = AStarSearch.ShortestPath(player.currentTile, tile, true);
        player.MoveToTile(playerToTarget, true);
        Services.Main.taskManager
                    .AddTask(new MoveMapObject(tile.containedMapObject, player.currentTile));
    }
}
