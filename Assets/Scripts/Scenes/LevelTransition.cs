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
    private GameObject trashLabel;
    [SerializeField]
    private GameObject playerUIHPCounter;
    [SerializeField]
    private GameObject startButton;
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
    private TrashController trashController;

    internal override void OnEnter(MainTransitionData data_)
    {
        data = data_;
        InitializeScene();
        ClearDeckOfOldMonsters();
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
            AddNewMonsters(monsterCards);
        }
        else
        {
            startButton.GetComponentInChildren<Text>().text = "RESTART";
            trashController.gameObject.SetActive(false);
            trashLabel.SetActive(false);
        }
        if(data.levelNum == 1)
        {
            trashController.gameObject.SetActive(false);
            trashLabel.SetActive(false);
        }
    }

    public void TransitionToLevel()
    {
        if (!data.gameOver)
        {
            data.deck.Remove(trashController.cardToTrash);
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
        trashController = GetComponentInChildren<TrashController>();
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
        if (!data.gameOver)
        {
            levelTitle.GetComponent<Text>().text = "LEVEL " + data.levelNum;
        }
        else
        {
            levelTitle.GetComponent<Text>().text = "GAME OVER - LEVEL " + data.levelNum;

        }
        SetPlayerUI();
    }

    void ClearDeckOfOldMonsters()
    {
        for (int i = data.deck.Count - 1; i >= 0; i--)
        {
            if (data.deck[i].info.IsMonster) data.deck.Remove(data.deck[i]);
        }
    }

    void AddNewMonsters(List<Card> monsterCards)
    {
        data.deck.AddRange(monsterCards);
    }

    List<Card> GenerateMonstersForLevel()
    {
        List<Card> monsterCards = new List<Card>();
        int numMonsters = data.levelNum + 1;
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
}
