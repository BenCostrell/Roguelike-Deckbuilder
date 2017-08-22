﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private GameObject moveCounter;
    public GameObject deckCounter;
    public GameObject deckZone;
    public GameObject inPlayZone;
    public GameObject handZone;
    public GameObject discardCounter;
    public GameObject discardZone;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void UpdateMoveCounter(int movesAvailable)
    {
        moveCounter.GetComponent<Text>().text = "Moves Available: " + movesAvailable;
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.GetComponent<Text>().text = cardsInDeck.ToString();
    }

    public void UpdateDiscardCounter(int cardsInDiscard)
    {
        discardCounter.GetComponent<Text>().text = cardsInDiscard.ToString();
    }

    public void SortHand(List<Card> cardsInHand)
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].Reposition(GetCardHandPosition(i), true);
        }
    }

    public Vector3 GetCardHandPosition(int handCountNum)
    {
        return Services.CardConfig.HandCardSpacing * handCountNum;
    }

    public Vector3 GetInPlayCardPosition(int inPlayZoneCountNum)
    {
        return Services.CardConfig.InPlaySpacing * inPlayZoneCountNum;
    }

    public void SortInPlayZone(List<Card> cardsInPlay)
    {
        for (int i = 0; i < cardsInPlay.Count; i++)
        {
            cardsInPlay[i].Reposition(GetInPlayCardPosition(i), true);
        }
    }
}
