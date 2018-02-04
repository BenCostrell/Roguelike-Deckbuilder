using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card {
    public CardController controller { get; private set; }
    public CardInfo info { get; protected set; }
    public CardType cardType { get; protected set; }
    public enum CardType
    {
        Step,
        Punch,
        Goblin,
        Tome,
        Bow,
        Run,
        Sprint,
        Crossbow,
        Zombie,
        Slash,
        Bloodlust,
        HitAndRun,
        Potion,
        Bat,
        Orc,
        Flamekin,
        Blank,
        SpikeTrap,
        Shield,
        Tick,
        Swipe,
        Scream,
        Advance,
        GrowTheRanks,
        Overgrowth,
        SpreadSeeds,
        Leapfrog,
        Lignify,
        Telekinesis,
        MortalCoil,
        Swap,
        DoubleSnip,
        AppleTree
    }
    public int tier { get; protected set; }
    public Sprite sprite { get; protected set; }
    public Tile currentTile { get; private set; }
    public Chest chest;
    protected Player player { get { return Services.GameManager.player; } }

    public void CreatePhysicalCard(Transform tform)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Card, tform);
        controller = obj.GetComponent<CardController>();
        controller.Init(this);
    }

    public void DestroyPhysicalCard()
    {
        GameObject.Destroy(controller.gameObject);
        controller = null;
    }

    public virtual TaskTree OnDraw() {
        TaskTree onDrawTasks = new TaskTree(new EmptyTask());
        return onDrawTasks;
    }

    public virtual TaskTree OnPlay() {
        controller.DisplayInPlay();
        return new TaskTree(new EmptyTask());
    }

    public virtual TaskTree PostResolutionEffects()
    {
        return new TaskTree(new EmptyTask());
    }

    public virtual void OnDiscard()
    {
        controller.EnterDiscardMode();
    }

    public virtual bool CanPlay()
    {
        return true;
    }

    public virtual void OnSelect() { }

    public virtual void OnUnselect() { }

    public void Reposition(Vector3 pos, bool changeBasePos)
    {
        controller.Reposition(pos, changeBasePos);
    }

    public void Reposition(Vector3 pos, bool changeBasePos, bool front)
    {
        controller.Reposition(pos, changeBasePos, front);
    }

    public void Disable()
    {
        controller.Disable();
    }

    public void Enable()
    {
        controller.Enable();
    }

    protected virtual void InitValues()
    {
        info = Services.CardConfig.GetCardOfType(cardType);
        tier = info.Tier;
        sprite = info.Sprite;
    }

    public virtual Color GetCardFrameColor()
    {
        return Color.white;
    }
}
