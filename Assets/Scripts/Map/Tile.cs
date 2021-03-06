﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile
{
    public readonly Coord coord;
    public readonly TileController controller;
    public List<Tile> neighbors;
    public int movementCost { get; private set; }
    public bool hovered { get; private set; }
    public Monster containedMonster { get; private set; }
    public MapObject containedMapObject;
    public bool isExit;
    public bool monsterMovementTarget;
    private Player player { get { return Services.GameManager.player; } }

    public Tile(Coord coord_, bool isExit_)
    {
        coord = coord_;
        GameObject obj = 
            GameObject.Instantiate(Services.Prefabs.Tile, Services.Main.transform);
        controller = obj.GetComponent<TileController>();
        controller.Init(this);
        movementCost = 1;
        isExit = isExit_;
    }

    public void SetSprite(Sprite sprite, Quaternion rot)
    {
        controller.GetComponent<SpriteRenderer>().sprite = sprite;
        controller.transform.localRotation = rot;
    }

    public void OnHoverEnter()
    {
        hovered = true;
        player.OnTileHover(this);
    }

    public void OnHoverExit()
    {
        hovered = false;
    }

    public void OnSelect()
    {
        Services.EventManager.Fire(new TileSelected(this));
    }

    public bool IsImpassable()
    {
        return IsImpassable(false);
    }

    public bool IsImpassable(bool monsterMovement)
    {
        return IsImpassable(monsterMovement, false);
    }

    public bool IsImpassable(bool monsterMovement, bool ignoreDamageableObjects)
    {
        if ((containedMonster != null && (!monsterMovement || containedMonster.IsPlayerInRange())) || 
            monsterMovementTarget || this == player.currentTile || 
            (containedMapObject != null && containedMapObject.IsImpassable(ignoreDamageableObjects)))
            return true;
        return false;
    }

    public void PlaceMonsterOnTile(Monster monster)
    {
        containedMonster = monster;
    }

    public void RemoveMonsterFromTile()
    {
        containedMonster = null;
    }

    public Tile GetTileBehind(Tile originTile)
    {
        Coord direction = coord.Subtract(originTile.coord);
        Coord tileBehindCoord = coord.Add(direction);
        if (!Services.MapManager.ContainedInMap(tileBehindCoord)) return null;
        Tile tileBehind = Services.MapManager.mapGrid[tileBehindCoord.x, tileBehindCoord.y];
        return tileBehind;
    }
}
