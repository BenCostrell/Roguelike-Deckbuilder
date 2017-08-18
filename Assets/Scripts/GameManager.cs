using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public GameObject sceneRoot;

    public int numPlayers;
    public List<Player> players { get; private set; }
    public Vector3[] playerSpawns;

    void Awake()
    {
        InitializeServices();
    }

    // Use this for initialization
    void Start()
    {
        InitializePlayers();
        Services.EventManager.Register<Reset>(Reset);
        Services.SceneStackManager.PushScene<TitleScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        Services.InputManager.GetInput();
        Services.TaskManager.Update();
    }

    void InitializeServices()
    {
        Services.GameManager = this;
        Services.EventManager = new EventManager();
        Services.TaskManager = new TaskManager();
        Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/Prefabs");
        Services.SceneStackManager = new SceneStackManager<TransitionData>(sceneRoot, Services.Prefabs.Scenes);
        Services.InputManager = new InputManager();
    }

    void InitializePlayers()
    {
        players = new List<Player>();
        for (int i = 0; i < numPlayers; i++) players.Add(InitializePlayer(i + 1));
    }

    Player InitializePlayer(int playerNum)
    {
        GameObject playerObj = Instantiate(Services.Prefabs.Player, playerSpawns[playerNum - 1], Quaternion.identity);
        Player player = playerObj.GetComponent<Player>();
        player.playerNum = playerNum;
        return player;
    }

    void Reset(Reset e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}