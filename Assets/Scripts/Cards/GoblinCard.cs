using UnityEngine;
using System.Collections;

public class GoblinCard : MonsterCard
{
    public GoblinCard()
    {
        cardType = CardType.Goblin;
        info = Services.CardConfig.GetCardOfType(cardType);
        monsterToSpawn = Monster.MonsterType.Goblin;
    }
}
