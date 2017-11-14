using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<MainTransitionData> {

    public TaskManager taskManager { get; private set; }
    public int levelNum { get; private set; }
    public DungeonDeck dungeonDeck { get; private set; }
    public List<Card> collection { get; private set; }
    private MainTransitionData data;
    private Player player { get { return Services.GameManager.player; } }

    private int lockId;

    private void Awake()
    {
        taskManager = new TaskManager();
    }

    internal override void Init()
    {
        InitializeMainServices();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
        Services.GameManager.currentCanvas = GetComponentInChildren<Canvas>();
    }

    internal override void OnEnter(MainTransitionData data_)
    {
        data = data_;
        levelNum = data.levelNum;
        //Services.MapManager.GenerateLevel(levelNum);
        Services.MapManager.GenerateLevelTest(levelNum);
        dungeonDeck = new DungeonDeck(data.dungeonDeck);
        dungeonDeck.Init();
        collection = data.collection;
        Services.SoundManager.SetMusicVolume(0.1f);
        player.Initialize(Services.MapManager.playerSpawnTile, data);
        GetComponentInChildren<CameraController>().InitCamera();
        TaskTree startTasks = new TaskTree(
            new ScrollMessageBanner(Services.UIManager.startBannerMessage));
        startTasks.Then(player.DrawCards(5));
        taskManager.AddTask(startTasks);
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
        int lockID = Services.UIManager.nextLockID;
        TaskTree endTurnTasks = player.OnTurnEnd();
        endTurnTasks
            .Then(new ScrollMessageBanner(Services.UIManager.dungeonTurnMessage))
            //.Then(Services.MonsterManager.MonstersMove())
            //.Then(Services.MonsterManager.MonstersAttack())
            .Then(dungeonDeck.TakeDungeonTurn())
            .Then(new ScrollMessageBanner(Services.UIManager.playerTurnMessage))
            .Then(player.OnTurnStart())
            .Then(new ParameterizedActionTask<int>(player.UnlockEverything, lockID));

        taskManager.AddTask(endTurnTasks);
        player.LockEverything(lockID);
    }

    public void ExitLevel()
    {
        lockId = Services.UIManager.nextLockID;
        player.LockEverything(lockId);
        Services.UIManager.ToggleLevelComplete(true);
    }

    public void CreateBonusChest()
    {
        Services.UIManager.ToggleLevelComplete(false);
        Chest chest = Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.Chest) as Chest;
        chest.tier = 1;
        chest.OnStep(player);
        Services.EventManager.Register<AcquisitionComplete>(GoToLevelTransitionScene);
    }

    void GoToLevelTransitionScene(AcquisitionComplete e)
    {
        Services.EventManager.Unregister<AcquisitionComplete>(GoToLevelTransitionScene);
        player.UnlockEverything(lockId);
        Services.SceneStackManager.Swap<LevelTransition>(new MainTransitionData(
            player.fullDeck,
            new List<Card>(),
            collection,
            player.currentHealth,
            player.maxHealth,
            levelNum + 1, false));
    }

    public void QueueAllButtonPress()
    {
        player.OnQueueButtonPressed();
    }

    public void DiscardQueuedCards()
    {
        player.DiscardQueuedCards();
    }
}
