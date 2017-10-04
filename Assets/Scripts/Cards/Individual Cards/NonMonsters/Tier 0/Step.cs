using UnityEngine;
using System.Collections;
using System;

public class Step : MovementCard {

    public Step()
    {
        cardType = CardType.Step;
        InitValues();
        baseRange = 1;
    }
}
