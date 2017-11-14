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
        Tile playersTargetTile = Services.MapManager.exitTile;
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
        if (potentialSpawnPoints.Count > 0)
        {
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
        return null;
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

    public void RefreshMonsters()
    {
        for (int i = 0; i < monsters.Count; i++)
        {
            monsters[i].Refresh();
        }
    }

    public TaskTree MonstersAttack()
    {
        TaskTree attackTree = new TaskTree(new EmptyTask());
        for (int i = 0; i < monsters.Count; i++)
        {
            Monster monster = monsters[i];
            if (monster.IsPlayerInRange() && !monster.attackedThisTurn)
            {
                attackTree.Then(monster.AttackPlayer());
                //Debug.Log("queuing attack at time " + Time.time);
            }
            else
            {
                DamageableObject obj = monster.GetDamageableObjectWithinRange();
                if (obj != null)
                {
                    attackTree.Then(monster.AttackMapObj(obj));
                }
            }
        }
        return attackTree;
    }

    public TaskTree MonstersMove()
    {
        TaskTree moveTree = new TaskTree(new EmptyTask());
        moveTree.AddChild(new WaitTask(Services.MonsterConfig.MaxMoveAnimDur));
        List<Monster> sortedMonsters = monsters.OrderBy(monster =>
            AStarSearch.ShortestPath(monster.currentTile, player.currentTile, true).Count).ToList();
        //Debug.Log("there are " + sortedMonsters.Count + " monsters at time " + Time.time);
        for (int i = 0; i < sortedMonsters.Count; i++)
        {
            Monster monster = sortedMonsters[i];
            if (!monster.IsPlayerInRange())
            {
                moveTree.AddChild(monster.Move());
                //Debug.Log("moving " + sortedMonsters[i].GetType() + "at " + sortedMonsters[i].currentTile.coord.x
                //    + " " + sortedMonsters[i].currentTile.coord.y);
            }
            else
            {
                monster.targetTile = monster.currentTile;
                monster.targetTile.monsterMovementTarget = true;
                moveTree.AddChild(new ActionTask(monster.ResetTargetTile));
            }
        }
        return moveTree;
    }
}
