using UnityEngine;
using System.Collections;

public class Apple : MapObject
{
    public Apple()
    {
        objectType = ObjectType.Apple;
        InitValues();
    }

    public override bool OnStep(Player player)
    {
        player.Heal(1);
        RemoveThis(true);
        return base.OnStep(player);
    }
}
