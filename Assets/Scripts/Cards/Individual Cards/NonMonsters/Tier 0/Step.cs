using UnityEngine;
using System.Collections;
using System;

public class Step : MovementCard {

    public Step()
    {
        cardType = CardType.Step;
        InitValues();
        range = 1;
    }

    public override void OnPlay()
    {
        base.OnPlay();
    }
}
