using UnityEngine;
using System.Collections;

public class Overgrowth : DungeonCard
{
    private const int radiusFromPlayer = 8;
    private const float proportionOfGrowth = 0.1f;
    private const float staggerTime = 0.4f;

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
        return Services.MapManager.Growth(radiusFromPlayer, proportionOfGrowth, staggerTime);
    }
}
