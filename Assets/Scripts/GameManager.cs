using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{ 
    public GameObject sceneRoot;

    public int playerStartingHealth;

    public Player player { get; private set; }

    public Camera currentCamera;

    public Canvas currentCanvas;

    void Awake()
    {
        InitializeServices();
        Services.SoundManager.PlayMusic(Services.AudioConfig.MainTrack, 0f);
    }

    // Use this for initialization
    void Start()
    {
        InitializePlayer();
        Services.EventManager.Register<Reset>(Reset);
        Services.SceneStackManager.PushScene<TitleScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        Services.InputManager.GetInput();
        Services.TaskManager.Update();
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
    }

    void InitializeServices()
    {
        Services.GameManager = this;
        Services.EventManager = new EventManager();
        Services.TaskManager = new TaskManager();
        Services.Prefabs = Resources.Load<PrefabDB>("Prefabs/Prefabs");
        Services.SceneStackManager = new SceneStackManager<MainTransitionData>(sceneRoot, Services.Prefabs.Scenes);
        Services.InputManager = new InputManager();
        Services.CardConfig = Resources.Load<CardConfig>("Config/Cards/CardConfig");
        Services.MonsterConfig = Resources.Load<MonsterConfig>("Config/Monsters/MonsterConfig");
        Services.SoundManager = new SoundManager();
        Services.AudioConfig = Resources.Load<AudioConfig>("Config/Audio/AudioConfig");
        Services.MapObjectConfig = Resources.Load<MapObjectConfig>("Config/MapObjects/MapObjectConfig");
    }

    void InitializePlayer()
    {
        player = new Player();
    }

    void Reset(Reset e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}