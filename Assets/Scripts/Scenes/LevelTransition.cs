using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTransition : Scene<MainTransitionData> {

    [SerializeField]
    private GameObject levelTitle;
    [SerializeField]
    private GameObject deckArea;
    [SerializeField]
    private GameObject monsterArea;
    [SerializeField]
    private GameObject playerUIHPCounter;
    [SerializeField]
    private GameObject startButton;
    [SerializeField]
    private GameObject finalScore;
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
        Services.SoundManager.SetMusicVolume(0.05f);
        for (int i = 0; i < data.deck.Count; i++)
        {
            data.deck[i].CreatePhysicalCard(transform);
            data.deck[i].Reposition(deckArea.transform.position +
                deckDisplayOffset +
                (deckDisplaySpacing * (i % maxCardsPerRow)) +
                (deckDisplaySpacingPerRow * (i / maxCardsPerRow)),
                true);
            data.deck[i].deckViewMode = true;
            data.deck[i].Disable();
        }

        if (!data.gameOver)
        {
            List<Card> monsterCards = GenerateMonstersForLevel();

            for (int j = 0; j < monsterCards.Count; j++)
            {
                monsterCards[j].CreatePhysicalCard(transform);
                monsterCards[j].Reposition(monsterArea.transform.position +
                    deckDisplayOffset +
                    (deckDisplaySpacing * (j % maxCardsPerRow)) +
                    (deckDisplaySpacingPerRow * (j / maxCardsPerRow)),
                    true);
                monsterCards[j].deckViewMode = true;
                monsterCards[j].Disable();
            }
            data.dungeonDeck = monsterCards;
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
            foreach (Card card in data.deck)
            {
                card.deckViewMode = false;
            }
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
            levelTitle.GetComponent<Text>().text = "LEVEL " + data.levelNum;
            finalScore.SetActive(false);
        }
        else
        {
            levelTitle.GetComponent<Text>().text = "GAME OVER";
            finalScore.GetComponent<Text>().text = "YOU MADE IT TO LEVEL " + data.levelNum;

        }
        if (data.deck.Count > data.maxDeckSize || data.deck.Count < data.minDeckSize)
        {
            startButton.GetComponent<Button>().interactable = false;
            startButton.GetComponentInChildren<Text>().text = "INVALID DECK";
            startButton.GetComponentInChildren<Text>().fontSize = 40;
        }
        SetPlayerUI();
    }

    List<Card> GenerateMonstersForLevel()
    {
        List<Card> monsterCards = new List<Card>();
        int numMonsters = Mathf.Min(
            Mathf.FloorToInt((Services.MonsterConfig.MonstersPerLevel * data.levelNum)
            + Services.MonsterConfig.BaseMonstersPerLevel), dungeonDeckSize);
        int numBlanks = Mathf.Max(0, dungeonDeckSize - numMonsters);
        for (int i = 0; i < numBlanks; i++)
        {
            monsterCards.Add(Services.CardConfig.CreateCardOfType(Card.CardType.Blank));
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
        playerUIHPCounter.GetComponent<Text>().text = data.maxHealth + "/" + data.maxHealth;
    }

    public void EditDeck()
    {
        for (int i = 0; i < data.deck.Count; i++)
        {
            data.deck[i].deckViewMode = false;
        }
        // for testing purposes
        //data.collection = new List<Card>();
        //for (int i = 0; i < Services.CardConfig.Cards.Length; i++)
        //{
        //    data.collection.Add(
        //        Services.CardConfig.CreateCardOfType(Services.CardConfig.Cards[i].CardType));
        //    data.collection.Add(
        //        Services.CardConfig.CreateCardOfType(Services.CardConfig.Cards[i].CardType));
        //}
        Services.SceneStackManager.Swap<DeckConstruction>(data);
    }
}
