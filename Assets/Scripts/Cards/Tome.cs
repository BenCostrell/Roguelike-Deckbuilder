using UnityEngine;
using System.Collections;

public class Tome : Card
{
    public Tome()
    {
        cardType = CardType.Tome;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        Services.Main.taskManager.AddTask(Services.GameManager.player.DrawCards(2));
    }
}
