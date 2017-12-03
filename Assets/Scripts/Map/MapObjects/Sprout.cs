using UnityEngine;
using System.Collections;

public class Sprout : DamageableObject
{
    public Sprout()
    {
        objectType = ObjectType.Sprout;
        InitValues();
    }

    public override bool IsImpassable(bool ignoreDamageableObjects)
    {
        return false;
    }

    public override int GetEstimatedMovementCost()
    {
        return 1;
    }

    public override void CreatePhysicalObject(Tile tile)
    {
        base.CreatePhysicalObject(tile);
        Services.MapManager.OnSproutBirth(this);
    }

    public override void RemoveThis(bool animate)
    {
        base.RemoveThis(animate);
        Services.MapManager.OnSproutDeath(this);
    }
}
