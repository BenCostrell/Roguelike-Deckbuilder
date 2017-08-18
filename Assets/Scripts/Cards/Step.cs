using UnityEngine;
using System.Collections;

public class Step : Card {

    public Step()
    {
        type = Type.Step;
        info = Services.CardConfig.GetCardOfType(type);
    }

    public override void OnPlay()
    {
        base.OnPlay();
        Services.GameManager.player.movesAvailable += 1;
    }
}
