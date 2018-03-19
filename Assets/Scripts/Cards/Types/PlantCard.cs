using UnityEngine;
using System.Collections;

public abstract class PlantCard : ObjectPlacementCard
{
    public override bool IsTargetValid(Tile tile)
    {
        return tile.coord.Distance(player.currentTile.coord) <= range &&
            tile.containedMapObject == null &&
            tile.containedMonster == null;
    }

    public override void OnTargetSelected(Tile tile)
    {
        if (tile == player.currentTile) OnConsume();
        else base.OnTargetSelected(tile);
    }

    public abstract void OnConsume();

    public override TaskTree PostResolutionEffects()
    {
        player.cardsInPlay.Remove(this);
        return new TaskTree(new DestroyCard(this));
    }
}
