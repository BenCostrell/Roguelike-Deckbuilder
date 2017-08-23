using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<MainTransitionData> {

    [SerializeField]
    private int spawnPointX;
    [SerializeField]
    private int spawnPointY;
    public Camera mainCamera { get; private set; }
    public TaskManager taskManager { get; private set; }

    private void Awake()
    {
        taskManager = new TaskManager();
    }

    internal override void Init()
    {
        InitializeMainServices();
        mainCamera = GetComponentInChildren<Camera>();

    }

    internal override void OnEnter(MainTransitionData data)
    {
        Services.MapManager.GenerateLevel();
        Services.GameManager.player.Initialize(
            Services.MapManager.map[spawnPointX, spawnPointY], data);
    }

    private void Update()
    {
        taskManager.Update();
    }

    void InitializeMainServices()
    {
        Services.Main = this;
        Services.MapManager = GetComponentInChildren<MapManager>();
        Services.UIManager = GetComponentInChildren<UIManager>();
        Services.MonsterManager = new MonsterManager();
    }

    public void EndTurn()
    {
        TaskTree endTurnTasks = new TaskTree(new EmptyTask());
        endTurnTasks
            .Then(Services.MonsterManager.MonstersMove())
            .Then(new ActionTask(Services.MonsterManager.MonstersAttack))
            .Then(new ActionTask(Services.GameManager.player.OnTurnEnd));

        taskManager.AddTask(endTurnTasks);
    }
}
