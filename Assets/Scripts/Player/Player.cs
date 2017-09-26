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
    public List<Card> hand;
    private List<Card> discardPile;
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
    public bool hasKey { get; private set; }
    private bool movementLocked;
    private int movementLockID;

    public void Initialize(Tile tile, MainTransitionData data)
    {
        hasKey = false;
        InitializeSprite(tile);
        InitializeDeck(data.deck);
        Services.Main.taskManager.AddTask(DrawCards(5));
        maxHealth = data.maxHealth;
        currentHealth = maxHealth;
        controller.UpdateHealthUI();
        ForceUnlockEverything();
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
            cardDrawTasks.Then(DrawCard(GetRandomCardFromDeck()));
        }
        cardDrawTasks.Then(new ParameterizedActionTask<int>(UnlockEverything, lockID));
        return cardDrawTasks;
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
        cardsInFlux.Add(card);
        return card;
    }

    TaskTree DrawCard(Card card)
    {
        return card.OnDraw(remainingDeck.Count, discardPile.Count, true);
    }

    public void DiscardCardFromHand(Card card)
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
        Services.UIManager.UpdateDiscardCounter(discardPile.Count);
    }

    public void PlaceOnTile(Tile tile, bool end)
    {
        controller.PlaceOnTile(tile);
        currentTile = tile;
        ShowAvailableMoves();
        if (tile.containedMapObject != null)
        {
            tile.containedMapObject.OnStep(this);
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
    }

    public bool CanMoveAlongPath(List<Tile> path)
    {
        if (path.Count <= movesAvailable && !movementLocked) return true;
        else return false;
    }

    public void MoveToTile(Tile tile)
    {
        if (!tile.IsImpassable())
        {
            List<Tile> shortestPath = GetShortestPath(tile);
            if (CanMoveAlongPath(shortestPath))
            {
                movesAvailable -= MovementCost(shortestPath);
                HideAvailableMoves();
                Services.Main.taskManager.AddTask(
                    new MoveObjectAlongPath(controller.gameObject, shortestPath));
            }
        }
    }

    List<Tile> GetShortestPath(Tile tile)
    {
        List<Tile> shortestPathToTile =
            AStarSearch.ShortestPath(currentTile, tile, false);
        return shortestPathToTile;
    }

    public void ShowAvailableMoves()
    {
        List<Tile> availableTiles = 
            AStarSearch.FindAllAvailableGoals(currentTile, movesAvailable, false);
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
        if (!targeting && !moving && !hoveredTile.IsImpassable())
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

    public TaskTree PlayAll()
    {
        int lockID = Services.UIManager.nextLockID;
        TaskTree playAllTree = new TaskTree(new ParameterizedActionTask<int>(LockEverything,
            lockID));
        for (int i = hand.Count - 1; i >= 0; i--)
        {
            if (hand[i] is MovementCard)
            {
                MovementCard movementCard = hand[i] as MovementCard;
                playAllTree.Then(PlayCard(hand[i]));
                playAllTree.Then(new ParameterizedActionTask<int>(AddMovement, movementCard.range));
            }
        }
        playAllTree.Then(new ParameterizedActionTask<int>(UnlockEverything, lockID));
        return playAllTree;
    }

    void AddMovement(int moves)
    {
        movesAvailable += moves;
    }

    void RemoveCardFromPlay(Card card)
    {
        cardsInPlay.Remove(card);
        Services.UIManager.SortInPlayZone(cardsInPlay);
    }

    public TaskTree OnTurnEnd()
    {
        //if(hand.Count > 0)
        //{
        //    for (int i = hand.Count - 1; i >= 0; i--)
        //    {
        //        DiscardCardFromHand(hand[i]);
        //    }
        //}
        if(cardsInPlay.Count > 0)
        {
            for (int i = cardsInPlay.Count - 1; i >= 0; i--)
            {
                DiscardCardFromPlay(cardsInPlay[i]);
            }
        }
        movesAvailable = 0;

        int cardsToDraw = Mathf.Max(0, 5 - hand.Count);
        return DrawCards(cardsToDraw);
    }

    public void DisableCardsWhileTargeting(int lockID)
    {
        targeting = true;
        DisableHand(lockID);
    }

    public void DisableHand(int lockID)
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Disable();
        }
        Services.UIManager.DisablePlayAll(lockID);
    }

    public void ReenableCardsWhenDoneTargeting(int lockID)
    {
        targeting = false;
        ReenableHand(lockID);
    }

    public void ReenableHand(int lockID)
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Enable();
        }
        Services.UIManager.EnablePlayAll(lockID);
    }

    public bool TakeDamage(int damage)
    {
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
            maxHealth,
            Services.Main.levelNum,
            true));
    }

    public void AcquireCard(Card card)
    {
        discardPile.Add(card);
        Services.UIManager.UpdateDiscardCounter(discardPile.Count);
        Services.Main.taskManager.AddTask(new AcquireCardTask(card));
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
        if (!movementLocked)
        {
            movementLocked = true;
            movementLockID = lockID;
        }
    }

    public void UnlockMovement(int lockID)
    {
        if(movementLocked && lockID == movementLockID)
        {
            movementLocked = false;
        }
    }

    void ForceUnlockMovement()
    {
        movementLocked = false;
    }

    void ForceUnlockHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            hand[i].Enable();
        }
    }

    public void LockEverything(int lockID)
    {
        LockMovement(lockID);
        DisableHand(lockID);
        Services.UIManager.DisableEndTurn(lockID);
        Services.UIManager.DisablePlayAll(lockID);
    }

    public void UnlockEverything(int lockID)
    {
        UnlockMovement(lockID);
        ReenableHand(lockID);
        Services.UIManager.EnableEndTurn(lockID);
        Services.UIManager.EnablePlayAll(lockID);
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
}
