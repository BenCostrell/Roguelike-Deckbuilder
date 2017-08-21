using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainTransitionData : TransitionData
{
    public List<Card> deck;
    public int maxHealth;

    public MainTransitionData(List<Card> deck_, int maxHealth_)
    {
        deck = deck_;
        maxHealth = maxHealth_;
    }
}
