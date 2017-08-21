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
        Goblin
    }

    public void CreatePhysicalCard()
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Card, 
            Services.UIManager.handZone.transform);
        controller = obj.GetComponent<CardController>();
        controller.Init(this);
    }

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

    public void Reposition(Vector3 pos, bool changeBasPos)
    {
        controller.Reposition(pos, changeBasPos);
    }

    public void Disable()
    {
        playable = false;
    }

    public void Enable()
    {
        playable = true;
    }

    protected virtual void InitValues()
    {
        info = Services.CardConfig.GetCardOfType(cardType);
    }
}
