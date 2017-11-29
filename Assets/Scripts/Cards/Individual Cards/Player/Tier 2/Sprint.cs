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
        player.movesAvailable += 3;
    }
}
