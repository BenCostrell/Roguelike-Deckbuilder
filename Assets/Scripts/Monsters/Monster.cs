using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster {
    public enum MonsterType
    {
        Goblin
    }
    protected MonsterType monsterType;
    protected MonsterController controller;
    public MonsterInfo info { get; protected set; }
    public Tile currentTile { get; protected set; }

    public void CreatePhysicalMonster(Tile tile)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Monster, Services.Main.transform);
        controller = obj.GetComponent<MonsterController>();
        controller.Init(this);
        PlaceOnTile(tile);
    }

    protected void PlaceOnTile(Tile tile)
    {
        currentTile = tile;
        controller.PlaceOnTile(tile);
    }

}
