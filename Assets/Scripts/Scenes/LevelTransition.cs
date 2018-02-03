using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTransition : Scene<MainTransitionData> {

    [SerializeField]
    private TextMeshProUGUI levelTitle;
    [SerializeField]
    private Transform deckArea;
    [SerializeField]
    private TextMeshProUGUI deckCount;
    [SerializeField]
    private Transform dungeonDeckArea;
    [SerializeField]
    private TextMeshProUGUI forestDeckCount;
    [SerializeField]
    private TextMeshProUGUI playerUIHPCounter;
    [SerializeField]
    private RectTransform playerUIRemainingHealthBody;
    [SerializeField]
    private GameObject shieldIcon;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private Button editDeckButton;
    [SerializeField]
    private TextMeshProUGUI finalScore;
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
    [SerializeField]
    private int advanceCount;
    [SerializeField]
    private int growthCount;
    [SerializeField]
    private int spreadSeedsCount;

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
            if (data.dungeonDeck.Count == 0) data.dungeonDeck = GenerateDungeonDeck();

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
            forestDeckCount.text = data.dungeonDeck.Count + "/" + data.dungeonDeck.Count;
            forestDeckCount.color = Color.green;
        }
        else
        {
            startButton.GetComponentInChildren<TextMeshProUGUI>().text = "RESTART";
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
            SetDeckCount();
            if (data.deck.Count > data.maxDeckSize) {
                startButton.interactable = false;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "DECK\n TOO BIG";
                startButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 40;
                deckCount.color = Color.red;
            }
            else if(data.deck.Count < data.minDeckSize)
            {
                startButton.interactable = false;
                startButton.GetComponentInChildren<TextMeshProUGUI>().text = "DECK\n TOO SMALL";
                startButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 40;
                deckCount.color = Color.yellow;
            }
            else
            {
                deckCount.color = Color.green;
            }
            //if (data.levelNum == 1) editDeckButton.gameObject.SetActive(false);
        }
        else
        {
            levelTitle.text = "GAME OVER";
            finalScore.text = "YOU MADE IT TO LEVEL " + data.levelNum;
            editDeckButton.gameObject.SetActive(false);
            dungeonDeckArea.gameObject.SetActive(false);
        }

        SetPlayerUI();
    }

    List<Card> GenerateDungeonDeck()
    {
        List<Card> forestDeck = new List<Card>();
        for (int i = 0; i < advanceCount; i++)
        {
            forestDeck.Add(Services.CardConfig.CreateCardOfType(Card.CardType.Advance));
        }
        for (int i = 0; i < growthCount; i++)
        {
            forestDeck.Add(Services.CardConfig.CreateCardOfType(Card.CardType.Overgrowth));
        }
        for (int i = 0; i < spreadSeedsCount; i++)
        {
            forestDeck.Add(Services.CardConfig.CreateCardOfType(Card.CardType.SpreadSeeds));
        }
        forestDeck.Add(Services.CardConfig.CreateCardOfType(Card.CardType.GrowTheRanks));
        //int highestTier = Mathf.Min((data.levelNum - 1) / levelsPerMonsterTierIncrease,
        //Services.CardConfig.HighestTierOfCardsAvailable(true));
        //int numMonsters = Mathf.FloorToInt(
        //    (Services.MonsterConfig.MonstersPerLevel * data.levelNum)
        //    + Services.MonsterConfig.BaseMonstersPerLevel);
        forestDeck.Add(Services.CardConfig.GenerateCardOfTier(0, true));
        for (int i = 0; i < data.levelNum; i++)
        {
            int tier = i % (Services.CardConfig.HighestTierOfCardsAvailable(true) + 1);
            forestDeck.Add(Services.CardConfig.GenerateCardOfTier(tier, true));
        }

        return forestDeck;
    }

    void SetDeckCount()
    {
        deckCount.text = data.deck.Count + "/" + data.minDeckSize;
    }

    void SetPlayerUI()
    {
        playerUIHPCounter.text = data.currentHealth + "/" + data.maxHealth;
        shieldIcon.SetActive(false);
        playerUIRemainingHealthBody.sizeDelta = new Vector2(
            playerUIRemainingHealthBody.sizeDelta.x * (float)data.currentHealth / data.maxHealth,
            playerUIRemainingHealthBody.sizeDelta.y);
    }

    public void EditDeck()
    {
        Services.SceneStackManager.Swap<DeckConstruction>(data);
    }

}
