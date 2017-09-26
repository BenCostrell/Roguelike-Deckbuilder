using UnityEngine;
using System.Collections;
using System;

public abstract class Trap : MapObject
{
    protected int damage;

    protected override void InitValues()
    {
        base.InitValues();
        TrapInfo trapInfo = info as TrapInfo;
        damage = trapInfo.Damage;
    }

    public override void OnStep(Player player)
    {
        player.TakeDamage(damage);
        RemoveThis();
    }

    public override void OnStep(Monster monster)
    {
        monster.TakeDamage(damage);
        RemoveThis();
    }

}
