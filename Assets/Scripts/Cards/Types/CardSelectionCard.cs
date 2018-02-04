using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class CardSelectionCard : Card
{
    public abstract bool IsSelectionValid(Card card);

    public abstract bool IsSelectionComplete(List<Card> cardsSelected);

    public abstract void OnSelectionComplete(List<Card> cardsSelected);

    public virtual TaskTree PreCardSelectionActions()
    {
        return new TaskTree(new EmptyTask());
    }

    public override TaskTree OnPlay()
    {
        TaskTree actions =
                    base.OnPlay();
        actions.Then(PreCardSelectionActions());
        actions.Then(new CardSelection(this));
        return actions;
    }

}
