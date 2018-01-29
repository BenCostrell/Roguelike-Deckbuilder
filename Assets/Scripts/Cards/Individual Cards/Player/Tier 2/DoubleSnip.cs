using UnityEngine;
using System.Collections;

public class DoubleSnip : AttackCard
{
    public DoubleSnip()
    {
        cardType = CardType.DoubleSnip;
        InitValues();
    }

    public override void ClearTargets()
    {
        base.ClearTargets();
        numRequiredTargets = 2;
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) && !targets.Contains(tile);
    }
}
