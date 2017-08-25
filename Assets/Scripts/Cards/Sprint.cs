using UnityEngine;
using System.Collections;

public class Sprint : Card
{
    public Sprint()
    {
        cardType = CardType.Sprint;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        Services.GameManager.player.movesAvailable += 3;
    }
}
