using UnityEngine;
using System.Collections;

public abstract class MonsterCard : DungeonCard
{
    public Monster.MonsterType monsterToSpawn { get; protected set; }

    public override TaskTree DungeonOnPlay()
    {
        //return new TaskTree(new SpawnMonster(this));
        Sprout sprout = null;
        for (int i = 1; i < Services.MonsterConfig.MaxSpawnRadius; i++)
        {
            sprout = Services.MapManager.GetLiveSprout(i);
            if (sprout != null) break;
        }
        TaskTree spawnMonsterTasks = new TaskTree(new EmptyTask());
        if (sprout != null)
        {
            spawnMonsterTasks.AddChild(new GrowObject(sprout.GetCurrentTile(),
                Services.MapObjectConfig.PlantGrowthTime, 
                SpawnMonster(sprout.GetCurrentTile()), true));
            spawnMonsterTasks.AddChild(new FadeOutSprout(sprout));
        }
        return spawnMonsterTasks;
    }

    public Monster SpawnMonster(Tile tile)
    {
        return Services.MonsterManager.SpawnMonster(monsterToSpawn, tile);
    }

    protected override float GetPriority()
    {
        return 20f;
    }
}
