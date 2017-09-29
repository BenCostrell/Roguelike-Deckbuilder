using UnityEngine;
using System.Collections;

public class Run : MovementCard
{
    public Run()
    {
        cardType = CardType.Run;
        InitValues();
        range = 2;
    }

    public override void OnPlay()
    {
        base.OnPlay();
    }
}
