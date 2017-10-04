using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<MainTransitionData> {

    public Camera mainCamera { get; private set; }
    public TaskManager taskManager { get; private set; }
    public int levelNum { get; private set; }
    public DungeonDeck dungeonDeck { get; private set; }
    public List<Card> collection { get; private set; }

    private void Awake()
    {
        taskManager = new TaskManager();
    }

    internal override void Init()
    {
        InitializeMainServices();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
    }

    internal override void OnEnter(MainTransitionData data)
    {
        levelNum = data.levelNum;
        Services.MapManager.GenerateLevel(levelNum);
        dungeonDeck = new DungeonDeck(data.dungeonDeck);
        collection = data.collection;
        Services.GameManager.player.Initialize(Services.MapManager.playerSpawnTile, data);
        Services.SoundManager.SetMusicVolume(0.1f);
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
        Services.GameManager.player.OnTurnEnd();
        int lockID = Services.UIManager.nextLockID;
        Services.GameManager.player.LockEverything(lockID);
        TaskTree endTurnTasks = new TaskTree(new EmptyTask());
        endTurnTasks
            .Then(Services.MonsterManager.MonstersMove())
            .Then(Services.MonsterManager.MonstersAttack())
            .Then(dungeonDeck.TakeDungeonTurn())
            .Then(Services.GameManager.player.OnTurnStart())
            .Then(new ParameterizedActionTask<int>(
                Services.GameManager.player.UnlockEverything, lockID));

        taskManager.AddTask(endTurnTasks);
    }

    public void ExitLevel()
    {
        Services.SceneStackManager.Swap<LevelTransition>(new MainTransitionData(
            Services.GameManager.player.fullDeck,
            new List<Card>(),
            collection,
            Services.GameManager.player.maxHealth + 1,
            levelNum + 1, false));
    }

    public void PlayAll()
    {
        Services.GameManager.player.PlayAll();
    }
}
