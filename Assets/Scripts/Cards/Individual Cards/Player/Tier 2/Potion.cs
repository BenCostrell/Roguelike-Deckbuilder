using UnityEngine;
using System.Collections;

public class Potion : Card
{
    public Potion()
    {
        cardType = CardType.Potion;
        InitValues();
    }

    public override TaskTree OnPlay()
    {
        player.Heal(1);
        return base.OnPlay();
    }
}
