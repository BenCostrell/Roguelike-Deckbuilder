using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        List<Monster> sortedMonsters = 
            Services.MonsterManager.monsters.OrderByDescending(monster =>
            AStarSearch.ShortestPath(monster.currentTile, player.currentTile, true).Count)
            .ToList();
        for (int i = 0; i < sortedMonsters.Count; i++)
        {
            Monster monster = sortedMonsters[i];
            moveTree.AddChild(monster.MoveAwayFromPlayer(monster.movementSpeed));
        }
        moveTree.Then(new ParameterizedActionTask<int>(player.UnlockEverything, lockid));
        moveTree.Then(new ActionTask(player.ShowAvailableMoves));
        Services.Main.taskManager.AddTask(moveTree);
    }
}
