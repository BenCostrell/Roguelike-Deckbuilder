using UnityEngine;
using System.Collections;

public class Advance : DungeonCard
{
    public Advance()
    {
        cardType = CardType.Advance;
        InitValues();
    }

    public override TaskTree DungeonOnPlay()
    {
        TaskTree onPlayTasks = Services.MonsterManager.MonstersMove();
        onPlayTasks.Then(Services.MonsterManager.MonstersAttack());
        return onPlayTasks;
    }

    protected override float GetPriority()
    {
        return 10f;
    }
}
