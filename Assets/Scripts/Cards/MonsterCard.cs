using UnityEngine;
using System.Collections;

public abstract class MonsterCard : Card
{
    protected Monster.MonsterType monsterToSpawn;
    public override void OnDraw()
    {
        Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }
}
