using UnityEngine;
using System.Collections;

public class DamageableObject : MapObject, IDamageable
{
    public int currentHealth { get; protected set; }
    public readonly int startingHealth = 1;

    protected override void InitValues()
    {
        base.InitValues();
        currentHealth = startingHealth;
    }

    public virtual bool TakeDamage (int incomingDamage)
    {
        currentHealth = Mathf.Max(0, currentHealth - incomingDamage);
        //controller.UpdateHealthUI();
        Services.UIManager.ShowMapObjUI(this);
        if (currentHealth == 0)
        {
            RemoveThis();
            return true;
        }
        return false;
    }

    public override bool IsImpassable(bool ignoreDamageableObjects)
    {
        return !ignoreDamageableObjects;
    }

    public override int GetEstimatedMovementCost()
    {
        return 2;
    }

    public Tile GetCurrentTile()
    {
        return currentTile;
    }
}
