using UnityEngine;
using System.Collections;

public class ResolveHit : Task
{
    private Monster monster;
    private IDamageable target;

    public ResolveHit(Monster monster_, IDamageable target_)
    {
        monster = monster_;
        target = target_;
    }

    protected override void Init()
    {
        if (monster.controller == null)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        if (monster.OnAttackHit(target)) SetStatus(TaskStatus.Aborted);
        else SetStatus(TaskStatus.Success);
    }
}
