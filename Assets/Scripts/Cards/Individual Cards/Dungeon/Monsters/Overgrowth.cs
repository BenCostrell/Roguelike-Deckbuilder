using UnityEngine;
using System.Collections;

public class Overgrowth : DungeonCard
{
    private const int radiusFromPlayer = 6;
    private const int numNewBrushes = 2;
    private const float staggerTime = 0.6f;

    public Overgrowth()
    {
        cardType = CardType.Overgrowth;
        InitValues();
    }

    public override TaskTree DungeonOnPlay()
    {
        return Growth();
    }

    TaskTree Growth()
    {
        return Services.MapManager.Overgrowth(radiusFromPlayer, numNewBrushes, staggerTime);
    }

    protected override float GetPriority()
    {
        return 2f;
    }
}
