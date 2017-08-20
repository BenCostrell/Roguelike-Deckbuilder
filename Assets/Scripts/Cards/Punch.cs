using UnityEngine;
using System.Collections;

public class Punch : AttackCard
{
    public Punch()
    {
        cardType = CardType.Punch;
        info = Services.CardConfig.GetCardOfType(cardType);
        InitValues();
    }
}
