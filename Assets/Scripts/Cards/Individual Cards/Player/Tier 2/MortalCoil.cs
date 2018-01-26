using UnityEngine;
using System.Collections;

public class MortalCoil : AttackCard
{
    public MortalCoil()
    {
        cardType = CardType.MortalCoil;
        InitValues();
    }

    protected override void OnHit(IDamageable damageTarget)
    {
        base.OnHit(damageTarget);
        if(damageTarget is Monster)
        {
            Monster monster = damageTarget as Monster;
            if (monster.markedForDeath) Services.Main.taskManager.AddTask(player.DrawCards(1));
        }
    }
}
