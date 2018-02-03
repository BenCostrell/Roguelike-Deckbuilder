using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonDeck 
{
    private readonly List<Card> fullDeck;
    private List<Card> deck;
    public int deckCount { get { return deck.Count; } }
    public List<Card> playedCards;
    public List<DungeonCard> hand { get; private set; }
    public List<Card> discardPile { get; private set; }
    public int discardCount { get { return discardPile.Count; } }
    public List<Card> cardsInFlux;
    private const int baseCardsPerRound = 3;
    private int queuedExtraCards_;
    private int queuedExtraCards
    {
        get { return queuedExtraCards_; }
        set
        {
            queuedExtraCards_ = value;
            Services.UIManager.UpdateCardsNextRound(cardsPerRound);
        }
    }
    private int cardsPerRound { get { return baseCardsPerRound + queuedExtraCards; } }
    private int dungeonTimerCount;
    private int dungeonTimerThreshold;
    public bool spedUp { get; private set; }
    private float forestTimeScale;
    private bool forestTurn;

    public DungeonDeck(List<Card> baseDeck)
    {
        fullDeck = baseDeck;
        deck = new List<Card>(fullDeck);
        playedCards = new List<Card>();
        discardPile = new List<Card>();
        cardsInFlux = new List<Card>();
        hand = new List<DungeonCard>();
        dungeonTimerCount = 0;
        forestTimeScale = 1;
        dungeonTimerThreshold = Services.MonsterConfig.DungeonTimerThreshold;
        //UpdateDungeonTimer();
    }

    public void Init()
    {
        Services.UIManager.UpdateDungeonDeckCounter();
        Services.UIManager.UpdateDungeonDiscardCounter();
    }

    public Card GetRandomCardFromDeck()
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
            cardDrawTasks.Then(new DrawCardTask(false));
        }
        cardDrawTasks.Then(new ActionTask(PutCardsInHandMode));
        return cardDrawTasks;
    }

    void PutCardsInHandMode()
    {
        for (int i = 0; i < playedCards.Count; i++)
        {
            playedCards[i].controller.EnterDungonHandMode();
        }
    }

    public void QueueExtraCard()
    {
        queuedExtraCards += 1;
    }

    void ResetQueuedCards()
    {
        queuedExtraCards = 0;
    }

    public TaskTree TakeDungeonTurn()
    {
        TaskTree turnTasks = new TaskTree(new EmptyTask());
        turnTasks.Then(new ActionTask(OnForestTurnStart));
        turnTasks.Then(new ActionTask(Services.MonsterManager.RefreshMonsters));
        turnTasks.Then(DrawCards(cardsPerRound));
        turnTasks.Then(new ActionTask(ResetQueuedCards));
        turnTasks.Then(new PerformActions());
        turnTasks.Then(new DiscardPlayedCards());
        turnTasks.Then(new ActionTask(OnForestTurnEnd));
        return turnTasks;
    }

    void OnForestTurnStart()
    {
        Time.timeScale = forestTimeScale;
        forestTurn = true;
    }

    void OnForestTurnEnd()
    {
        Time.timeScale = 1;
        forestTurn = false;
    }

    public TaskTree DiscardCard(Card card, float timeOffset)
    {
        discardPile.Add(card);
        playedCards.Remove(card);
        TaskTree discardTasks = new TaskTree(new WaitTask(timeOffset));
        discardTasks.Then(new ActionTask(card.OnDiscard));
        discardTasks.Then(new DiscardCard(card));
        discardTasks.Then(new ActionTask(Services.UIManager.UpdateDungeonDiscardCounter));
        return discardTasks;
    }

    //void UpdateDungeonTimer()
    //{
    //    Services.UIManager.UpdateDungeonTimer((float)dungeonTimerCount / dungeonTimerThreshold);
    //}

    public TaskTree AlterDungeonTimerCount(int amt)
    {
        return new TaskTree(new AlterDungeonTimerTask(this, amt));
    }

    public TaskTree AddMonster(Card card)
    {
        return new TaskTree(new AlterDungeonTimerTask(this, card));
    }

    public bool ActuallyAlterDungeonTimerCount(int amt)
    {
        bool addMonster = false;
        dungeonTimerCount += amt;
        if (dungeonTimerCount >= dungeonTimerThreshold)
        {
            dungeonTimerCount -= dungeonTimerThreshold;
            addMonster = true;
        }
        //UpdateDungeonTimer();
        return addMonster;
    }

    public MonsterCard GetNewMonsterCard()
    {
        List<Card.CardType> monsterTypes = new List<Card.CardType>();
        Dictionary<Card.CardType, int> monsterTypeCounts = new Dictionary<Card.CardType, int>();
        foreach (Card card in fullDeck)
        {
            if (card is MonsterCard)
            {
                if (!monsterTypeCounts.ContainsKey(card.cardType))
                {
                    monsterTypeCounts[card.cardType] = 1;
                }
                else
                {
                    monsterTypeCounts[card.cardType] += 1;
                }
            }
        }
        List<Card.CardType> leastFrequentMonsterTypes = new List<Card.CardType>();
        int leastFrequentMonsterCount = 999;
        foreach(KeyValuePair<Card.CardType, int> entry in monsterTypeCounts)
        {
            if(entry.Value == leastFrequentMonsterCount)
            {
                leastFrequentMonsterTypes.Add(entry.Key);
            }
            else if (entry.Value < leastFrequentMonsterCount)
            {
                leastFrequentMonsterTypes = new List<Card.CardType>() { entry.Key };
                leastFrequentMonsterCount = entry.Value;
            }
        }
        MonsterCard newMonsterCard = Services.CardConfig.CreateCardOfType(
            leastFrequentMonsterTypes[Random.Range(0, leastFrequentMonsterTypes.Count)]) 
            as MonsterCard;
        return newMonsterCard;
    }

    public void AddCard(DungeonCard card)
    {
        discardPile.Add(card);
        Services.UIManager.UpdateDungeonDiscardCounter();
    }

    public TaskTree Act()
    {
        if (hand.Count > 0)
        {
            DungeonCard nextCardToPlay = NextCardToPlay();
            hand.Remove(nextCardToPlay);
            return new TaskTree(new PlayDungeonCard(nextCardToPlay));
        }
        else return null;
    }

    DungeonCard NextCardToPlay()
    {
        DungeonCard cardToPlay = null;
        float priority = Mathf.Infinity;
        for (int i = 0; i < hand.Count; i++)
        {
            DungeonCard card = hand[i];
            if (card.priority < priority)
            {
                cardToPlay = card;
                priority = card.priority;
            }
        }
        return cardToPlay;
    }


    public void SetSpeed(float timeScale)
    {
        forestTimeScale = timeScale;
        if (forestTurn) Time.timeScale = forestTimeScale;
        spedUp = !spedUp;
    }
}

public class AlterDungeonTimerTask : Task
{
    private int amt;
    private DungeonDeck dungeonDeck;
    private float timeElapsed;
    private float midpointDuration;
    private float duration;
    private bool addMonster;
    private MonsterCard newMonsterCard;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 initialScale;
    private Vector3 midpointPos;
    private Vector3 midpointScale;
    private bool forceSpawn;
    private Card card;

    public AlterDungeonTimerTask(DungeonDeck dungeonDeck_, int amt_)
    {
        dungeonDeck = dungeonDeck_;
        amt = amt_;
    }

    public AlterDungeonTimerTask(DungeonDeck dungeonDeck_, Card card_)
    {
        dungeonDeck = dungeonDeck_;
        forceSpawn = true;
        card = card_;
    }

    protected override void Init()
    {
        if (forceSpawn || dungeonDeck.ActuallyAlterDungeonTimerCount(amt))
        {
            addMonster = true;
            timeElapsed = 0;
            duration = Services.MonsterConfig.AddMonsterCardDuration;
            midpointDuration = duration 
                * Services.MonsterConfig.AddMonsterCardMidpointTimeProportion;
            newMonsterCard = dungeonDeck.GetNewMonsterCard();
            //initialPos = Services.UIManager.dungeonTimerPos;
            initialPos = card.controller.transform.position;
            newMonsterCard.CreatePhysicalCard(Services.UIManager.canvas.transform);
            newMonsterCard.controller.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            newMonsterCard.controller.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            newMonsterCard.Reposition(initialPos, true, true);
            midpointPos = initialPos + Services.MonsterConfig.AddMonsterCardMidpointOffset;
            targetPos = Services.UIManager.dungeonDeckPos;
            initialScale = newMonsterCard.controller.transform.localScale;
            midpointScale = initialScale * Services.MonsterConfig.AddMonsterCardScaleUpFactor;
            newMonsterCard.controller.EnterAddToDungeonDeckMode();
            Debug.Log("inital pos " + initialPos);
            Debug.Log("midpoint pos " + midpointPos);
            Debug.Log("target pos " + targetPos);
        }
        else
        {
            addMonster = false;
            SetStatus(TaskStatus.Success);
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed <= midpointDuration)
        {
            newMonsterCard.controller.transform.position = Vector3.Lerp(
                initialPos,
                midpointPos,
                Easing.QuartEaseOut(timeElapsed / midpointDuration));
            newMonsterCard.controller.transform.localScale = Vector3.Lerp(
                initialScale,
                midpointScale,
                Easing.QuartEaseOut(timeElapsed / midpointDuration));
        }
        else
        {
            newMonsterCard.controller.transform.position = Vector3.Lerp(
                midpointPos,
                targetPos,
                Easing.QuartEaseIn(
                    (timeElapsed - midpointDuration) / (duration - midpointDuration)));
            newMonsterCard.controller.transform.localScale = Vector3.Lerp(
                midpointScale,
                Vector3.zero,
                Easing.QuartEaseIn(
                    (timeElapsed - midpointDuration) / (duration - midpointDuration)));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        if (addMonster)
        {
            dungeonDeck.AddCard(newMonsterCard);
            newMonsterCard.DestroyPhysicalCard();
        }
    }
}

public class PerformActions : Task
{
    private TaskManager subTaskManager;

    protected override void Init()
    {
        subTaskManager = new TaskManager();
        AddNextAction();
    }

    internal override void Update()
    {
        subTaskManager.Update();
    }

    void AddNextAction()
    {
        TaskTree nextAction = Services.Main.dungeonDeck.Act();
        if (nextAction != null)
        {
            nextAction.Then(new ActionTask(AddNextAction));
            subTaskManager.AddTask(nextAction);
        }
        else SetStatus(TaskStatus.Success);
    }
}

public class DiscardPlayedCards : Task
{
    private TaskManager subtaskManager;

    protected override void Init()
    {
        subtaskManager = new TaskManager();
        DungeonDeck dungeonDeck = Services.Main.dungeonDeck;
        TaskTree discardPlayedCards = new TaskTree(new EmptyTask());
        float staggerTime = Services.CardConfig.DiscardAnimStaggerTime 
            * (dungeonDeck.playedCards.Count - 1);
        for (int i = dungeonDeck.playedCards.Count - 1; i >= 0; i--)
        {
            discardPlayedCards.AddChild(dungeonDeck
                .DiscardCard(dungeonDeck.playedCards[i], staggerTime));
            staggerTime -= Services.CardConfig.DiscardAnimStaggerTime;
        }
        subtaskManager.AddTask(discardPlayedCards);
    }

    internal override void Update()
    {
        subtaskManager.Update();
        if (subtaskManager.tasksInProcessCount == 0) SetStatus(TaskStatus.Success);
    }

}

