using UnityEngine;
using System.Collections;

public class Bloodlust : MovementCard
{
    public Bloodlust()
    {
        cardType = CardType.Bloodlust;
        InitValues();
        baseRange = 0;
    }

    protected override int GetRange()
    {
        int movementRange = 0;
        foreach (Card card in player.hand)
        {
            if (card is AttackCard) movementRange += 1;
        }
        return movementRange;
    }
}
