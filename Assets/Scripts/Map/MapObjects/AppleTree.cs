using System.Collections.Generic;
using UnityEngine;

public class AppleTree : GrowingPlant
{
    public AppleTree()
    {
        objectType = ObjectType.AppleTree;
        InitValues();
    }

    protected override void OnFullyGrown()
    {
        base.OnFullyGrown();
        List<Tile> adjacentTiles = AStarSearch.FindAllAvailableGoals(currentTile, 1, true);
        for (int i = adjacentTiles.Count - 1; i >= 0; i--)
        {
            Tile tile = adjacentTiles[i];
            if (tile.containedMapObject != null ||
                tile.containedMonster != null ||
                tile == player.currentTile)
                adjacentTiles.Remove(tile);
        }
        if(adjacentTiles.Count > 0)
        {
            Tile appleTile = adjacentTiles[Random.Range(0, adjacentTiles.Count)];
            Services.Main.taskManager.AddTask(new GrowObject(appleTile, 
                Services.MapObjectConfig.PlantGrowthTime, 
                Services.MapObjectConfig.CreateMapObjectOfType(ObjectType.Apple)));
        }
    }
}
