using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardSelection : Task
{
    private List<Card> cardsSelected;
    private int lockID;
    private CardSelectionCard card;

    public CardSelection(CardSelectionCard card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        lockID = Services.UIManager.nextLockID;
        Services.GameManager.player.LockEverything(lockID);
        Services.GameManager.player.selectingCards = true;
        Services.EventManager.Register<CardSelected>(OnCardSelected);
        cardsSelected = new List<Card>();
    }

    void OnCardSelected(CardSelected e)
    {
        Card cardSelected = e.card;
        if (card.IsSelectionValid(cardSelected))
        {
            if (cardsSelected.Contains(cardSelected))
            {
                cardsSelected.Remove(cardSelected);
                cardSelected.controller.UnselectedForCard();
            }
            else
            {
                cardsSelected.Add(cardSelected);
                cardSelected.controller.SelectedForCard();
            }
            if (card.IsSelectionComplete(cardsSelected)) SetStatus(TaskStatus.Success);
        }
    }

    protected override void OnSuccess()
    {
        Services.GameManager.player.UnlockEverything(lockID);
        card.OnSelectionComplete(cardsSelected);
        Services.EventManager.Unregister<CardSelected>(OnCardSelected);
        Services.GameManager.player.selectingCards = false;
    }
}
