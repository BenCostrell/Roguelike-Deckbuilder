using UnityEngine;
using System.Collections;

public class Tick : DungeonCard
{
    public Tick()
    {
        cardType = CardType.Tick;
        InitValues();
    }

    public override TaskTree DungeonOnPlay()
    {
        return Services.Main.dungeonDeck.AlterDungeonTimerCount(1);
    }
}
