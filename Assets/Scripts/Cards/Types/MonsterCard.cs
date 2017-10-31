using UnityEngine;
using System.Collections;

public abstract class MonsterCard : DungeonCard
{
    public Monster.MonsterType monsterToSpawn { get; protected set; }

    public override TaskTree OnDraw()
    {
        TaskTree onDrawTasks = base.OnDraw();
        onDrawTasks.Then(new SpawnMonster(this));
        return onDrawTasks;
    }

    public Monster SpawnMonster()
    {
        return Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }
}
