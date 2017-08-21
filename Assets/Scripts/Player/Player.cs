using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public PlayerController controller { get; private set; }
    public Tile currentTile { get; private set; }
    private int movesAvailable_;
    public int movesAvailable
    {
        get { return movesAvailable_; }
        set
        {
            movesAvailable_ = value;
            Services.UIManager.UpdateMoveCounter(movesAvailable_);
        }
    }
    private List<Card> fullDeck
    {
        get
        {
            List<Card> deck = new List<Card>();
            deck.AddRange(remainingDeck);
            deck.AddRange(discardPile);
            deck.AddRange(hand);
            deck.AddRange(cardsInPlay);
            return deck;
        }
    }
    private List<Card> remainingDeck;
    private List<Card> hand;
    private List<Card> discardPile;
    private List<Card> cardsInPlay;
    public bool targeting;

    public void InitializeSprite(Tile tile)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Player, Services.Main.transform);
        controller = obj.GetComponent<PlayerController>();
        controller.Init(this);
        PlaceOnTile(tile);
        movesAvailable = 0;
    }

    public void InitializeDeck()
    {
        discardPile = new List<Card>();
        cardsInPlay = new List<Card>();
        remainingDeck = new List<Card>();
        hand = new List<Card>();
        foreach(CardInfo cardInfo in Services.CardConfig.StartingDeck)
        {
            Card card = Services.CardConfig.CreateCardOfType(cardInfo.CardType);
            remainingDeck.Add(card);
        }
    }

    public void DrawCards(int numCardsToDraw)
    {
        for (int i = 0; i < numCardsToDraw; i++)
        {
            DrawCard(GetRandomCardFromDeck());
        }
        Services.UIManager.UpdateDeckCounter(remainingDeck.Count);
    }

    Card GetRandomCardFromDeck()
    {
        if (remainingDeck.Count == 0)
        {
            remainingDeck = new List<Card>(discardPile);
            discardPile = new List<Card>();
        }
        int index = Random.Range(0, remainingDeck.Count);
        Card card = remainingDeck[index];
        remainingDeck.Remove(card);
        return card;
    }

    void DrawCard(Card card)
    {
        hand.Add(card);
        card.OnDraw();
        Services.UIManager.SortHand(hand);
    }

    void DiscardCardFromHand(Card card)
    {
        hand.Remove(card);
        DiscardCard(card);
        Services.UIManager.SortHand(hand);
    }

    void DiscardCardFromPlay(Card card)
    {
        RemoveCardFromPlay(card);
        DiscardCard(card);
        Services.UIManager.SortInPlayZone(cardsInPlay);
    }

    void DiscardCard(Card card)
    {
        discardPile.Add(card);
        card.OnDiscard();
    }

    public void PlaceOnTile(Tile tile)
    {
        controller.PlaceOnTile(tile);
        currentTile = tile;
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        if (path.Count <= movesAvailable) return true;
        else return false;
    }

    public void MoveToTile(Tile tile)
    {
        List<Tile> shortestPath = GetShortestPath(tile);
        if (CanMoveAlongPath(shortestPath))
        {
            movesAvailable -= MovementCost(shortestPath);
            PlaceOnTile(tile);
            if (tile.hovered) OnTileHover(tile);
        }
    }

    List<Tile> GetShortestPath(Tile tile)
    {
        List<Tile> shortestPathToTile =
            AStarSearch.ShortestPath(currentTile, tile, this, false);
        return shortestPathToTile;
    }

    int MovementCost(List<Tile> path)
    {
        int cost = 0;
        foreach(Tile tile in path)
        {
            cost += tile.movementCost;
        }
        return cost;
    }

    public void OnTileHover(Tile hoveredTile)
    {
        if (!targeting)
        {
            List<Tile> pathToTile = GetShortestPath(hoveredTile);
            if (CanMoveAlongPath(pathToTile))
            {
                controller.ShowPathArrow(pathToTile);
            }
            else HideArrow();
        }
    }

    public void HideArrow()
    {
        controller.HideArrow();
    }

    public void PlayCard(Card card)
    {
        Debug.Assert(hand.Contains(card));
        card.OnPlay();
        hand.Remove(card);
        PutCardInPlay(card);
    }

    void PutCardInPlay(Card card)
    {
        cardsInPlay.Add(card);
        card.controller.DisplayInPlay();
        Services.UIManager.SortInPlayZone(cardsInPlay);
    }

    void RemoveCardFromPlay(Card card)
    {
        cardsInPlay.Remove(card);
        Services.UIManager.SortInPlayZone(cardsInPlay);
    }

    public void OnTurnEnd()
    {
        if(hand.Count > 0)
        {
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                DiscardCardFromHand(hand[i]);
            }
        }
        if(cardsInPlay.Count > 0)
        {
            for (int i = cardsInPlay.Count - 1; i >= 0; i--)
            {
                DiscardCardFromPlay(cardsInPlay[i]);
            }
        }
        DrawCards(5);
        movesAvailable = 0;
    }

    public void DisableCardsWhileTargeting()
    {
        targeting = true;
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Disable();
        }
    }

    public void ReenableCardsWhenDoneTargeting()
    {
        targeting = false;
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Enable();
        }
    }
}
