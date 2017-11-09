using UnityEngine;
using System.Collections;

public class HeavyBrush : MapObject
{
    public HeavyBrush()
    {
        objectType = ObjectType.HeavyBrush;
        InitValues();
    }

    public override bool IsImpassable(bool ignoreDamageableObjects)
    {
        return true;
    }

    public override int GetEstimatedMovementCost()
    {
        return 10;
    }
}
