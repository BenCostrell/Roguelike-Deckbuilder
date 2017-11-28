using UnityEngine;
using System.Collections;

public class FadeOutSprout : Task
{
    private float timeElapsed;
    private float duration;
    private Sprout sprout;
    private SpriteRenderer sr;
    private Transform tform;
    private Vector3 initScale;

    public FadeOutSprout(Sprout sprout_)
    {
        sprout = sprout_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        sr = sprout.GetSpriteRenderer();
        tform = sprout.GetPhysicalObject().transform;
        duration = Services.MapObjectConfig.PlantGrowthTime;
        initScale = tform.localScale;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        sr.color = Color.Lerp(new Color(1, 1, 1, 1), new Color(1, 1, 1, 0),
            Easing.QuadEaseOut(timeElapsed / duration));
        tform.localScale = Vector3.Lerp(initScale, Vector3.zero,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        sprout.RemoveThis(false);
    }
}
