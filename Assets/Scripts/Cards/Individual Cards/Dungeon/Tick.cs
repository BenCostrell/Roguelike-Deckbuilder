using UnityEngine;
using System.Collections;

public class Tick : DungeonCard
{
    public Tick()
    {
        cardType = CardType.Tick;
        InitValues();
    }

    public override TaskTree OnDraw()
    {
        TaskTree onDrawTasks = base.OnDraw();
        onDrawTasks.Then(Services.Main.dungeonDeck.AlterDungeonTimerCount(1));
        return onDrawTasks;
    }

    public override Color GetCardFrameColor()
    {
        return Services.CardConfig.DungeonCardColor;
    }
}
