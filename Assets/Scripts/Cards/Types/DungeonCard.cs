using UnityEngine;
using System.Collections;

public abstract class DungeonCard : Card
{
    public float priority { get { return GetPriority(); } }

    public override Color GetCardFrameColor()
    {
        return Services.CardConfig.DungeonCardColor;
    }

    protected virtual float GetPriority()
    {
        return 0f;
    }

    public virtual TaskTree DungeonOnPlay()
    {
        return new TaskTree(new EmptyTask());
    }

    public override TaskTree OnPlay() {
        return new TaskTree(new EmptyTask());
    }
}
