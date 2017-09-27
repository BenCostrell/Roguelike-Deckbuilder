using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class AttackCard : TileTargetedCard
{
    protected int damage;

    public override bool CanPlay()
    {
        return Services.MonsterManager.MonstersWithinRange(range).Count > 0;
    }

    protected virtual void OnHit(Monster monster)
    {
        AttackCardInfo attackInfo = info as AttackCardInfo;
        if (attackInfo.OnHitAudio != null)
            Services.SoundManager.CreateAndPlayAudio(attackInfo.OnHitAudio, 0.5f);
        monster.TakeDamage(damage);
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) && tile.containedMonster != null;
            
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        OnHit(tile.containedMonster);
    }  

    protected override void InitValues()
    {
        base.InitValues();
        AttackCardInfo attackInfo = info as AttackCardInfo;
        damage = attackInfo.Damage;
    }
}
