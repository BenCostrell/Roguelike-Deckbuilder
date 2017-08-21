﻿using System.Collections;
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
    protected int movementSpeed;
    protected int attackRange;
    protected int attackDamage;

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
        GameObject.Destroy(controller.gameObject);
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
        attackDamage = info.AttackDamage;
        attackRange = info.AttackRange;
        movementSpeed = info.MovementSpeed;
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        controller.UpdateHealthUI();
        if (currentHealth == 0) Die();
    }

    void Die()
    {
        Services.MonsterManager.KillMonster(this);
        DestroyPhysicalMonster();
    }

    public bool IsPlayerInRange()
    {
        return Services.GameManager.player.currentTile.coord.Distance(currentTile.coord) 
            <= attackRange;
    }

    public virtual void AttackPlayer()
    {
        Services.GameManager.player.TakeDamage(attackDamage);
    }

    public virtual void Move()
    {
        List<Tile> shortestPathToPlayer =
            AStarSearch.ShortestPath(currentTile,
            Services.GameManager.player.currentTile, false);
        List<Tile> pathToMoveAlong = new List<Tile>();
        int movesLeft = movementSpeed;
        for (int i = shortestPathToPlayer.Count - 1; i >= 1; i--)
        {
            if (movesLeft == 0) break;
            pathToMoveAlong.Add(shortestPathToPlayer[i]);
            movesLeft -= 1;
        }
        if (pathToMoveAlong.Count > 0)
        {
            PlaceOnTile(pathToMoveAlong[pathToMoveAlong.Count - 1]);
        }
    }

}
