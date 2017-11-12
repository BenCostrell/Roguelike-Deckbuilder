using UnityEngine;
using System.Collections;

public class GrowtheRanks : DungeonCard
{
    public GrowtheRanks()
    {
        cardType = CardType.GrowTheRanks;
        InitValues();
    }

    public override TaskTree DungeonOnPlay()
    {
        return Services.Main.dungeonDeck.AddMonster(this);
    }

    protected override float GetPriority()
    {
        return 5f;
    }
}
