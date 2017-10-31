using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReshuffleDeck : Task
{
    private readonly bool playerDeck;
    private float totalDuration;
    private float indivDuration;
    private float staggerTime;
    private float timeElapsed;
    private Vector2 startPos;
    private Vector2 endPos;
    private List<Card> cards;
    private bool[] cardsCreated;
    private Transform parent;

    public ReshuffleDeck(bool playerDeck_)
    {
        playerDeck = playerDeck_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        if (playerDeck)
        {
            cards = Services.GameManager.player.discardPile;
            startPos = Services.UIManager.discardZone
                .GetComponent<RectTransform>().anchoredPosition;
            endPos = Services.UIManager.deckZone
                .GetComponent<RectTransform>().anchoredPosition;
            parent = Services.UIManager.discardZone.transform.parent;
        }
        else
        {
            cards = Services.Main.dungeonDeck.discardPile;
            startPos = Services.UIManager.dungeonDiscardZone
                .GetComponent<RectTransform>().anchoredPosition;
            endPos = Services.UIManager.dungeonDeckZone
                .GetComponent<RectTransform>().anchoredPosition;
            parent = Services.UIManager.dungeonDiscardZone.transform.parent;
        }
        indivDuration = Services.CardConfig.ReshuffleAnimDur;
        staggerTime = Services.CardConfig.ReshuffleAnimStagger;
        totalDuration = indivDuration + ((cards.Count - 1) * staggerTime);
        cardsCreated = new bool[cards.Count];
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        for (int i = 0; i < cards.Count; i++)
        {
            if(timeElapsed >= staggerTime * i && 
                timeElapsed <= (staggerTime * i) + indivDuration)
            {
                if (!cardsCreated[i])
                {
                    cards[i].CreatePhysicalCard(parent);
                    cards[i].controller.EnterReshuffleState();
                    cardsCreated[i] = true;
                }
                cards[i].Reposition(Vector3.Lerp(startPos, endPos,
                    Easing.QuadEaseOut((timeElapsed - (i * staggerTime)) / indivDuration)),
                    false, true);
            }
        }


        if (timeElapsed >= totalDuration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].DestroyPhysicalCard();
        }
    }

}
