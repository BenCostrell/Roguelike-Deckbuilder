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
        return base.IsTargetValid(tile) && 
            (tile.containedMonster != null || (
            tile.containedMapObject != null && tile.containedMapObject is IDamageable));
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        if (tile.containedMonster != null)
        {
            Monster monster = tile.containedMonster;
            monster.Die();
            if(tile.containedMapObject != null && tile.containedMapObject is DamageableObject)
            {
                DamageableObject dmgableObj = tile.containedMapObject as DamageableObject;
                dmgableObj.Die();
            }
        }
        else
        {
            DamageableObject dmgableObj = tile.containedMapObject as DamageableObject;
            dmgableObj.Die();
        }
        Services.Main.taskManager.AddTask(new GrowObject(tile, Services.MapObjectConfig.PlantGrowthTime,
            Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.LightBrush)));
    }
}
