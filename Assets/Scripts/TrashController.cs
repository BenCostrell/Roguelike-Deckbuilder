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

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.GetComponent<CardController>() != null && 
            !levelTransition.data.gameOver)
        {
            Card card = other.gameObject.GetComponent<CardController>().card;
            if (!card.info.IsMonster)
            {
                card.controller.sr.color = Color.red;
                if (!Services.GameManager.mouseDown && card != cardToTrash)
                {
                    if (cardToTrash != null)
                    {
                        cardToTrash.controller.transform.position = cardPrevPos;
                    }
                    cardToTrash = card;
                    cardPrevPos = card.controller.transform.position;
                    cardToTrash.controller.transform.position = transform.position;
                }
            }
        }
    }
}
