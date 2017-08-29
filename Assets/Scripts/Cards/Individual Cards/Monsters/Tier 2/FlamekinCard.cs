using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamekinCard : MonsterCard {

	public FlamekinCard()
    {
        cardType = CardType.Flamekin;
        monsterToSpawn = Monster.MonsterType.Flamekin;
        InitValues();
    }
}
