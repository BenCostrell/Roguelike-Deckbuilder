using UnityEngine;
using System.Collections;

public abstract class MapObject 
{
    public enum ObjectType
    {
        SpikeTrap
    }
    protected ObjectType objectType;
    protected MapObjectInfo info;
    protected GameObject physicalObject;
    protected Tile currentTile;

    protected virtual void InitValues()
    {
        info = Services.MapObjectConfig.GetMapObjectOfType(objectType);
    }

    public void PlaceOnTile(Tile tile)
    {
        physicalObject = new GameObject();
        SpriteRenderer sr = physicalObject.AddComponent<SpriteRenderer>();
        sr.sprite = info.Sprite;
        physicalObject.transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
        physicalObject.transform.parent = Services.Main.transform;
        sr.sortingOrder = 1;
        tile.containedMapObject = this;
        currentTile = tile;
    }

    public virtual void OnStep(Player player) { }

    public virtual void OnStep(Monster monster) { }

    protected void RemoveThis()
    {
        currentTile.containedMapObject = null;
        currentTile = null;
        GameObject.Destroy(physicalObject);
    }
}
