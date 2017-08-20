using UnityEngine;
using System.Collections;

public class Goblin : Monster
{
    public Goblin()
    {
        monsterType = MonsterType.Goblin;
        info = Services.MonsterConfig.GetMonsterOfType(monsterType);
        InitValues();
    }
}
