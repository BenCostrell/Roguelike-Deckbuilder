using UnityEngine;
using System.Collections;

public class HitAndRun : AttackCard
{
    public HitAndRun()
    {
        cardType = CardType.HitAndRun;
        InitValues();
    }

    protected override void OnHit(IDamageable target)
    {
        base.OnHit(target);
        player.movesAvailable += 1;
    }
}
