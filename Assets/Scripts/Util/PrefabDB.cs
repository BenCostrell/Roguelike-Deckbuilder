using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Prefab DB")]
public class PrefabDB : ScriptableObject {
	[SerializeField]
	private GameObject player;
	public GameObject Player { get { return player; } }

    [SerializeField]
    private GameObject[] scenes;
    public GameObject[] Scenes { get { return scenes; } }

    [SerializeField]
    private GameObject tile;
    public GameObject Tile { get { return tile; } }

    [SerializeField]
    private GameObject card;
    public GameObject Card { get { return card; } }

    [SerializeField]
    private GameObject monster;
    public GameObject Monster { get { return monster; } }

    [SerializeField]
    private GameObject targetReticle;
    public GameObject TargetReticle { get { return targetReticle; } }

    [SerializeField]
    private GameObject key;
    public GameObject Key { get { return key; } }

    [SerializeField]
    private GameObject chest;
    public GameObject Chest { get { return chest; } }

    [SerializeField]
    private GameObject cardCounter;
    public GameObject CardCounter { get { return cardCounter; } }
}
