using UnityEngine;
using System.Collections;

public class DeathAnimation : Task
{
    private Monster monster;
    private float timeElapsed;
    private float duration;
    private SpriteRenderer monsterSr;
    private SpriteRenderer whitemaskSr;

    public DeathAnimation(Monster monster_)
    {
        monster = monster_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.MonsterConfig.DeathAnimTime;
        monsterSr = monster.controller.sr;
        whitemaskSr = monster.controller.maskSprite;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        monsterSr.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0),
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
