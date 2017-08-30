using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Services {
    public static GameManager GameManager { get; set; }
    public static EventManager EventManager { get; set; }
	public static TaskManager TaskManager { get; set; }
    public static PrefabDB Prefabs { get; set; }
    public static SceneStackManager<MainTransitionData> SceneStackManager { get; set; }
    public static InputManager InputManager { get; set; }
    public static Main Main { get; set; }
    public static MapManager MapManager { get; set; }
    public static UIManager UIManager { get; set; }
    public static CardConfig CardConfig { get; set; }
    public static MonsterConfig MonsterConfig { get; set; }
    public static MonsterManager MonsterManager { get; set; }
    public static SoundManager SoundManager { get; set; }
    public static AudioConfig AudioConfig { get; set; }
}
