using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card {
    public CardController controller { get; private set; }
    public CardInfo info { get; protected set; }
    public bool playable { get; private set; }
    protected CardType cardType;
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
        Flamekin
    }
    public int tier { get; protected set; }
    public Sprite sprite { get; protected set; }
    public Tile currentTile { get; private set; }
    public bool deckViewMode;
    public Chest chest;

    public void CreatePhysicalCard(Transform tform)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Card, tform);
        controller = obj.GetComponent<CardController>();
        controller.Init(this);
    }

    //public void CreatePhysicalCard(Tile tile)
    //{
    //    CreatePhysicalCard(Services.Main.transform);
    //    currentTile = tile;
    //    tile.containedCard = this;
    //    controller.DisplayCardOnBoard();
    //    Reposition(tile.controller.transform.position + Services.CardConfig.CardOnBoardOffset, 
    //        true);
    //}

    public void DestroyPhysicalCard()
    {
        GameObject.Destroy(controller.gameObject);
        controller = null;
    }

    public virtual TaskTree OnDraw() {
        TaskTree onDrawTasks = new TaskTree(new DrawCardTask(this));
        return onDrawTasks;
    }

    public virtual void OnPlay() {
        playable = false;
        controller.DisplayInPlay();
    }

    public virtual void OnDiscard()
    {
        playable = false;
        DestroyPhysicalCard();
    }

    public virtual bool CanPlay()
    {
        return true;
    }

    public virtual void OnSelect() { }

    public virtual void OnUnselect() { }

    public void Reposition(Vector3 pos, bool changeBasPos)
    {
        controller.Reposition(pos, changeBasPos);
    }

    public void Disable()
    {
        playable = false;
        if (!deckViewMode && controller.transform.parent != Services.UIManager.inPlayZone)
        {
            controller.color = Color.gray;
        }
    }

    public void Enable()
    {
        playable = true;
        controller.color = Color.white;
    }

    protected virtual void InitValues()
    {
        info = Services.CardConfig.GetCardOfType(cardType);
        tier = info.Tier;
        sprite = info.Sprite;
    }

    //public void GetPickedUp()
    //{
    //    currentTile.containedCard = null;
    //    currentTile = null;
    //    Services.MapManager.cardsOnBoard.Remove(this);
    //}
}
