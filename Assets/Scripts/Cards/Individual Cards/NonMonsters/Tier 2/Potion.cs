using UnityEngine;
using System.Collections;

public class Potion : Card
{
    public Potion()
    {
        cardType = CardType.Potion;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        player.Heal(1);
    }
}
