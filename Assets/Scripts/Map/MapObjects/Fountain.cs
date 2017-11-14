using UnityEngine;
using System.Collections;

public class Fountain : MapObject
{
    private bool used;

    public Fountain()
    {
        objectType = ObjectType.Fountain;
        InitValues();
    }

    public override bool OnStep(Player player)
    {
        if (!used)
        {
            Services.UIManager.InitOptions(
                new string[2] { "HEAL FULLY", "+1 MAX HP" },
                new Sprite[2] { info.Sprites[1], info.Sprites[2] }
            );
            Services.EventManager.Register<OptionChosen>(OnOptionChosen);
            used = true;
        }
        return base.OnStep(player);
    }

    void OnOptionChosen(OptionChosen e)
    {
        if(e.optionChosen == 1)
        {
            Services.GameManager.player.Heal(player.maxHealth - player.currentHealth);
        }
        else
        {
            Services.GameManager.player.GainMaxHealth(1);
        }
        Services.EventManager.Unregister<OptionChosen>(OnOptionChosen);
        RemoveThis(false);
    }
}
