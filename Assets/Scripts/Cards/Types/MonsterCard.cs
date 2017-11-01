using UnityEngine;
using System.Collections;

public abstract class MonsterCard : DungeonCard
{
    public Monster.MonsterType monsterToSpawn { get; protected set; }

    public override TaskTree DungeonOnPlay()
    {
        return new TaskTree(new SpawnMonster(this));
    }

    public Monster SpawnMonster()
    {
        return Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }

    protected override float GetPriority()
    {
        return 20f;
    }
}
