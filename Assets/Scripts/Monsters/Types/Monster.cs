using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster {
    public enum MonsterType
    {
        Goblin,
        Zombie,
        Bat,
        Orc,
        Flamekin
    }
    protected MonsterType monsterType;
    public MonsterController controller { get; protected set; }
    public MonsterInfo info { get; protected set; }
    public Tile currentTile { get; protected set; }
    public int currentHealth { get; protected set; }
    public int maxHealth { get; protected set; }
    public int movementSpeed { get; protected set; }
    public int attackRange { get; protected set; }
    public int attackDamage { get; protected set; }
    public bool markedForDeath { get; protected set; }
    public Tile targetTile;

    public void CreatePhysicalMonster(Tile tile)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Monster, Services.Main.transform);
        controller = obj.GetComponent<MonsterController>();
        controller.Init(this);
        controller.GetComponent<SpriteRenderer>().sprite = info.Sprite;
        PlaceOnTile(tile);
        controller.UpdateHealthUI();
    }

    void DestroyPhysicalMonster()
    {
        GameObject.Destroy(controller.gameObject);
    }

    public void PlaceOnTile(Tile tile)
    {
        if (currentTile != null) currentTile.containedMonster = null;
        currentTile = tile;
        tile.containedMonster = this;
        controller.PlaceOnTile(tile);
        if(tile.containedMapObject != null)
        {
            tile.containedMapObject.OnStep(this);
        }
    }

    protected void InitValues()
    {
        info = Services.MonsterConfig.GetMonsterOfType(monsterType);
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
        Services.UIManager.ShowUnitUI(this);
        if (currentHealth == 0) Die();
    }

    void Die()
    {
        Services.MonsterManager.KillMonster(this);
        currentTile.containedMonster = null;
        currentTile = null;
        markedForDeath = true;
        Services.GameManager.player.ShowAvailableMoves();
        DeathAnimation deathAnim = new DeathAnimation(this);
        deathAnim.Then(new ActionTask(DestroyPhysicalMonster));
        Services.Main.taskManager.AddTask(deathAnim);
    }

    public bool IsPlayerInRange()
    {
        return Services.GameManager.player.currentTile.coord.Distance(currentTile.coord) <= attackRange ||
            (targetTile != null && Services.GameManager.player.currentTile.coord.Distance(targetTile.coord) <= attackRange);
    }

    public virtual TaskTree AttackPlayer()
    {
        TaskTree attackTasks = new TaskTree(new AttackAnimation(controller.gameObject,
            Services.GameManager.player.controller.gameObject));
        attackTasks.Then(new ResolveHit(this));
        return attackTasks;
    }

    public virtual bool OnAttackHit()
    {
        return Services.GameManager.player.TakeDamage(attackDamage);
    }

    public virtual TaskTree Move()
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
        pathToMoveAlong.Reverse();
        if (pathToMoveAlong.Count > 0)
        {
            targetTile = pathToMoveAlong[0];
            return new TaskTree(new MoveObjectAlongPath(controller.gameObject,
                pathToMoveAlong));
        }
        else return new TaskTree(new EmptyTask());
    }

}
