using UnityEngine;
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
    public SpriteMask mask { get; private set; }
    public SpriteRenderer maskSprite { get; private set; }
    public SpriteRenderer sr { get; private set; }

    protected virtual void InitValues()
    {
        info = Services.MapObjectConfig.GetMapObjectOfType(objectType);
        onStepAudio = info.OnStepAudio;
    }

    public void PlaceOnTile(Tile tile)
    {
        physicalObject = GameObject.Instantiate(Services.Prefabs.MapObject);
        sr = physicalObject.GetComponent<SpriteRenderer>();
        sr.sprite = info.Sprites[0];
        physicalObject.transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
        physicalObject.transform.parent = Services.Main.transform;
        mask = physicalObject.GetComponentInChildren<SpriteMask>();
        maskSprite = mask.gameObject.GetComponent<SpriteRenderer>();
        mask.sprite = info.Sprites[0];
        mask.enabled = false;
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

    protected void RemoveThis(bool animate)
    {
        currentTile.containedMapObject = null;
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
}
