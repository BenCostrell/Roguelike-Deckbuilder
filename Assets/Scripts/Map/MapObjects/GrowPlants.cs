using UnityEngine;
using System.Collections;

public class GrowPlants : Task
{
    private TaskManager subTaskManager;

    protected override void Init()
    {
        subTaskManager = new TaskManager();
        TaskTree growthTree = new TaskTree(new EmptyTask());
        for (int i = 0; i < Services.MapManager.growingPlants.Count; i++)
        {
            growthTree.AddChild(new ActionTask(Services.MapManager.growingPlants[i].Grow));
        }
        subTaskManager.AddTask(growthTree);
    }

    internal override void Update()
    {
        subTaskManager.Update();
        if (subTaskManager.tasksInProcessCount == 0) SetStatus(TaskStatus.Success);
    }

}
