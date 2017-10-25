using UnityEngine;
using System.Collections;

public class Tick : Card
{
    public Tick()
    {
        cardType = CardType.Tick;
        InitValues();
        isDungeon = true;
    }

    public override TaskTree OnDraw(int deckSize, int discardSize, bool playerDraw)
    {
        TaskTree onDrawTasks = base.OnDraw(deckSize, discardSize, playerDraw);
        onDrawTasks.Then(Services.Main.dungeonDeck.AlterDungeonTimerCount(1));
        return onDrawTasks;
    }

    public override Color GetCardFrameColor()
    {
        return Services.CardConfig.DungeonCardColor;
    }
}
