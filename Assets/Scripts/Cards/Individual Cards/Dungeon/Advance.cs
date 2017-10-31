using UnityEngine;
using System.Collections;

public class Advance : DungeonCard
{
    public Advance()
    {
        cardType = CardType.Advance;
        InitValues();
    }

    public override TaskTree OnDraw()
    {
        TaskTree onDrawTasks = base.OnDraw();
        onDrawTasks.Then(Services.MonsterManager.MonstersMove());
        onDrawTasks.Then(Services.MonsterManager.MonstersAttack());
        return onDrawTasks;
    }
}
