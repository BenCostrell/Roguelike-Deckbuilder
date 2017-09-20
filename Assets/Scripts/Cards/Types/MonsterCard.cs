﻿using UnityEngine;
using System.Collections;

public abstract class MonsterCard : Card
{
    protected Monster.MonsterType monsterToSpawn;
    public override TaskTree OnDraw(int deckSize, int discardSize, bool playerDraw)
    {
        TaskTree onDrawTasks = base.OnDraw(deckSize, discardSize, playerDraw);
        onDrawTasks.Then(new SpawnMonster(this));
        return onDrawTasks;
    }

    public Monster SpawnMonster()
    {
        return Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }
}
