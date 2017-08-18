using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card {
    private CardController controller;
    public CardInfo info { get; protected set; }
    protected Type type;
    public enum Type
    {
        Step
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

    public virtual void OnPlay() {
        DestroyPhysicalCard();
    }

    public void Reposition(Vector3 pos)
    {
        controller.Reposition(pos);
    }
}
