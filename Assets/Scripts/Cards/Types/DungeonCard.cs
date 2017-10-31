using UnityEngine;
using System.Collections;

public class DungeonCard : Card
{
    public override Color GetCardFrameColor()
    {
        return Services.CardConfig.DungeonCardColor;
    }
}
