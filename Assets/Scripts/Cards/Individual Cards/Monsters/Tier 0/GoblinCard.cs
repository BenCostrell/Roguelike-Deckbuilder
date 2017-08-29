using UnityEngine;
using System.Collections;

public class GoblinCard : MonsterCard
{
    public GoblinCard()
    {
        cardType = CardType.Goblin;
        monsterToSpawn = Monster.MonsterType.Goblin;
        InitValues();
    }
}
