using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonDeck 
{
    private readonly List<Card> fullDeck;
    private List<Card> deck;
    public List<Card> playedCards;
    private List<Card> discardPile;
    public List<Card> cardsInFlux;
    private const int baseCardsPerRound = 5;
    private int cardsPerRound;

    public DungeonDeck(List<Card> baseDeck)
    {
        fullDeck = baseDeck;
        deck = fullDeck;
        playedCards = new List<Card>();
        discardPile = new List<Card>();
        cardsInFlux = new List<Card>();
        cardsPerRound = baseCardsPerRound;
        Services.UIManager.UpdateDungeonDeckCounter(deck.Count);
        Services.UIManager.UpdateDungeonDiscardCounter(0);
    }

    Card GetRandomCardFromDeck()
    {
        if (deck.Count == 0)
        {
            deck = new List<Card>(discardPile);
            discardPile = new List<Card>();
        }
        int index = Random.Range(0, deck.Count);
        Card card = deck[index];
        deck.Remove(card);
        cardsInFlux.Add(card);
        return card;
    }

    public TaskTree DrawCards(int numCardsToDraw)
    {
        TaskTree cardDrawTasks = new TaskTree(new EmptyTask());
        for (int i = 0; i < numCardsToDraw; i++)
        {
            cardDrawTasks.Then(DrawCard(GetRandomCardFromDeck()));
        }
        return cardDrawTasks;
    }

    TaskTree DrawCard(Card card)
    {
        return card.OnDraw(deck.Count, discardPile.Count, false);
    }

    public TaskTree TakeDungeonTurn()
    {
        for (int i = playedCards.Count-1; i >= 0; i--)
        {
            DiscardCard(playedCards[i]);
        }
        return DrawCards(cardsPerRound);
    }

    void DiscardCard(Card card)
    {
        discardPile.Add(card);
        playedCards.Remove(card);
        card.OnDiscard();
        Services.UIManager.UpdateDiscardCounter(discardPile.Count);
    }
}
