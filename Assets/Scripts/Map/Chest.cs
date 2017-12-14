using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MapObject {

    public int tier;
    private int numCards;
    private Vector3 leftmostCardPosition;
    private Vector3 cardSpacing;
    public List<Card> cardsInChest;
    public bool opened { get; private set; }
    private int lockID;
    private float scaleFactor;
    private ParticleSystem ps;

    public Chest()
    {
        objectType = ObjectType.Chest;
        InitValues();
    }

    protected override void InitValues()
    {
        base.InitValues();
        ChestInfo chestInfo = info as ChestInfo;
        scaleFactor = chestInfo.ScaleFactor;
        cardSpacing = chestInfo.CardSpacing;
        leftmostCardPosition = chestInfo.LeftmostCardPosition;
        numCards = chestInfo.NumCards;
    }

    public override void CreatePhysicalObject(Tile tile)
    {
        base.CreatePhysicalObject(tile);
        //light.gameObject.SetActive(true);
        //ps.Play();
        ps = GameObject.Instantiate(Services.Prefabs.SporeParticles, physicalObject.transform)
            .GetComponent<ParticleSystem>();
        ps.transform.localPosition = 0.1f * Vector3.back;
    }

    public override void RemoveThis(bool animate)
    {
        base.RemoveThis(animate);
        Services.MapManager.RemoveLitMapObject(this);
    }

    public override bool OnStep(Player player)
    {
        if(!opened) OpenChest();
        return base.OnStep(player);
    }

    void OpenChest()
    {
        Services.UIManager.ToggleChestArea(true);
        opened = true;
        cardsInChest = new List<Card>();
        List<Card> cardPool = Services.CardConfig.GetAllCardsOfTier(tier, false);
        for (int i = 0; i < numCards; i++)
        {
            Card cardToAdd = cardPool[Random.Range(0, cardPool.Count)];
            cardPool.Remove(cardToAdd);
            cardsInChest.Add(cardToAdd);
        }

        for(int i = 0; i < cardsInChest.Count; i++)
        {
            Card cardInChest = cardsInChest[i];
            cardInChest.CreatePhysicalCard(Services.UIManager.chestCardArea);
            CardController cardCont = cardsInChest[i].controller;
            cardCont.EnterChestMode();
            cardInChest.chest = this;
            cardInChest.Reposition(leftmostCardPosition + i * cardSpacing, true);
            cardCont.baseScale *= scaleFactor;
            cardCont.transform.localScale = cardCont.baseScale;
        }
        lockID = Services.UIManager.nextLockID;
        Services.GameManager.player.LockEverything(lockID);
    }

    public void OnCardPicked(Card card)
    {
        card.chest = null;
        foreach (Card otherCard in cardsInChest)
        {
            if (otherCard != card) otherCard.DestroyPhysicalCard();
        }

        Services.GameManager.player.AcquireCard(card);
        Services.UIManager.ToggleChestArea(false);
        Services.GameManager.player.UnlockEverything(lockID);
        if (currentTile != null)
        {
            RemoveThis(false);
            //sr.sprite = info.Sprites[1];
        }
    }
}
