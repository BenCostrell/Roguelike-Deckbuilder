using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour {

    [HideInInspector]
    public int tier;
    [SerializeField]
    private int numCards;
    [SerializeField]
    private Vector3 leftmostCardPosition;
    [SerializeField]
    private Vector3 cardSpacing;
    public List<Card> cardsInChest;
    public bool opened { get; private set; }
    public Tile currentTile;
    private int lockID;
    [SerializeField]
    private float scaleFactor;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OpenChest()
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
        Destroy(gameObject);
    }
}
