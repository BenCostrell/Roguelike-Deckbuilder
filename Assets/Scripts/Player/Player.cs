using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    private PlayerController controller;
    public Tile currentTile { get; private set; }
    private int movesAvailable_;
    public int movesAvailable
    {
        get { return movesAvailable_; }
        set
        {
            movesAvailable_ = value;
            Services.UIManager.UpdateMoveCounter(movesAvailable_);
            if (hand != null && hand.Count == 0 && movesAvailable_ == 0) Services.Main.EndTurn();
        }
    }
    private List<Card> fullDeck;
    private List<Card> remainingDeck;
    private List<Card> hand_;
    private List<Card> hand
    {
        get { return hand_; }
        set
        {
            hand_ = value;
            if (hand_.Count == 0 && movesAvailable == 0) Services.Main.EndTurn();
        }
    }
    private List<Card> discardPile;

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
        fullDeck = new List<Card>();
        discardPile = new List<Card>();
        foreach(CardInfo cardInfo in Services.CardConfig.StartingDeck)
        {
            Card card = Services.CardConfig.CreateCardOfType(cardInfo.Type);
            fullDeck.Add(card);
        }

        remainingDeck = new List<Card>(fullDeck);
    }

    public void DrawCards(int numCardsToDraw)
    {
        for (int i = 0; i < numCardsToDraw; i++)
        {
            AddCardToHand(DrawRandomCardFromDeck());
        }
    }

    Card DrawRandomCardFromDeck()
    {
        if (remainingDeck.Count == 0)
        {
            remainingDeck = new List<Card>(fullDeck);
            discardPile = new List<Card>();
        }
        int index = Random.Range(0, remainingDeck.Count);
        Card card = remainingDeck[index];
        remainingDeck.Remove(card);
        return card;
    }

    void AddCardToHand(Card card)
    {
        if (hand != null) hand.Add(card);
        else hand = new List<Card>() { card };
        card.CreatePhysicalCard();
        SortHand();
    }

    void RemoveCardFromHand(Card card)
    {
        hand.Remove(card);
        discardPile.Add(card);
        card.DestroyPhysicalCard();
        SortHand();
    }

    void SortHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Reposition(Services.CardConfig.HandStartPos + 
                i * Vector3.right * Services.CardConfig.HandCardSpacing);
        }
    }

    public void PlaceOnTile(Tile tile)
    {
        controller.PlaceOnTile(tile);
        currentTile = tile;
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        if (path.Count <= movesAvailable)
        {
            return true;
        }
        else
        {
            return false;
        }
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
        List<Tile> pathToTile = GetShortestPath(hoveredTile);
        if (CanMoveAlongPath(pathToTile))
        {
            controller.ShowPathArrow(pathToTile);
        }
        else HideArrow();
    }

    public void HideArrow()
    {
        controller.HideArrow();
    }

    public void PlayCard(Card card)
    {
        if (hand.Contains(card))
        {
            card.OnPlay();
            hand.Remove(card);
            discardPile.Add(card);
        }
    }

    public void OnTurnEnd()
    {
        if(hand.Count > 0)
        {
            for (int i = hand.Count - 1; i >= 0; i--)
            {
                RemoveCardFromHand(hand[i]);
            }
        }
        DrawCards(5);
    }
}
