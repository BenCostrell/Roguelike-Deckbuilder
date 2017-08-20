using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterManager
{
    private List<Monster> monsters = new List<Monster>();

    public void SpawnMonster(Monster.MonsterType monsterType)
    {
        Monster monster = CreateMonsterOfType(monsterType);
        monsters.Add(monster);
        int minCol = Services.MonsterConfig.MinDistFromPlayer 
            + Services.GameManager.player.currentTile.coord.x;
        Tile tile = Services.MapManager.GenerateValidTile(
            Services.MonsterConfig.MinDistFromMonsters,
            Services.MonsterConfig.MinDistFromCards,
            minCol,
            minCol + Services.MonsterConfig.SpawnRange);
        if (tile != null) monster.CreatePhysicalMonster(tile);
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
}
