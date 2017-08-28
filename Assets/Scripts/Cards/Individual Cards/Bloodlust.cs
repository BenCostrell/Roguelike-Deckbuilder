using UnityEngine;
using System.Collections;

public class Bloodlust : Card
{
    public Bloodlust()
    {
        cardType = CardType.Bloodlust;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        TaskTree playTasks = Services.GameManager.player.DrawCards(1);
        playTasks.Then(new ActionTask(FinishResolution));
        Services.Main.taskManager.AddTask(playTasks);

    }

    void FinishResolution()
    {
        foreach (Card card in Services.GameManager.player.hand)
        {
            if (card is AttackCard) Services.GameManager.player.movesAvailable += 1;
        }
    }
}
