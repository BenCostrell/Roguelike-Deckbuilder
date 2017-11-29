using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public abstract class AttackCard : TileTargetedCard
{
    protected int damage;

    protected virtual void OnHit(IDamageable damageTarget)
    {
        AttackCardInfo attackInfo = info as AttackCardInfo;
        if (attackInfo.OnHitAudio != null)
            Services.SoundManager.CreateAndPlayAudio(attackInfo.OnHitAudio, 0.5f);
        damageTarget.TakeDamage(damage);
    }

    public override bool IsTargetValid(Tile tile)
    {
        return base.IsTargetValid(tile) && (tile.containedMonster != null || 
            (tile.containedMapObject != null && tile.containedMapObject is IDamageable));
            
    }

    public override void OnTargetSelected(Tile tile)
    {
        base.OnTargetSelected(tile);
        if (tile.containedMonster != null)
            OnHit(tile.containedMonster);
        else if (tile.containedMapObject != null && tile.containedMapObject is DamageableObject)
            OnHit(tile.containedMapObject as DamageableObject);
    }  

    protected override void InitValues()
    {
        base.InitValues();
        AttackCardInfo attackInfo = info as AttackCardInfo;
        damage = attackInfo.Damage;
    }
}
