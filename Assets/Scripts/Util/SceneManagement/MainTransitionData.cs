using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainTransitionData : TransitionData
{
    public List<Card> deck;
    public List<Card> dungeonDeck;
    public List<Card> collection;
    public int maxHealth;
    public int levelNum;
    public bool gameOver;
    public int minDeckSize { get { return levelNum + 9; } }
    public int maxDeckSize { get { return levelNum + 9; } }

    public MainTransitionData(List<Card> deck_, List<Card> dungeonDeck_, 
        List<Card> collection_, int maxHealth_, int levelNum_, bool gameOver_)
    {
        deck = deck_;
        dungeonDeck = dungeonDeck_;
        collection = collection_;
        maxHealth = maxHealth_;
        levelNum = levelNum_;
        gameOver = gameOver_;
    }
}
