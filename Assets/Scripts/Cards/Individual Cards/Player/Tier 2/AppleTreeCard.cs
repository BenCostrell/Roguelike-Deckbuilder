using UnityEngine;
using System.Collections;
using System;

public class AppleTreeCard : PlantCard
{
    public AppleTreeCard()
    {
        cardType = CardType.AppleTree;
        InitValues();
    }

    public override void OnConsume()
    {
        //
        player.Heal(1);
    }
}
