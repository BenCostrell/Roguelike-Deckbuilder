using UnityEngine;
using System.Collections;

public class GrowObject : Task
{
    private float timeElapsed;
    private float duration;
    private Tile tile;
    private IPlaceable placeableObj;
    private GameObject gameObj;
    private SpriteRenderer sr;
    private bool fadeIn;
    private GameObject leafBurst;

    public GrowObject(Tile tile_, float dur, IPlaceable obj_)
    {
        tile = tile_;
        duration = dur;
        placeableObj = obj_;
        fadeIn = false;
    }

    public GrowObject(Tile tile_, float dur, IPlaceable obj_, bool fadeIn_)
    {
        tile = tile_;
        duration = dur;
        placeableObj = obj_;
        fadeIn = fadeIn_;
    }

    protected override void Init()
    {
        placeableObj.CreatePhysicalObject(tile);
        gameObj = placeableObj.GetPhysicalObject();
        sr = placeableObj.GetSpriteRenderer();
        gameObj.transform.localScale = Vector3.zero;
        if (fadeIn) sr.color = new Color(1, 1, 1, 0);
        timeElapsed = 0;
        leafBurst = GameObject.Instantiate(Services.Prefabs.LeafBurst);
        leafBurst.transform.position = tile.controller.transform.position;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        gameObj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            Easing.QuadEaseOut(timeElapsed / duration));
        if (fadeIn)
        {
            sr.color = Color.Lerp(new Color(1, 1, 1, 0), new Color(1, 1, 1, 1),
                Easing.QuadEaseOut(timeElapsed / duration));
        }
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
