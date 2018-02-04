using UnityEngine;
using System.Collections;

public class Shield : Card
{
    public Shield()
    {
        cardType = CardType.Shield;
        InitValues();
    }

    public override TaskTree OnPlay()
    {
        player.GainShield(2);
        return base.OnPlay();
    }
}
