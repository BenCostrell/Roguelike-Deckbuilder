﻿using UnityEngine;
using System.Collections;

public abstract class MapObject : IPlaceable
{
    public enum ObjectType
    {
        SpikeTrap,
        Fountain,
        LightBrush,
        HeavyBrush,
        Chest,
        Sprout,
        Door,
        Apple,
        AppleTree
    }
    protected ObjectType objectType;
    public MapObjectInfo info { get; protected set; }
    public GameObject physicalObject { get; protected set; }
    protected Tile currentTile;
    protected AudioClip onStepAudio;
    protected Player player { get { return Services.GameManager.player; } }
    public SpriteMask mask { get; private set; }
    public SpriteRenderer maskSprite { get; private set; }
    public SpriteRenderer sr { get; private set; }
    protected Light light;
    //protected ParticleSystem ps;
    private float baseIntensity;
    private static int nextID_;
    private static int nextID
    {
        get
        {
            nextID_ += 1;
            return nextID_;
        }
    }
    private int id;

    protected virtual void InitValues()
    {
        info = Services.MapObjectConfig.GetMapObjectOfType(objectType);
        onStepAudio = info.OnStepAudio;
        id = nextID;
    }

    public virtual void CreatePhysicalObject(Tile tile)
    {
        physicalObject = GameObject.Instantiate(Services.Prefabs.MapObject);
        sr = physicalObject.GetComponent<SpriteRenderer>();
        sr.sprite = info.Sprites[0];
        physicalObject.transform.parent = Services.Main.transform;
        mask = physicalObject.GetComponentInChildren<SpriteMask>();
        maskSprite = mask.gameObject.GetComponent<SpriteRenderer>();
        mask.sprite = info.Sprites[0];
        mask.enabled = false;
        light = physicalObject.GetComponentInChildren<Light>();
        light.gameObject.SetActive(false);
        physicalObject.name = GetType().ToString();
        baseIntensity = light.intensity;
        light.color = info.LightColor;

        //ps = physicalObject.GetComponentInChildren<ParticleSystem>();
        //ParticleSystem.MainModule main = ps.main;
        //main.startColor = info.LightColor;
        PlaceOnTile(tile);
    }

    public virtual void PlaceOnTile(Tile tile)
    {
        if (currentTile != null && currentTile.containedMapObject == this)
            currentTile.containedMapObject = null;
        tile.containedMapObject = this;
        currentTile = tile;
        physicalObject.transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
    }

    public Tile GetCurrentTile()
    {
        return currentTile;
    }

    public GameObject GetPhysicalObject()
    {
        return physicalObject;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return sr;
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

    public virtual void RemoveThis(bool animate)
    {
        if(currentTile.containedMapObject == this) currentTile.containedMapObject = null;
        currentTile = null;
        player.ShowAvailableMoves();
        if(!animate) DestroyThis();
        else
        {
            DeathAnimation deathAnim = new DeathAnimation(this);
            deathAnim.Then(new ActionTask(DestroyThis));
            Services.Main.taskManager.AddTask(deathAnim);
        }
    }

    protected void DestroyThis()
    {
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

    public void AdjustLighting()
    {
        light.intensity = baseIntensity * 1 / Mathf.Max(1, Mathf.Pow(
            Vector2.Distance(currentTile.controller.transform.position,
            player.controller.transform.position), 1f)) 
            * (0.5f + (1 + Mathf.Sin(Time.time * 2)) / 4);
    }

    protected void SetSprite(int spriteIndex)
    {
        sr.sprite = info.Sprites[spriteIndex];
        mask.sprite = info.Sprites[spriteIndex];
    }
}
