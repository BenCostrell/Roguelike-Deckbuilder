using UnityEngine;
using System.Collections;

public class Zombie : Monster
{
    public Zombie()
    {
        monsterType = MonsterType.Zombie;
        InitValues();
    }
}
