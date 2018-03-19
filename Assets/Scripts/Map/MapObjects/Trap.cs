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

    public override bool OnStep(Player player)
    {
        base.OnStep(player);
        player.TakeDamage(damage, true);
        RemoveThis(true);
        return true;
    }

    public override bool OnStep(Monster monster)
    {
        base.OnStep(monster);
        monster.TakeDamage(damage, true);
        RemoveThis(true);
        return true;
    }

}
