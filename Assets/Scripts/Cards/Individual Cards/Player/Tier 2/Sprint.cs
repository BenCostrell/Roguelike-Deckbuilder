using UnityEngine;
using System.Collections;

public class Sprint : MovementCard
{
    public Sprint()
    {
        cardType = CardType.Sprint;
        InitValues();
        baseRange = 3;
    }
}
