using UnityEngine;
using System.Collections;
using System;

public class ObjectPlacementCard : TileTargetedCard
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
            tile.containedChest == null &&
            tile.containedMapObject == null &&
            tile.containedKey == null;
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        MapObject mapObject = Services.MapObjectConfig.CreateMapObjectOfType(objectTypeToPlace);
        mapObject.PlaceOnTile(tile);
    }
}
