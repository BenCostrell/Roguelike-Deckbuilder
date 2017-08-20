using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster {
    public enum MonsterType
    {
        Goblin
    }
    protected MonsterType monsterType;
    protected MonsterController controller;
    public MonsterInfo info { get; protected set; }
    public Tile currentTile { get; protected set; }
    public int currentHealth { get; protected set; }
    public int maxHealth { get; protected set; }

    public void CreatePhysicalMonster(Tile tile)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Monster, Services.Main.transform);
        controller = obj.GetComponent<MonsterController>();
        controller.Init(this);
        PlaceOnTile(tile);
        controller.UpdateHealthUI();
    }

    void DestroyPhysicalMonster()
    {
        currentTile.containedMonster = null;
        currentTile = null;
        GameObject.Destroy(controller);
    }

    protected void PlaceOnTile(Tile tile)
    {
        if (currentTile != null) currentTile.containedMonster = null;
        currentTile = tile;
        tile.containedMonster = this;
        controller.PlaceOnTile(tile);
    }

    protected void InitValues()
    {
        maxHealth = info.StartingHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        controller.UpdateHealthUI();
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Services.MonsterManager.KillMonster(this);
        DestroyPhysicalMonster();
    }

}
