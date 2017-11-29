using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Leapfrog : TileTargetedCard
{
    public Leapfrog()
    {
        cardType = CardType.Leapfrog;
        InitValues();
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) &&
            (tile.GetTileBehind(player.currentTile) != null && 
            !tile.GetTileBehind(player.currentTile).IsImpassable()) &&
            (tile.containedMonster != null || tile.containedMapObject != null);
    }

    public override void OnTargetSelected(Tile tile)
    {
        player.MoveToTile(
            new List<Tile>() { tile.GetTileBehind(player.currentTile), tile },
            true);
    }


}
