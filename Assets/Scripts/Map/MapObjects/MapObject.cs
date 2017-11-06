using UnityEngine;
using System.Collections;

public abstract class MapObject 
{
    public enum ObjectType
    {
        SpikeTrap,
        Fountain
    }
    protected ObjectType objectType;
    protected MapObjectInfo info;
    protected GameObject physicalObject;
    protected Tile currentTile;
    protected AudioClip onStepAudio;
    protected Player player { get { return Services.GameManager.player; } }

    protected virtual void InitValues()
    {
        info = Services.MapObjectConfig.GetMapObjectOfType(objectType);
        onStepAudio = info.OnStepAudio;
    }

    public void PlaceOnTile(Tile tile)
    {
        physicalObject = new GameObject();
        SpriteRenderer sr = physicalObject.AddComponent<SpriteRenderer>();
        sr.sprite = info.Sprites[0];
        physicalObject.transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
        physicalObject.transform.parent = Services.Main.transform;
        sr.sortingOrder = 1;
        tile.containedMapObject = this;
        currentTile = tile;
    }

    public virtual bool OnStep(Player player)
    {
        if (onStepAudio != null) Services.SoundManager.CreateAndPlayAudio(onStepAudio);
        return false;
    }

    public virtual bool OnStep(Monster monster)
    {
        if (onStepAudio != null) Services.SoundManager.CreateAndPlayAudio(onStepAudio);
        return false;
    }

    protected void RemoveThis()
    {
        currentTile.containedMapObject = null;
        currentTile = null;
        GameObject.Destroy(physicalObject);
    }
}
