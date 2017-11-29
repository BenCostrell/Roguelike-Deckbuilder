using UnityEngine;
using System.Collections;

public class SpreadSeeds : DungeonCard
{
    private const int numSeeds = 2;
    private const int spreadRadius = 6;

    public SpreadSeeds()
    {
        cardType = Card.CardType.SpreadSeeds;
        InitValues();
    }

    public override TaskTree DungeonOnPlay()
    {
        return Services.MapManager.SproutSaplings(player.GetCurrentTile(), numSeeds, 
            spreadRadius, true, Services.MapObjectConfig.PlantGrowthTime);
    }

    protected override float GetPriority()
    {
        return 30f;
    }
}
