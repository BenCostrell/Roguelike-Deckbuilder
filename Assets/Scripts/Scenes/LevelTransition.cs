using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransition : Scene<MainTransitionData> {

    [SerializeField]
    private Text levelTitle;
    [SerializeField]
    private Transform deckArea;
    [SerializeField]
    private Transform dungeonDeckArea;
    [SerializeField]
    private Text playerUIHPCounter;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Text finalScore;
    [SerializeField]
    private int levelsPerMonsterTierIncrease;
    [SerializeField]
    private Vector3 deckDisplayOffset;
    [SerializeField]
    private Vector3 deckDisplaySpacing;
    [SerializeField]
    private Vector3 deckDisplaySpacingPerRow;
    [SerializeField]
    private int maxCardsPerRow;
    public MainTransitionData data { get; private set; }
    [SerializeField]
    private int dungeonDeckSize;

    internal override void OnEnter(MainTransitionData data_)
    {
        data = data_;
        InitializeScene();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
        Services.GameManager.currentCanvas = GetComponentInChildren<Canvas>();
        Services.SoundManager.SetMusicVolume(0.05f);
        for (int i = 0; i < data.deck.Count; i++)
        {
            data.deck[i].CreatePhysicalCard(deckArea.transform);
            data.deck[i].Reposition(
                deckDisplayOffset +
                (deckDisplaySpacing * (i % maxCardsPerRow)) +
                (deckDisplaySpacingPerRow * (i / maxCardsPerRow)),
                true);
            data.deck[i].controller.EnterDeckViewMode();
        }

        if (!data.gameOver)
        {
            if (data.dungeonDeck.Count == 0) data.dungeonDeck = GenerateMonstersForLevel();

            for (int j = 0; j < data.dungeonDeck.Count; j++)
            {
                data.dungeonDeck[j].CreatePhysicalCard(dungeonDeckArea.transform);
                data.dungeonDeck[j].Reposition(
                    deckDisplayOffset +
                    (deckDisplaySpacing * (j % maxCardsPerRow)) +
                    (deckDisplaySpacingPerRow * (j / maxCardsPerRow)),
                    true);
                data.dungeonDeck[j].controller.EnterDeckViewMode();
            }
        }
        else
        {
            startButton.GetComponentInChildren<Text>().text = "RESTART";
        }
    }

    public void TransitionToLevel()
    {
        if (!data.gameOver)
        {
            Services.SceneStackManager.Swap<Main>(data);
        }
        else
        {
            Services.EventManager.Fire(new Reset());
        }
    }

    void InitializeScene()
    {
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
        if (!data.gameOver)
        {
            levelTitle.text = "LEVEL " + data.levelNum;
            finalScore.gameObject.SetActive(false);
            if (data.deck.Count > data.maxDeckSize || data.deck.Count < data.minDeckSize)
            {
                startButton.interactable = false;
                startButton.GetComponentInChildren<Text>().text = "INVALID DECK";
                startButton.GetComponentInChildren<Text>().fontSize = 40;
            }
        }
        else
        {
            levelTitle.text = "GAME OVER";
            finalScore.text = "YOU MADE IT TO LEVEL " + data.levelNum;
        }

        SetPlayerUI();
    }

    List<Card> GenerateMonstersForLevel()
    {
        List<Card> monsterCards = new List<Card>();
        int numMonsters = Mathf.Min(
            Mathf.FloorToInt((Services.MonsterConfig.MonstersPerLevel * data.levelNum)
            + Services.MonsterConfig.BaseMonstersPerLevel), dungeonDeckSize);
        int numTicks = Mathf.Max(0, dungeonDeckSize - numMonsters);
        for (int i = 0; i < numTicks; i++)
        {
            monsterCards.Add(Services.CardConfig.CreateCardOfType(Card.CardType.Tick));
        }
        int highestTier = Mathf.Min(data.levelNum / levelsPerMonsterTierIncrease, 
            Services.CardConfig.HighestTierOfCardsAvailable(true));
        int lowestTier;
        if (data.levelNum / levelsPerMonsterTierIncrease > highestTier)
        {
            lowestTier = highestTier;
        }
        else lowestTier = Mathf.Max(highestTier - 1, 0);
        float proportionOfLowTier =
            ((levelsPerMonsterTierIncrease - (data.levelNum % levelsPerMonsterTierIncrease)) /
            (float)(levelsPerMonsterTierIncrease + 1));
        int numLowTierMonsters = Mathf.RoundToInt(proportionOfLowTier * numMonsters);
        int numHighTierMonsters = numMonsters - numLowTierMonsters;

        for (int i = 0; i < numLowTierMonsters; i++)
        {
            monsterCards.Add(
                Services.CardConfig.GenerateCardOfTier(lowestTier, true));
        }
        for (int j = 0; j < numHighTierMonsters; j++)
        {
            monsterCards.Add(
                Services.CardConfig.GenerateCardOfTier(highestTier, true));
        }

        return monsterCards;
    }

    void SetPlayerUI()
    {
        playerUIHPCounter.text = data.maxHealth + "/" + data.maxHealth;
    }

    public void EditDeck()
    {
        Services.SceneStackManager.Swap<DeckConstruction>(data);
    }
}
