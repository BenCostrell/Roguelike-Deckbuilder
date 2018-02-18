using UnityEngine;
using System.Collections;

public abstract class DamageableObject : MapObject, IDamageable
{
    public int currentHealth { get; protected set; }
    public int startingHealth { get; protected set; }

    protected override void InitValues()
    {
        base.InitValues();
        startingHealth = info.Health;
        currentHealth = startingHealth;
    }

    public virtual bool TakeDamage (int incomingDamage)
    {
        currentHealth = Mathf.Max(0, currentHealth - incomingDamage);
        //controller.UpdateHealthUI();
        Services.UIManager.ShowMapObjUI(this);
        if (currentHealth == 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public virtual void Die()
    {
        RemoveThis(true);
    }

    public override bool IsImpassable(bool ignoreDamageableObjects)
    {
        return !ignoreDamageableObjects;
    }

    public override int GetEstimatedMovementCost()
    {
        return 2;
    }
}
