﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card {
    private CardController controller;
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
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Card, Services.Main.transform);
        controller = obj.GetComponent<CardController>();
        controller.Init(this);
    }

    public void DestroyPhysicalCard()
    {
        GameObject.Destroy(controller.gameObject);
        controller = null;
    }

    public virtual void OnDraw() {
        playable = true;
        CreatePhysicalCard();
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