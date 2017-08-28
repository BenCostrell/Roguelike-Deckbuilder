using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class AttackCard : TargetedCard
{
    protected int damage;

    public override bool CanPlay()
    {
        return Services.MonsterManager.MonstersWithinRange(range).Count > 0;
    }

    protected virtual void OnHit(Monster monster)
    {
        monster.TakeDamage(damage);
    }

    public override bool IsTargetValid(Tile tile)
    {
        return (tile.containedMonster != null && 
            tile.coord.Distance(Services.GameManager.player.currentTile.coord) <= range);
    }

    public override void OnTargetSelected(Tile tile)
    {
        OnHit(tile.containedMonster);
    }

    

    protected override void InitValues()
    {
        base.InitValues();
        AttackCardInfo attackInfo = info as AttackCardInfo;
        damage = attackInfo.Damage;
    }
}
