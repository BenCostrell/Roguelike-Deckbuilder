using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : Scene<TransitionData> {

    [SerializeField]
    private int spawnPointX;
    [SerializeField]
    private int spawnPointY;
    public Camera mainCamera { get; private set; }

    internal override void Init()
    {
        InitializeMainServices();
        mainCamera = GetComponentInChildren<Camera>();
        Services.MapManager.GenerateLevel();
        Services.GameManager.player.InitializeSprite(
            Services.MapManager.map[spawnPointX, spawnPointY]);
        Services.GameManager.player.InitializeDeck();
        Services.GameManager.player.DrawCards(5);
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
    }
}
