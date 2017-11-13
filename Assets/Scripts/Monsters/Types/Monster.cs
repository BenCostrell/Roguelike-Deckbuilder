using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : IDamageable {
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
    public bool attackedThisTurn { get; private set; }
    public Tile targetTile;
    protected Player player { get { return Services.GameManager.player; } }

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
        if (currentTile != null && currentTile.containedMonster == this)
            currentTile.RemoveMonsterFromTile();
        currentTile = tile;
        tile.PlaceMonsterOnTile(this);
        controller.PlaceOnTile(tile);
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

    public bool TakeDamage(int incomingDamage)
    {
        currentHealth = Mathf.Max(0, currentHealth - incomingDamage);
        controller.UpdateHealthUI();
        Services.UIManager.ShowUnitUI(this);
        if (currentHealth == 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public Tile GetCurrentTile()
    {
        return currentTile;
    }

    void Die()
    {
        Services.MonsterManager.KillMonster(this);
        currentTile.RemoveMonsterFromTile();
        currentTile = null;
        markedForDeath = true;
        player.ShowAvailableMoves();
        DeathAnimation deathAnim = new DeathAnimation(this);
        deathAnim.Then(new ActionTask(DestroyPhysicalMonster));
        Services.Main.taskManager.AddTask(deathAnim);
    }

    public bool IsPlayerInRange()
    {
        return player.currentTile.coord.Distance(currentTile.coord) <= attackRange ||
            (targetTile != null && player.currentTile.coord.Distance(targetTile.coord) <= attackRange);
    }

    public DamageableObject GetDamageableObjectWithinRange()
    {
        List<Tile> tilesWithinAttackRange;
        List<Tile> possiblePathToPlayer;
        if (targetTile != null)
        {
            tilesWithinAttackRange = AStarSearch.FindAllAvailableGoals(targetTile, attackRange, true);
            possiblePathToPlayer = AStarSearch.ShortestPath(targetTile, player.currentTile, false, false, true, true);
        }
        else
        {
            tilesWithinAttackRange = AStarSearch.FindAllAvailableGoals(currentTile, attackRange, true);
            possiblePathToPlayer = AStarSearch.ShortestPath(currentTile, player.currentTile, false, false, true, true);

        }
        DamageableObject targetObj = null;
        int closestDistance = 100000;
        for (int i = 0; i < tilesWithinAttackRange.Count; i++)
        {
            Tile tile = tilesWithinAttackRange[i];
            int dist = tile.coord.Distance(player.currentTile.coord);
            if (dist < closestDistance && tile.containedMapObject != null 
                && tile.containedMapObject is DamageableObject && possiblePathToPlayer.Contains(tile))
            {
                targetObj = tile.containedMapObject as DamageableObject;
                closestDistance = dist;

            }
        }
        return targetObj;
    }

    public void Refresh()
    {
        attackedThisTurn = false;
    }

    public virtual TaskTree AttackPlayer()
    {
        TaskTree attackTasks = new TaskTree(new AttackAnimation(controller.gameObject,
            player.controller.gameObject));
        attackTasks.Then(new ResolveHit(this, player));
        attackedThisTurn = true;
        return attackTasks;
    }

    public virtual TaskTree AttackMapObj(DamageableObject obj)
    {
        TaskTree attackTasks = new TaskTree(new AttackAnimation(controller.gameObject,
            obj.physicalObject));
        attackTasks.Then(new ResolveHit(this, obj));
        return attackTasks;
    }

    public virtual bool OnAttackHit(IDamageable target)
    {
        return target.TakeDamage(attackDamage);
    }

    public virtual TaskTree Move()
    {
        List<Tile> availableTiles =
            AStarSearch.FindAllAvailableGoals(currentTile, movementSpeed, false, false, true);
        //Debug.Log(availableTiles.Count + " available tiles to move to from " 
        //    + currentTile.coord.x + ","+ currentTile.coord.y);
        availableTiles.Add(currentTile);
        Tile closestTile = currentTile;
        int closestDistance = 1000000;
        if (currentTile.coord.Distance(player.currentTile.coord) > attackRange)
        {
            for (int i = 0; i < availableTiles.Count; i++)
            {
                Tile tile = availableTiles[i];
                if (tile.coord.Distance(player.currentTile.coord) <= attackRange)
                {
                    closestTile = tile;
                    closestDistance = tile.coord.Distance(player.currentTile.coord);
                    break;
                }
                List<Tile> shortestPathFromPlayer = AStarSearch.ShortestPath(
                    Services.GameManager.player.currentTile, tile, false, false, true, true);
                //Debug.Log("tile at " + tile.coord.x + "," + tile.coord.y + " is "
                //    + shortestPathFromPlayer.Count + " distance from player, compared to current closest tile at "
                //    + closestTile.coord.x + "," + closestTile.coord.y + " at " +
                //    closestDistance + " distance from player");
                if (shortestPathFromPlayer.Count < closestDistance)
                {
                    closestTile = tile;
                    closestDistance = shortestPathFromPlayer.Count;
                }
            }
        }
        List<Tile> shortestPathToTarget = new List<Tile>();
        if (closestTile != null && closestDistance != 0 && closestTile != currentTile)
        {
            shortestPathToTarget = AStarSearch.ShortestPath(currentTile, closestTile, false, false, true);
            targetTile = closestTile;
        }
        else
        {
            targetTile = currentTile;
        }
        return new TaskTree(new MoveObjectAlongPath(controller.gameObject, shortestPathToTarget));
        //List<Tile> shortestPathToPlayer = 
        //    AStarSearch.ShortestPath(currentTile, player.currentTile);
        //List<Tile> pathToMoveAlong = new List<Tile>();
        //int indexToGoUntil = shortestPathToPlayer[0] == player.currentTile ? 1 : 0;
        //int movesLeft = movementSpeed;
        //for (int i = shortestPathToPlayer.Count - 1; i >= indexToGoUntil; i--)
        //{
        //    if (movesLeft == 0) break;
        //    pathToMoveAlong.Add(shortestPathToPlayer[i]);
        //    movesLeft -= 1;
        //}
        //pathToMoveAlong.Reverse();
        //if (pathToMoveAlong.Count > 0)
        //{
        //    targetTile = pathToMoveAlong[0];
        //    return new TaskTree(new MoveObjectAlongPath(controller.gameObject,
        //        pathToMoveAlong));
        //}
        //else return new TaskTree(new EmptyTask());
    }

    public TaskTree MoveAwayFromPlayer(int numMovesAllowed)
    {
        List<Tile> availableTiles = 
            AStarSearch.FindAllAvailableGoals(currentTile, numMovesAllowed, false, false, true);
        //Debug.Log(availableTiles.Count + " available tiles to flee to");
        availableTiles.Add(currentTile);
        Tile farthestTile = currentTile;
        int farthestDistance = 0;
        for (int i = 0; i < availableTiles.Count; i++)
        {
            Tile tile = availableTiles[i];
            List<Tile> shortestPathFromPlayer = AStarSearch.ShortestPath(
                Services.GameManager.player.currentTile, tile, true);
            //Debug.Log("tile at " + tile.coord.x + "," + tile.coord.y + " is " 
            //    + shortestPathFromPlayer.Count + " distance from player, compared to current farthest tile at "
            //    + farthestTile.coord.x + "," + farthestTile.coord.y + " at " + 
            //    farthestDistance + " distance from player");
            if(shortestPathFromPlayer.Count > farthestDistance)
            {
                farthestTile = tile;
                farthestDistance = shortestPathFromPlayer.Count;
            }
        }
        List<Tile> shortestPathToTarget = new List<Tile>();
        if (farthestTile != null && farthestDistance != 0 && farthestTile != currentTile)
        {
            shortestPathToTarget = AStarSearch.ShortestPath(currentTile, farthestTile, false, false, true);
            targetTile = farthestTile;
        }
        else
        {
            targetTile = currentTile;
        }
        return new TaskTree(new MoveObjectAlongPath(controller.gameObject, shortestPathToTarget));
    }

}
