using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MonsterManager
{
    public List<Monster> monsters = new List<Monster>();
    private Player player { get { return Services.GameManager.player; } }

    public Monster SpawnMonster(Monster.MonsterType monsterType)
    {
        Monster monster = CreateMonsterOfType(monsterType);
        Tile playersTargetTile = Services.MapManager.keyTile;
        //if (!Services.GameManager.player.hasKey)
        //{
        //    playersTargetTile = Services.MapManager.keyTile;
        //}
        //else
        //{
        //    playersTargetTile = Services.MapManager.playerSpawnTile;
        //}
        List<Tile> playersPathToTarget = AStarSearch.ShortestPath(
            player.currentTile,
            playersTargetTile, true);
        playersPathToTarget.Reverse();
        Tile spawnCenterPoint;
        if(playersPathToTarget.Count < Services.MonsterConfig.MinDistFromMonsters)
        {
            spawnCenterPoint = playersPathToTarget[playersPathToTarget.Count - 1];
        }
        else
        {
            spawnCenterPoint = 
                playersPathToTarget[Services.MonsterConfig.MinDistFromMonsters - 1];
        }
        List<Tile> potentialSpawnPoints = AStarSearch.FindAllAvailableGoals(spawnCenterPoint,
            Services.MonsterConfig.SpawnRange, true);
        for (int i = potentialSpawnPoints.Count - 1; i >= 0; i--)
        {
            if (potentialSpawnPoints[i].containedMonster != null ||
                player.currentTile == potentialSpawnPoints[i] ||
                potentialSpawnPoints[i].containedMapObject != null)
                potentialSpawnPoints.Remove(potentialSpawnPoints[i]);
        }
        Tile spawnPoint = potentialSpawnPoints[Random.Range(0, potentialSpawnPoints.Count)];
        if (spawnPoint != null)
        {
            CreateMonster(monster, spawnPoint);
        }
        else
        {
            monster = null;
        }
        return monster;
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
            case Monster.MonsterType.Bat:
                return new Bat();
            case Monster.MonsterType.Orc:
                return new Orc();
            case Monster.MonsterType.Flamekin:
                return new Flamekin();
            default:
                return null;
        }
    }    

    public List<Monster> MonstersWithinRange(int range)
    {
        List<Monster> monstersInRange = new List<Monster>();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i].currentTile.coord.Distance(player.currentTile.coord) <= range)
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
        List<Monster> sortedMonsters = monsters.OrderBy(monster =>
        AStarSearch.ShortestPath(monster.currentTile, player.currentTile, true).Count).ToList();
        for (int i = 0; i < sortedMonsters.Count; i++)
        {
            if (!sortedMonsters[i].IsPlayerInRange())
                moveTree.AddChild(sortedMonsters[i].Move());
        }
        return moveTree;
    }
}
