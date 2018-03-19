using UnityEngine;
using System.Collections;

public interface IDamageable
{
    bool TakeDamage(int incomingDamage, bool fromPlayer);

    void Die(bool killedByPlayer);

    Tile GetCurrentTile();
}
