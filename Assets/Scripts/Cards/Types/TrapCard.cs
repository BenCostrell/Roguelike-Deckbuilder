using UnityEngine;
using System.Collections;

public class TrapCard : ObjectPlacementCard
{
    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) && tile.containedMonster == null;
    }
}
