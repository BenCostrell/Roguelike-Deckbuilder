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
    public Card containedCard;

    public Tile(Coord coord_)
    {
        coord = coord_;
        GameObject obj = 
            GameObject.Instantiate(Services.Prefabs.Tile, Services.Main.transform);
        controller = obj.GetComponent<TileController>();
        controller.Init(this);
        movementCost = 1;
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
        if (Services.GameManager.player.targeting)
        {
            Services.EventManager.Fire(new TileSelected(this));
        }
        else Services.GameManager.player.MoveToTile(this);
    }

    public bool IsImpassable()
    {
        if (containedMonster != null) return true;
        return false;
    }
}
