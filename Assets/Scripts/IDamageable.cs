using UnityEngine;
using System.Collections;

public interface IDamageable
{
    bool TakeDamage(int incomingDamage);

    void Die();

    Tile GetCurrentTile();
}
