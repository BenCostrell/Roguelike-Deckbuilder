using UnityEngine;
using System.Collections;

public class Fountain : MapObject
{
    private bool used;
    private SpriteRenderer shimmerSr;
    private float timeElapsed;
    private const float period = 1f;
    private float direction;

    public Fountain()
    {
        objectType = ObjectType.Fountain;
        InitValues();
    }
    public override void CreatePhysicalObject(Tile tile)
    {
        base.CreatePhysicalObject(tile);
        //light.gameObject.SetActive(true);
        //ps.Play();
        shimmerSr = GameObject.Instantiate(Services.Prefabs.HealingPlantParticles, 
            physicalObject.transform).GetComponent<SpriteRenderer>();
        direction = 1;
        shimmerSr.transform.localPosition = new Vector3(0.02f, 0.1f, 0);
    }

    public override void RemoveThis(bool animate)
    {
        base.RemoveThis(animate);
        Services.MapManager.RemoveLitMapObject(this);
        Services.MapManager.RemoveFountain(this);
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

    public void Update()
    {
        timeElapsed += direction * Time.deltaTime;
        shimmerSr.color = Color.Lerp(new Color(1, 1, 1, 0.25f), new Color(1, 1, 1, 0.75f),
            Easing.QuadEaseIn(timeElapsed / period));
        shimmerSr.transform.localScale = Vector3.Lerp(0.1f * Vector3.one, 0.3f * Vector3.one,
            Easing.QuadEaseOut(timeElapsed / period));
        if(timeElapsed >= period && direction == 1)
        {
            direction *= -1;
        }
        else if (timeElapsed <= 0 && direction == -1)
        {
            direction *= -1;
        }
    }
}
