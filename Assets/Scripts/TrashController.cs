using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashController : MonoBehaviour {
    public Card cardToTrash { get; private set; }
    private Vector3 cardPrevPos;
    private LevelTransition levelTransition;

    private void Awake()
    {
        levelTransition = GetComponentInParent<LevelTransition>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CardController>() != null && 
            !levelTransition.data.gameOver)
        {
            Card card = other.gameObject.GetComponent<CardController>().card;
            if (!card.info.IsMonster)
            {
                card.controller.overTrash = this;
                card.controller.color = Color.red;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CardController>())
        {
            Card card = other.gameObject.GetComponent<CardController>().card;
            card.controller.overTrash = null;
            card.controller.color = Color.white;
        }
    }

    public void PlaceCardInTrash(Card card)
    {
        if (cardToTrash != null)
        {
            cardToTrash.controller.DisplayInDeckViewMode();
        }
        GetComponent<SpriteRenderer>().enabled = false;
        cardToTrash = card;
        cardToTrash.Reposition(transform.position + (2 * Vector3.forward), false);
    }
}
