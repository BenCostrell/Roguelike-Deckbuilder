using UnityEngine;
using System.Collections;

public class HitAndRun : AttackCard
{
    public HitAndRun()
    {
        cardType = CardType.HitAndRun;
        InitValues();
    }

    protected override void OnHit(Monster monster)
    {
        base.OnHit(monster);
        Services.GameManager.player.movesAvailable += 1;
    }
}
