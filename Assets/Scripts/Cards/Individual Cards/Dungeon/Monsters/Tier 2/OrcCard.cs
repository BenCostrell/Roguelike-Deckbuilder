using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcCard : MonsterCard {

	public OrcCard()
    {
        cardType = CardType.Orc;
        monsterToSpawn = Monster.MonsterType.Orc;
        InitValues();
    }
}
