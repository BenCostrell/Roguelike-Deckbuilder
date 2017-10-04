using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Tome : CardSelectionCard
{
    public Tome()
    {
        cardType = CardType.Tome;
        InitValues();
    }

    public override bool IsSelectionComplete(List<Card> cardsSelected)
    {
        return cardsSelected.Count == 1;
    }

    public override bool IsSelectionValid(Card card)
    {
        return player.hand.Contains(card);
    }

    public override TaskTree PreCardSelectionActions()
    {
        return player.DrawCards(2);
    }

    public override void OnSelectionComplete(List<Card> cardsSelected)
    {
        foreach(Card card in cardsSelected)
        {
            player.DiscardCardFromHand(card);
        }
    }
}
