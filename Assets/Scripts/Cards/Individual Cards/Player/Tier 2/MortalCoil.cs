using UnityEngine;
using System.Collections;

public class MortalCoil : AttackCard
{
    private bool drawTrigger;

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
            if (monster.markedForDeath) drawTrigger = true;
        }
    }

    public override TaskTree PostResolutionEffects()
    {
        if (drawTrigger)
        {
            return player.DrawCards(1);
        }
        else return base.PostResolutionEffects();
    }
}
