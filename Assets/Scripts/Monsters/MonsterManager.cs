using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager
{
    public List<Monster> monsters = new List<Monster>();

    public void SpawnMonster(Monster.MonsterType monsterType)
    {
        Monster monster = CreateMonsterOfType(monsterType);
        int originCol;
        if (!Services.GameManager.player.hasKey)
        {
            originCol = Mathf.Min(Services.MapManager.levelLength - 1,
            Services.GameManager.player.currentTile.coord.x +
            Services.MonsterConfig.MinDistFromPlayer);
        }
        else
        {
            originCol = Mathf.Max(0,
                Services.GameManager.player.currentTile.coord.x -
                Services.MonsterConfig.MinDistFromPlayer);
        }
        Tile originTile = Services.MapManager.map[originCol, 
            Services.GameManager.player.currentTile.coord.y];
        for (int i = Services.MonsterConfig.MinDistFromMonsters; i >= 1; i--)
        {
            Tile tile = Services.MapManager.GenerateValidTile(
            originTile,
            Services.MonsterConfig.SpawnRange,
            i,
            Services.MonsterConfig.MinDistFromCards);
            if (tile != null)
            {
                CreateMonster(monster, tile);
                break;
            }
        }
        
    }

    void CreateMonster(Monster monster, Tile tile)
    {
        monsters.Add(monster);
        monster.CreatePhysicalMonster(tile);
    }

    public void KillMonster(Monster monster)
    {
        monsters.Remove(monster);
    }

    Monster CreateMonsterOfType(Monster.MonsterType type)
    {
        switch (type)
        {
            case Monster.MonsterType.Goblin:
                return new Goblin();
            case Monster.MonsterType.Zombie:
                return new Zombie();
            default:
                return null;
        }
    }    

    public List<Monster> MonstersWithinRange(int range)
    {
        List<Monster> monstersInRange = new List<Monster>();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].currentTile.coord.Distance(
                Services.GameManager.player.currentTile.coord) <= range)
            {
                monstersInRange.Add(monsters[i]);
            }
        }
        return monstersInRange;
    }

    public TaskTree MonstersAttack()
    {
        TaskTree attackTree = new TaskTree(new EmptyTask());
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].IsPlayerInRange()) attackTree.Then(monsters[i].AttackPlayer());
        }
        return attackTree;
    }

    public TaskTree MonstersMove()
    {
        TaskTree moveTree = new TaskTree(new EmptyTask());
        moveTree.AddChild(new WaitTask(Services.MonsterConfig.MaxMoveAnimDur));
        for (int i = 0; i < monsters.Count; i++)
        {
            if (!monsters[i].IsPlayerInRange()) moveTree.AddChild(monsters[i].Move());
        }
        return moveTree;
    }
}
