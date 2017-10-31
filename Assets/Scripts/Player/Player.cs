﻿using System.Collections;
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
            ShowAvailableMoves();
        }
    }
    public List<Card> fullDeck
    {
        get
        {
            List<Card> deck = new List<Card>();
            deck.AddRange(remainingDeck);
            deck.AddRange(discardPile);
            deck.AddRange(hand);
            deck.AddRange(cardsInPlay);
            deck.AddRange(cardsInFlux);
            return deck;
        }
    }
    private List<Card> remainingDeck;
    public int deckCount { get { return remainingDeck.Count; } }
    public List<Card> hand;
    public List<Card> discardPile { get; private set; }
    public int discardCount { get { return discardPile.Count; } }
    public List<Card> cardsInPlay;
    public List<Card> cardsInFlux;
    public bool targeting;
    public bool selectingCards;
    public bool moving;
    private int maxHealth_;
    public int maxHealth
    {
        get { return maxHealth_; }
        private set
        {
            maxHealth_ = value;
            Services.UIManager.UpdatePlayerUI(currentHealth, maxHealth);
        }
    }
    private int currentHealth_;
    public int currentHealth
    {
        get { return currentHealth_; }
        private set
        {
            currentHealth_ = value;
            Services.UIManager.UpdatePlayerUI(currentHealth, maxHealth);
        }
    }
    private int shield_;
    private int shield
    {
        get { return shield_; }
        set
        {
            shield_ = value;
            Services.UIManager.SetShieldUI(shield_);
        }
    }
    public bool hasKey { get; private set; }
    private bool movementLocked;
    private List<int> movementLockIDs;
    private List<int> handLockIDs;
    private List<int> nonMovementLockIDs;
    private List<MovementCard> movementCardsSelected;
    public List<Card> cardsSelected;

    public Player()
    {
        hasKey = true;
        movementLockIDs = new List<int>();
        handLockIDs = new List<int>();
        nonMovementLockIDs = new List<int>();
        cardsSelected = new List<Card>();
        movementCardsSelected = new List<MovementCard>();
    }

    public void Initialize(Tile tile, MainTransitionData data)
    {
        InitializeSprite(tile);
        InitializeDeck(data.deck);
        ForceUnlockEverything();
        maxHealth = data.maxHealth;
        currentHealth = maxHealth;
        controller.UpdateHealthUI();
        shield = 0;
        Services.EventManager.Register<TileSelected>(OnTileSelected);
        Services.UIManager.UpdateDeckCounter();
        Services.UIManager.UpdateDiscardCounter();
    }

    void InitializeSprite(Tile tile)
    {
        GameObject obj = GameObject.Instantiate(Services.Prefabs.Player, Services.Main.transform);
        controller = obj.GetComponent<PlayerController>();
        controller.Init(this);
        PlaceOnTile(tile, false);
        movesAvailable = 0;
    }

    void InitializeDeck(List<Card> deck)
    {
        discardPile = new List<Card>();
        cardsInPlay = new List<Card>();
        remainingDeck = new List<Card>(deck);
        hand = new List<Card>();
        cardsInFlux = new List<Card>();
    }

    public TaskTree DrawCards(int numCardsToDraw)
    {
        int lockID = Services.UIManager.nextLockID;
        LockEverything(lockID);
        TaskTree cardDrawTasks = new TaskTree(new EmptyTask());
        for (int i = 0; i < numCardsToDraw; i++)
        {
            cardDrawTasks.Then(new DrawCardTask(true));
        }
        cardDrawTasks.Then(new ParameterizedActionTask<int>(UnlockEverything, lockID));
        return cardDrawTasks;
    }

    public Card GetRandomCardFromDeck()
    {
        if (remainingDeck.Count == 0)
        {
            remainingDeck = new List<Card>(discardPile);
            discardPile = new List<Card>();
        }
        int index = Random.Range(0, remainingDeck.Count);
        Card card = remainingDeck[index];
        remainingDeck.Remove(card);
        cardsInFlux.Add(card);
        return card;
    }

    public void DiscardQueuedCards()
    {
        float staggerTime = 0;
        float totalStaggerTime = (movementCardsSelected.Count - 1) 
            * Services.CardConfig.DiscardAnimStaggerTime;
        TaskTree discardTasks = new TaskTree(new EmptyTask());
        discardTasks.AddChild(new WaitTask(totalStaggerTime));
        for (int i = 0; i < movementCardsSelected.Count; i++)
        {
            MovementCard card = movementCardsSelected[i];
            discardTasks.AddChild(DiscardQueuedCard(card, staggerTime));
            staggerTime += Services.CardConfig.DiscardAnimStaggerTime;
        }
        movementCardsSelected.Clear();
        Services.Main.taskManager.AddTask(discardTasks);
        Services.UIManager.SortHand(hand);
    }

    public TaskTree DiscardQueuedCard(Card card, float timeOffset)
    {
        card.OnUnselect();
        hand.Remove(card);
        return DiscardCard(card, timeOffset);
    }

    public TaskTree DiscardCardFromHand(Card card, float timeOffset)
    {
        hand.Remove(card);
        Services.UIManager.SortHand(hand);
        return DiscardCard(card, timeOffset);
    }

    TaskTree DiscardCardFromPlay(Card card, float timeOffset)
    {
        Services.UIManager.SortInPlayZone(cardsInPlay);
        return DiscardCard(card, timeOffset);
    }

    TaskTree DiscardCard(Card card, float timeOffset)
    {
        discardPile.Add(card);
        TaskTree discardCardTasks = new TaskTree(new WaitTask(timeOffset));
        discardCardTasks.Then(new ActionTask(card.OnDiscard));
        discardCardTasks.Then(new DiscardCard(card));
        discardCardTasks.Then(new ActionTask(Services.UIManager.UpdateDiscardCounter));
        return discardCardTasks;
    }

    public bool PlaceOnTile(Tile tile, bool end)
    {
        bool stopped = false;
        controller.PlaceOnTile(tile);
        currentTile = tile;
        ShowAvailableMoves();
        if (tile.containedMapObject != null)
        {
            stopped = tile.containedMapObject.OnStep(this);
        }
        if (end)
        {
            if (currentTile.containedChest != null && !currentTile.containedChest.opened)
            {
                currentTile.containedChest.OpenChest();
            }
            if (currentTile.containedKey != null)
            {
                PickUpKey(currentTile);
            }
        }
        return stopped;
    }

    void OnTileSelected(TileSelected e)
    {
        if (!e.tile.IsImpassable() && !movementLocked)
        {
            List<Tile> shortestPath = GetShortestPath(e.tile);
            if (CanMoveAlongPath(shortestPath)) MoveToTile(shortestPath);
        }
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        return ((path.Count <= movesAvailable && path.Count > 0 && !movementLocked));
    }

    public bool CanMoveToTile(Tile tile)
    {
        List<Tile> shortestPath = GetShortestPath(tile);
        return CanMoveAlongPath(shortestPath);
    }

    public void MoveToTile(List<Tile> path)
    {
        Services.EventManager.Fire(new MovementInitiated());
        movesAvailable -= MovementCost(path);
        HideAvailableMoves();
        Services.Main.taskManager.AddTask(
            new MoveObjectAlongPath(controller.gameObject, path));
    }

    List<Tile> GetShortestPath(Tile tile)
    {
        List<Tile> shortestPathToTile =
            AStarSearch.ShortestPath(currentTile, tile, false, true);
        return shortestPathToTile;
    }

    public void ShowAvailableMoves()
    {
        List<Tile> availableTiles = 
            AStarSearch.FindAllAvailableGoals(currentTile, movesAvailable, false, true);
        foreach(Tile tile in Services.MapManager.mapDict.Values)
        {
            if (availableTiles.Contains(tile)) tile.controller.ShowAsAvailable();
            else tile.controller.ShowAsUnavailable();
        }
    }

    public void HideAvailableMoves()
    {
        foreach(Tile tile in Services.MapManager.mapDict.Values)
        {
            tile.controller.ShowAsUnavailable();
        }
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
        if ((!targeting || movementCardsSelected.Count > 0) && !moving && !hoveredTile.IsImpassable())
        {
            List<Tile> pathToTile = GetShortestPath(hoveredTile);
            if (CanMoveAlongPath(pathToTile))
            {
                controller.ShowPathArrow(pathToTile);
            }
            else HideArrow();
        }
        else HideArrow();

    }

    public void HideArrow()
    {
        if (controller != null) controller.HideArrow();
    }

    public Task PlayCard(Card card)
    {
        Debug.Assert(hand.Contains(card));
        return new PlayCardTask(card);
    }

    public void SelectMovementCard(MovementCard card)
    {
        movementCardsSelected.Add(card);
        Services.UIManager.SetQueueButtonStatus(CheckQueueButtonStatus());
    }

    public void UnselectMovementCard(MovementCard card)
    {
        movementCardsSelected.Remove(card);
        Services.UIManager.SetQueueButtonStatus(CheckQueueButtonStatus());
    }

    bool CheckQueueButtonStatus()
    {
        int numMovementCardsInHand = 0;
        for (int i = 0; i < hand.Count; i++)
        {
            if (hand[i] is MovementCard) numMovementCardsInHand += 1;
        }
        if (movementCardsSelected.Count == numMovementCardsInHand) return false;
        else return true;
    }

    public void OnQueueButtonPressed()
    {
        if (CheckQueueButtonStatus()) QueueAll();
        else UnqueueAll();
        Services.UIManager.SetQueueButtonStatus(CheckQueueButtonStatus());
    }

    public void QueueAll()
    {
        for (int i = hand.Count - 1; i >= 0; i--)
        {
            if (hand[i] is MovementCard)
            {
                MovementCard movementCard = hand[i] as MovementCard;
                if (!movementCardsSelected.Contains(movementCard))
                    movementCard.controller.SelectMovementCard();
            }
        }
    }

    public void UnqueueAll()
    {
        for (int i = movementCardsSelected.Count - 1; i >= 0; i--)
        {
            movementCardsSelected[i].controller.UnselectMovementCard();
        }
    }

    void AddMovement(int moves)
    {
        movesAvailable += moves;
    }

    public TaskTree OnTurnEnd()
    {
        TaskTree turnEndTasks = new TaskTree(new EmptyTask());
        float totalStaggerTime = Services.CardConfig.DiscardAnimStaggerTime 
            * (cardsInPlay.Count - 1);
        float staggerTime = 0;
        turnEndTasks.AddChild(new WaitTask(totalStaggerTime));
        if (cardsInPlay.Count > 0)
        {
            for (int i = 0; i < cardsInPlay.Count; i++)
            {
                turnEndTasks.AddChild(DiscardCardFromPlay(cardsInPlay[i], staggerTime));
                staggerTime += Services.CardConfig.DiscardAnimStaggerTime;
            }
            cardsInPlay.Clear();
        }
        UnqueueAll();
        Services.UIManager.SetQueueButtonStatus(true);
        movesAvailable = 0;
        return turnEndTasks;
    }

    public TaskTree OnTurnStart()
    {
        int cardsToDraw = Mathf.Max(0, 5 - hand.Count);

        TaskTree startTurnTaskTree = 
            new TaskTree(new ParameterizedActionTask<int>(SetShield, 0));
        startTurnTaskTree.Then(DrawCards(cardsToDraw));
        return startTurnTaskTree;
    }

    public void DisableCardsWhileTargeting(int lockID)
    {
        targeting = true;
        LockEverything(lockID);
    }

    public void DisableHand(int lockID)
    {
        handLockIDs.Add(lockID);
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Disable();
        }
    }

    public void ReenableCardsWhenDoneTargeting(int lockID)
    {
        targeting = false;
        UnlockEverything(lockID);
    }

    public void ReenableHand(int lockID)
    {
        if (handLockIDs.Contains(lockID))
        {
            handLockIDs.Remove(lockID);
            if (handLockIDs.Count == 0)
            {
                for (int i = 0; i < hand.Count; i++)
                {
                    hand[i].Enable();
                }
            }
        }
    }

    public bool TakeDamage(int incomingDamage)
    {
        int damage = incomingDamage;
        if (shield > 0)
        {
            int shieldDamage = Mathf.Min(damage, shield);
            shield -= shieldDamage;
            damage -= shieldDamage;
        }
        currentHealth = Mathf.Max(0, currentHealth - damage);
        controller.UpdateHealthUI();
        if (currentHealth == 0)
        {
            Die();
            return true;
        }
        return false;
    }

    void Die()
    {
        Services.SceneStackManager.Swap<LevelTransition>(new MainTransitionData(
            fullDeck,
            new List<Card>(),
            Services.Main.collection,
            maxHealth,
            Services.Main.levelNum,
            true));
    }

    public void AcquireCard(Card card)
    {
        discardPile.Add(card);
        Services.UIManager.UpdateDiscardCounter();
    }

    void PickUpKey(Tile tile)
    {
        DoorKey key = tile.containedKey;
        tile.containedKey = null;
        hasKey = true;
        GameObject.Destroy(key.gameObject);
        Services.UIManager.UpdatePlayerUI(currentHealth, maxHealth);
    }

    public void LockMovement(int lockID)
    {
        movementLocked = true;
        movementLockIDs.Add(lockID);
    }

    public void UnlockMovement(int lockID)
    {
        if(movementLockIDs.Contains(lockID))
        {
            movementLockIDs.Remove(lockID);
            if (movementLockIDs.Count == 0) movementLocked = false;
        }
    }

    void ForceUnlockMovement()
    {
        movementLocked = false;
        movementLockIDs.Clear();
    }

    void ForceUnlockHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Enable();
        }
        nonMovementLockIDs.Clear();
        handLockIDs.Clear();
    }

    public void LockEverything(int lockID)
    {
        LockMovement(lockID);
        DisableHand(lockID);
        Services.UIManager.DisableEndTurn(lockID);
        Services.UIManager.DisablePlayAll(lockID);
        Services.UIManager.DisableDiscardQueued(lockID);
    }

    public void UnlockEverything(int lockID)
    {
        UnlockMovement(lockID);
        ReenableHand(lockID);
        Services.UIManager.EnableEndTurn(lockID);
        Services.UIManager.EnablePlayAll(lockID);
        Services.UIManager.EnableDiscardQueued(lockID);
    }

    void ForceUnlockEverything()
    {
        ForceUnlockMovement();
        ForceUnlockHand();
        Services.UIManager.ForceUnlockEndTurn();
        Services.UIManager.ForceUnlockPlayAll();
    }

    public void Heal(int amountHealed)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amountHealed);
    }

    public void DisableNonMovementCards(int lockID)
    {
        nonMovementLockIDs.Add(lockID);
        for (int i = 0; i < hand.Count; i++)
        {
            if (!(hand[i] is MovementCard))
            {
                hand[i].Disable();
            }
        }

    }

    public void EnableNonMovementCards(int lockID)
    {
        nonMovementLockIDs.Remove(lockID);
        if(nonMovementLockIDs.Count == 0)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                hand[i].Enable();
            }
        }
    }

    public void GainShield(int shieldAmount)
    {
        shield += shieldAmount;
    }

    void SetShield(int shieldValue)
    {
        shield = shieldValue;
    }

    public void EnterCardSelectionMode()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].controller.UnselectedForCard();
        }
    }

    public void ExitCardSelectionMode()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].controller.Enable();
        }
    }
}
