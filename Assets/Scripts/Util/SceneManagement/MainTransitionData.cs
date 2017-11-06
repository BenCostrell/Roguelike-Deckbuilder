using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainTransitionData : TransitionData
{
    public List<Card> deck;
    public List<Card> dungeonDeck;
    public List<Card> collection;
    public int currentHealth;
    public int maxHealth;
    public int levelNum;
    public bool gameOver;
    public int minDeckSize { get { return Mathf.Min(20, (levelNum*2) + 8); } }
    public int maxDeckSize { get { return Mathf.Min(20, (levelNum*2) + 8); } }

    public MainTransitionData(List<Card> deck_, List<Card> dungeonDeck_, 
        List<Card> collection_, int currentHealth_, int maxHealth_, int levelNum_, bool gameOver_)
    {
        deck = deck_;
        dungeonDeck = dungeonDeck_;
        collection = collection_;
        currentHealth = currentHealth_;
        maxHealth = maxHealth_;
        levelNum = levelNum_;
        gameOver = gameOver_;
    }
}
