using UnityEngine;
using System.Collections;
using System;

public abstract class ObjectPlacementCard : TileTargetedCard
{
    protected MapObject.ObjectType objectTypeToPlace;
    protected override void InitValues()
    {
        base.InitValues();
        ObjectPlacementCardInfo opInfo = info as ObjectPlacementCardInfo;
        objectTypeToPlace = opInfo.ObjectType;
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) &&
            tile.containedMapObject == null &&
            tile.containedMonster == null &&
            tile != player.currentTile;
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        MapObject mapObject = Services.MapObjectConfig.CreateMapObjectOfType(objectTypeToPlace);
        mapObject.CreatePhysicalObject(tile);
    }
}
