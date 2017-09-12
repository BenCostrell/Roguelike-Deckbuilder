using UnityEngine;
using System.Collections;

public abstract class MonsterCard : Card
{
    protected Monster.MonsterType monsterToSpawn;
    public override TaskTree OnDraw()
    {
        TaskTree onDrawTasks = base.OnDraw();
        onDrawTasks.Then(new SpawnMonster(this));
        onDrawTasks.Then(Services.GameManager.player.DrawCards(1));
        return onDrawTasks;
    }

    public Monster SpawnMonster()
    {
        Services.Main.taskManager.AddTask(Services.GameManager.player.PlayCard(this));
        return Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }
}
