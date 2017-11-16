using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdvanceMonster : Task
{
    private TaskManager subTaskManager;
    private Monster monster;

    public AdvanceMonster(Monster monster_)
    {
        monster = monster_;
    }

    protected override void Init()
    {
        subTaskManager = new TaskManager();
        List<Tile> approachPath = monster.GetApproachPath();
        TaskTree subtasks = new TaskTree(new EmptyTask());
        if(approachPath.Count > 0)
        {
            subtasks.Then(new MoveObjectAlongPath(monster.controller.gameObject, 
                approachPath));
        }
        subtasks.Then(new ActionTask(CheckIfMonsterShouldAttack));
        subTaskManager.AddTask(subtasks);
    }

    internal override void Update()
    {
        subTaskManager.Update();
        if (subTaskManager.tasksInProcessCount == 0) SetStatus(TaskStatus.Success);
    }

    void CheckIfMonsterShouldAttack()
    {
        if (!monster.markedForDeath)
        {
            if (monster.IsPlayerInRange())
            {
                if (!monster.attackedThisTurn)
                {
                    subTaskManager.AddTask(monster.AttackPlayer());
                }
            }
            else
            {
                DamageableObject obj = monster.GetDamageableObjectWithinRange();
                if (obj != null)
                {
                    subTaskManager.AddTask(monster.AttackMapObj(obj));
                }
            }
        }
    }
}
