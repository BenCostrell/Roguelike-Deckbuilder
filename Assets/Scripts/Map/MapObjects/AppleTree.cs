using System.Collections.Generic;
using UnityEngine;

public class AppleTree : GrowingPlant
{
    public AppleTree()
    {
        objectType = ObjectType.AppleTree;
        InitValues();
    }

    protected override void OnFullyGrown()
    {
        base.OnFullyGrown();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        if (fullyGrown) BearFruit(ObjectType.Apple);
    }
}
