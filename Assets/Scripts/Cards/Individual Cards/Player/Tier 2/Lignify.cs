using UnityEngine;
using System.Collections;

public class Lignify : TileTargetedCard
{
    public Lignify()
    {
        cardType = CardType.Lignify;
        InitValues();
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) && tile.containedMonster != null;
    }

    public override void OnTargetSelected(Tile tile)
    {
        Monster monster = tile.containedMonster;
        monster.Die();
        Services.Main.taskManager.AddTask(new GrowObject(tile, Services.MapObjectConfig.PlantGrowthTime,
            Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.LightBrush)));
    }
}
