using UnityEngine;
using System.Collections;

public class ZombieCard : MonsterCard
{
    public ZombieCard()
    {
        cardType = CardType.Zombie;
        monsterToSpawn = Monster.MonsterType.Zombie;
        InitValues();
    }
}
