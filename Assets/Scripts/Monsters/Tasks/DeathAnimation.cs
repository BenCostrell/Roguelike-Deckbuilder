using UnityEngine;
using System.Collections;

public class DeathAnimation : Task
{
    private Monster monster;
    private MapObject mapObj;
    private float timeElapsed;
    private float duration;
    private SpriteRenderer sr;
    private SpriteRenderer whitemaskSr;

    public DeathAnimation(Monster monster_)
    {
        monster = monster_;
    }

    public DeathAnimation(MapObject mapObj_)
    {
        mapObj = mapObj_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.MonsterConfig.DeathAnimTime;
        if (monster != null)
        {
            sr = monster.controller.sr;
            whitemaskSr = monster.controller.maskSprite;
            monster.controller.mask.enabled = true;
        }
        else
        {
            sr = mapObj.sr;
            whitemaskSr = mapObj.maskSprite;
            mapObj.mask.enabled = true;
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        sr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0),
            Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed < (duration / 2))
        {
            whitemaskSr.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white,
                Easing.QuadEaseOut(timeElapsed / (duration / 2)));
        }
        else
        {
            whitemaskSr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0),
                Easing.QuadEaseIn((timeElapsed-(duration/2) / (duration / 2))));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
