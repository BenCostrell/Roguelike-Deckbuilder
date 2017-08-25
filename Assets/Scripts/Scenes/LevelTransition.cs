﻿using System.Collections;
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
    private int levelsPerMonsterTierIncrease;
    [SerializeField]
    private Vector3 deckDisplayOffset;
    [SerializeField]
    private Vector3 deckDisplaySpacing;
    [SerializeField]
    private Vector3 deckDisplaySpacingPerRow;
    [SerializeField]
    private int maxCardsPerRow;
    private MainTransitionData data;

    internal override void OnEnter(MainTransitionData data_)
    {
        data = data_;
        InitializeScene();
        ClearDeckOfOldMonsters();
        for (int i = 0; i < data.deck.Count; i++)
        {
            data.deck[i].CreatePhysicalCard(deckArea.transform);
            data.deck[i].Reposition(deckDisplayOffset +
                (deckDisplaySpacing * (i % maxCardsPerRow)) +
                (deckDisplaySpacingPerRow * (i / maxCardsPerRow)),
                true);
            data.deck[i].Disable();
            data.deck[i].deckViewMode = true;
        }

        if (!data.gameOver)
        {
            List<Card> monsterCards = GenerateMonstersForLevel();

            for (int j = 0; j < monsterCards.Count; j++)
            {
                monsterCards[j].CreatePhysicalCard(monsterArea.transform);
                monsterCards[j].Reposition(deckDisplayOffset +
                    (deckDisplaySpacing * (j % maxCardsPerRow)) +
                    (deckDisplaySpacingPerRow * (j / maxCardsPerRow)),
                    true);
                monsterCards[j].Disable();
                monsterCards[j].deckViewMode = true;
            }
            AddNewMonsters(monsterCards);
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
        int numMonsters = data.levelNum + 2;
        int highestTier = Mathf.Min(data.levelNum / levelsPerMonsterTierIncrease, 
            Services.CardConfig.HighestTierOfCardsAvailable(true));
        int lowestTier = Mathf.Max(highestTier - 1, 0);
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
