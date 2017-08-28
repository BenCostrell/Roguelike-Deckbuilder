using UnityEngine;
using System.Collections;

public class Run : Card
{
    public Run()
    {
        cardType = CardType.Run;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        Services.GameManager.player.movesAvailable += 2;
    }
}
