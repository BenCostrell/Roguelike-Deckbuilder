using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile
{
    public readonly Coord coord;
    public readonly TileController controller;
    public List<Tile> neighbors;
    public int movementCost { get; private set; }
    public bool hovered { get; private set; }
    public Monster containedMonster;
    //public Card containedCard;
    public Chest containedChest;
    public DoorKey containedKey;
    public MapObject containedMapObject;
    public bool isExit;
    public bool locked;
    public readonly bool isRoom;
    public bool monsterMovementTarget;
    private Player player { get { return Services.GameManager.player; } }

    public Tile(Coord coord_, bool isExit_, bool isRoom_)
    {
        coord = coord_;
        GameObject obj = 
            GameObject.Instantiate(Services.Prefabs.Tile, Services.Main.transform);
        controller = obj.GetComponent<TileController>();
        controller.Init(this);
        movementCost = 1;
        isExit = isExit_;
        if (isExit) locked = true;
        isRoom = isRoom_;
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
        if ((containedMonster != null && !monsterMovement) || monsterMovementTarget || this == player.currentTile)
            return true;
        return false;
    }
}
