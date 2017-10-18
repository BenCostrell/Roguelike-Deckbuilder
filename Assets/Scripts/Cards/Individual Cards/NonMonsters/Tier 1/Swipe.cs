using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Swipe : AttackCard
{
    public Swipe()
    {
        cardType = CardType.Swipe;
        InitValues();
    }

    List<Monster> FindAdjacentMonsters(Monster centerMonster)
    {
        List<Monster> adjMonsters = new List<Monster>();
        Tile monsterTile = centerMonster.currentTile;
        for (int i = 0; i < monsterTile.neighbors.Count; i++)
        {
            if (monsterTile.neighbors[i].containedMonster != null)
                adjMonsters.Add(monsterTile.neighbors[i].containedMonster);
        }
        return adjMonsters;
    }

    protected override void OnHit(Monster monster)
    {
        List<Monster> adjMonsters = FindAdjacentMonsters(monster);
        for (int i = 0; i < adjMonsters.Count; i++)
        {
            adjMonsters[i].TakeDamage(damage);
        }
        base.OnHit(monster);
    }
}
