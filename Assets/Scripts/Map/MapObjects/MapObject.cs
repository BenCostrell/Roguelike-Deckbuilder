﻿using UnityEngine;
using System.Collections;

public abstract class MapObject 
{
    public enum ObjectType
    {
        SpikeTrap,
        Fountain,
        LightBrush,
        HeavyBrush
    }
    protected ObjectType objectType;
    public MapObjectInfo info { get; protected set; }
    public GameObject physicalObject { get; protected set; }
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
        player.ShowAvailableMoves();
        GameObject.Destroy(physicalObject);
    }

    public virtual bool IsImpassable(bool ignoreDamageableObjects)
    {
        return false;
    }

    public virtual int GetEstimatedMovementCost()
    {
        return 0;
    }
}
