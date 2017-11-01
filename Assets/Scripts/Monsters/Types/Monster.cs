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
        if (currentTile != null) currentTile.containedMonster = null;
        currentTile = tile;
        tile.containedMonster = this;
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

    public virtual TaskTree AttackPlayer()
    {
        TaskTree attackTasks = new TaskTree(new AttackAnimation(controller.gameObject,
            player.controller.gameObject));
        attackTasks.Then(new ResolveHit(this));
        return attackTasks;
    }

    public virtual bool OnAttackHit()
    {
        return player.TakeDamage(attackDamage);
    }

    public virtual TaskTree Move()
    {
        List<Tile> availableTiles =
            AStarSearch.FindAllAvailableGoals(currentTile, movementSpeed);
        //Debug.Log(availableTiles.Count + " available tiles to move to from " 
        //    + currentTile.coord.x + ","+ currentTile.coord.y);
        availableTiles.Add(currentTile);
        Tile closestTile = currentTile;
        int closestDistance = 1000000;
        for (int i = 0; i < availableTiles.Count; i++)
        {
            Tile tile = availableTiles[i];
            List<Tile> shortestPathFromPlayer = AStarSearch.ShortestPath(
                Services.GameManager.player.currentTile, tile, true);
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
        if (closestTile != null && closestDistance != 0 && closestTile != currentTile)
        {
            List<Tile> shortestPathToTarget =
                AStarSearch.ShortestPath(currentTile, closestTile);
            targetTile = closestTile;
            return new TaskTree(new MoveObjectAlongPath(controller.gameObject,
                shortestPathToTarget));
        }
        else return new TaskTree(new EmptyTask());
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
            AStarSearch.FindAllAvailableGoals(currentTile, numMovesAllowed);
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
        if (farthestTile != null && farthestDistance != 0 && farthestTile != currentTile)
        {
            List<Tile> shortestPathToTarget =
                AStarSearch.ShortestPath(currentTile, farthestTile);
            targetTile = farthestTile;
            return new TaskTree(new MoveObjectAlongPath(controller.gameObject,
                shortestPathToTarget));
        }
        else return new TaskTree(new EmptyTask());
    }

}
