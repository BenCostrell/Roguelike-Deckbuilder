﻿using UnityEngine;
using System.Collections;

public class Step : Card {

    public Step()
    {
        cardType = CardType.Step;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        Services.GameManager.player.movesAvailable += 1;
    }
}
