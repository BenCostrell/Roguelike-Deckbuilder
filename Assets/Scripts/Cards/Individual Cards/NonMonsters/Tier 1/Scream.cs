using UnityEngine;
using System.Collections;

public class Scream : Card
{
    public Scream()
    {
        cardType = CardType.Scream;
        InitValues();
    }

    public override void OnPlay()
    {
        base.OnPlay();
        int lockid = Services.UIManager.nextLockID;
        player.LockEverything(lockid);
        TaskTree moveTree = new TaskTree(new EmptyTask());
        moveTree.AddChild(new WaitTask(Services.MonsterConfig.MaxMoveAnimDur));
        for (int i = 0; i < Services.MonsterManager.monsters.Count; i++)
        {
            Monster monster = Services.MonsterManager.monsters[i];
            if (!monster.IsPlayerInRange())
                moveTree.AddChild(monster.MoveAwayFromPlayer(monster.movementSpeed));
        }
        moveTree.Then(new ParameterizedActionTask<int>(player.UnlockEverything, lockid));
        Services.Main.taskManager.AddTask(moveTree);
    }
}
