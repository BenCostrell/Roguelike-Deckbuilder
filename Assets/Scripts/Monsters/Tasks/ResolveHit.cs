﻿using UnityEngine;
using System.Collections;

public class ResolveHit : Task
{
    private Monster monster;

    public ResolveHit(Monster monster_)
    {
        monster = monster_;
    }

    protected override void Init()
    {
        if(monster.controller == null)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        if (monster.OnAttackHit()) SetStatus(TaskStatus.Aborted);
        else SetStatus(TaskStatus.Success);
    }
}
