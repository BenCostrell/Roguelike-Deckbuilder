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
        Services.GameManager.player.OnTileHover(this);
    }

    public void OnHoverExit()
    {
        hovered = false;
    }

    public void OnSelect()
    {
        if (this != Services.GameManager.player.currentTile &&
            ((CardController.currentlySelectedCards.Count == 0) ||
            Services.GameManager.player.movementCardsSelected.Count != 0))
        {
            Services.GameManager.player.MoveToTile(this);
        }
        Services.EventManager.Fire(new TileSelected(this));
    }

    public bool IsImpassable()
    {
        if (containedMonster != null || monsterMovementTarget) return true;
        return false;
    }
}
