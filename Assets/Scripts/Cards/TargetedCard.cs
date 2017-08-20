using UnityEngine;
using System.Collections;

public abstract class TargetedCard : Card
{
    public override void OnPlay()
    {
        base.OnPlay();
        Services.Main.taskManager.AddTask(new TargetSelection(this));
    }

    public abstract bool IsTargetValid(Tile tile);

    public abstract void OnTargetSelected(Tile tile);
}
