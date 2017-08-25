using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainTransitionData : TransitionData
{
    public List<Card> deck;
    public int maxHealth;
    public int levelNum;

    public MainTransitionData(List<Card> deck_, int maxHealth_, int levelNum_)
    {
        deck = deck_;
        maxHealth = maxHealth_;
        levelNum = levelNum_;
    }
}
