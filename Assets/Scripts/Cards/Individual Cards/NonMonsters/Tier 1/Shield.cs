using UnityEngine;
using System.Collections;

public class Shield : Card
{
    public Shield()
    {
        cardType = CardType.Shield;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        player.GainShield(1);
    }
}
