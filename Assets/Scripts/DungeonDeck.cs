using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonDeck 
{
    private readonly List<Card> fullDeck;
    private List<Card> deck;
    public int deckCount { get { return deck.Count; } }
    public List<Card> playedCards;
    public List<Card> discardPile { get; private set; }
    public int discardCount { get { return discardPile.Count; } }
    public List<Card> cardsInFlux;
    private const int baseCardsPerRound = 5;
    private int cardsPerRound;
    private int dungeonTimerCount;
    private int dungeonTimerThreshold;

    public DungeonDeck(List<Card> baseDeck)
    {
        fullDeck = baseDeck;
        deck = new List<Card>(fullDeck);
        playedCards = new List<Card>();
        discardPile = new List<Card>();
        cardsInFlux = new List<Card>();
        cardsPerRound = baseCardsPerRound;
        dungeonTimerCount = 0;
        dungeonTimerThreshold = Services.MonsterConfig.DungeonTimerThreshold;
        UpdateDungeonTimer();
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
        cardDrawTasks.Then(new ActionTask(PutCardsInPlayedMode));
        return cardDrawTasks;
    }

    void PutCardsInPlayedMode()
    {
        for (int i = 0; i < playedCards.Count; i++)
        {
            playedCards[i].controller.EnterPlayedMode();
        }
    }

    public TaskTree TakeDungeonTurn()
    {
        TaskTree turnTasks = new TaskTree(new EmptyTask());
        float staggerTime = Services.CardConfig.DiscardAnimStaggerTime
            * (playedCards.Count - 1);
        for (int i = playedCards.Count - 1; i >= 0; i--)
        {
            turnTasks.AddChild(DiscardCard(playedCards[i], staggerTime));
            staggerTime -= Services.CardConfig.DiscardAnimStaggerTime;
        }
        turnTasks.Then(DrawCards(cardsPerRound));
        return turnTasks;
    }

    TaskTree DiscardCard(Card card, float timeOffset)
    {
        discardPile.Add(card);
        playedCards.Remove(card);
        TaskTree discardTasks = new TaskTree(new WaitTask(timeOffset));
        discardTasks.Then(new ActionTask(card.OnDiscard));
        discardTasks.Then(new DiscardCard(card));
        discardTasks.Then(new ActionTask(Services.UIManager.UpdateDungeonDiscardCounter));
        return discardTasks;
    }

    void UpdateDungeonTimer()
    {
        Services.UIManager.UpdateDungeonTimer((float)dungeonTimerCount / dungeonTimerThreshold);
    }

    public TaskTree AlterDungeonTimerCount(int amt)
    {
        return new TaskTree(new AlterDungeonTimerTask(this, amt));
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
        UpdateDungeonTimer();
        return addMonster;
    }

    public MonsterCard GetNewMonsterCard()
    {
        List<Card.CardType> monsterTypes = new List<Card.CardType>();
        foreach (Card card in fullDeck)
        {
            if (card is MonsterCard && !monsterTypes.Contains(card.cardType)) 
                monsterTypes.Add(card.cardType);
        }
        MonsterCard newMonsterCard = Services.CardConfig.CreateCardOfType(
            monsterTypes[Random.Range(0, monsterTypes.Count)]) as MonsterCard;
        return newMonsterCard;
    }

    public void AddCard(Card card)
    {
        discardPile.Add(card);
        Services.UIManager.UpdateDungeonDiscardCounter();
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

    public AlterDungeonTimerTask(DungeonDeck dungeonDeck_, int amt_)
    {
        dungeonDeck = dungeonDeck_;
        amt = amt_;
    }

    protected override void Init()
    {
        if (dungeonDeck.ActuallyAlterDungeonTimerCount(amt))
        {
            addMonster = true;
            timeElapsed = 0;
            duration = Services.MonsterConfig.AddMonsterCardDuration;
            midpointDuration = duration 
                * Services.MonsterConfig.AddMonsterCardMidpointTimeProportion;
            newMonsterCard = dungeonDeck.GetNewMonsterCard();
            initialPos = Services.UIManager.dungeonTimerPos;
            newMonsterCard.CreatePhysicalCard(Services.UIManager.bottomLeft);
            newMonsterCard.Reposition(initialPos, true, true);
            midpointPos = initialPos + Services.MonsterConfig.AddMonsterCardMidpointOffset;
            targetPos = Services.UIManager.dungeonDeckPos;
            initialScale = newMonsterCard.controller.transform.localScale;
            midpointScale = initialScale * Services.MonsterConfig.AddMonsterCardScaleUpFactor;
            newMonsterCard.controller.EnterAddToDungeonDeckMode();
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
            newMonsterCard.Reposition(Vector3.Lerp(
                initialPos,
                midpointPos,
                Easing.QuartEaseOut(timeElapsed / midpointDuration)), false, true);
            newMonsterCard.controller.transform.localScale = Vector3.Lerp(
                initialScale,
                midpointScale,
                Easing.QuartEaseOut(timeElapsed / midpointDuration));
        }
        else
        {
            newMonsterCard.Reposition(Vector3.Lerp(
                midpointPos,
                targetPos,
                Easing.QuartEaseIn(
                    (timeElapsed - midpointDuration) / (duration - midpointDuration))),
                false, true);
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

