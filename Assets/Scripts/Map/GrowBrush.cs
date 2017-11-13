using UnityEngine;
using System.Collections;

public class GrowBrush : Task
{
    private float timeElapsed;
    private float duration;
    private Tile tile;
    private LightBrush brush;

    public GrowBrush(Tile tile_, float dur)
    {
        tile = tile_;
        duration = dur;
    }

    protected override void Init()
    {
        brush = Services.MapObjectConfig.CreateMapObjectOfType(
        MapObject.ObjectType.LightBrush) as LightBrush;
        brush.PlaceOnTile(tile);
        brush.physicalObject.transform.localScale = Vector3.zero;
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        brush.physicalObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
            Easing.QuadEaseOut(timeElapsed / duration));
        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
