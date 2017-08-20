using UnityEngine;
using System.Collections;

public abstract class MonsterCard : Card
{
    protected Monster.MonsterType monsterToSpawn;
    public override void OnDraw()
    {
        base.OnDraw();
        Services.MonsterManager.SpawnMonster(monsterToSpawn);
    }
}
