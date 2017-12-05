using UnityEngine;
using System.Collections;

public class MoveMapObject : Task
{
    private float timeElapsed;
    private float duration;
    private MapObject mapObj;
    private Tile startTile;
    private Tile targetTile;
    private Vector3 startPos;
    private Vector3 endPos;
    private Transform tform;

    public MoveMapObject(MapObject mapObj_, Tile target_)
    {
        mapObj = mapObj_;
        targetTile = target_;
        startTile = mapObj.GetCurrentTile();
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.MonsterConfig.MoveAnimTime;
        tform = mapObj.GetPhysicalObject().transform;
        startPos = startTile.controller.transform.position;
        endPos = targetTile.controller.transform.position;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        tform.position = Vector3.Lerp(startPos, endPos, 
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        mapObj.PlaceOnTile(targetTile);
        Services.GameManager.player.ShowAvailableMoves();
    }
}
