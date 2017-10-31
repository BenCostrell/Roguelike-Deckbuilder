using UnityEngine;
using System.Collections;

public class BatCard : MonsterCard
{
    public BatCard()
    {
        cardType = CardType.Bat;
        monsterToSpawn = Monster.MonsterType.Bat;
        InitValues();
    }
}
