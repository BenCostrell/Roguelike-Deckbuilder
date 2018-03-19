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

    List<IDamageable> FindAdjacentTargets(IDamageable centerTarget)
    {
        List<IDamageable> adjTargets = new List<IDamageable>();
        Tile targetTile = centerTarget.GetCurrentTile();
        for (int i = 0; i < targetTile.neighbors.Count; i++)
        {
            Tile neighbor = targetTile.neighbors[i];
            if (neighbor.containedMonster != null)
                adjTargets.Add(neighbor.containedMonster);
            else if (neighbor.containedMapObject != null
                && neighbor.containedMapObject is DamageableObject)
                adjTargets.Add(neighbor.containedMapObject as DamageableObject);
        }
        return adjTargets;
    }

    protected override void OnHit(IDamageable target)
    {
        List<IDamageable> adjTargets = FindAdjacentTargets(target);
        for (int i = 0; i < adjTargets.Count; i++)
        {
            adjTargets[i].TakeDamage(damage, true);
        }
        base.OnHit(target);
    }
}
