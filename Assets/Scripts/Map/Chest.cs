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
            cardsInChest[i].CreatePhysicalCard(Services.GameManager.currentCamera.transform);
            cardsInChest[i].Reposition(leftmostCardPosition + i * cardSpacing, true);
            cardsInChest[i].chest = this;
            cardsInChest[i].controller.baseScale *= scaleFactor;
            cardsInChest[i].controller.transform.localScale = 
                cardsInChest[i].controller.baseScale;
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

        Services.GameManager.player.UnlockEverything(lockID);
        Destroy(gameObject);
    }
}
