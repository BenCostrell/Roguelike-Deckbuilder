using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveObjectAlongPath : Task
{
    private float duration;
    private float timeElapsed;
    private GameObject obj;
    private List<Tile> path;
    private Vector3 curPos;
    private Vector3 nextPos;
    private int pathIndex;

    public MoveObjectAlongPath(GameObject obj_, List<Tile> path_)
    {
        obj = obj_;
        path = path_;
    }

    protected override void Init()
    {
        pathIndex = path.Count - 1;
        timeElapsed = 0;
        curPos = obj.transform.position;
        nextPos = path[pathIndex].controller.transform.position;
        duration = Services.MonsterConfig.MoveAnimTime;
        if (obj.GetComponent<MonsterController>() != null)
        {
            if (path.Count * duration > Services.MonsterConfig.MaxMoveAnimDur)
            {
                duration = Services.MonsterConfig.MaxMoveAnimDur / path.Count;
            }
        }
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        obj.transform.position = Vector3.Lerp(curPos, nextPos,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration)
        {
            if (pathIndex == 0)
            {
                SetStatus(TaskStatus.Success);
            }
            else
            {
                curPos = nextPos;
                pathIndex -= 1;
                nextPos = path[pathIndex].controller.transform.position;
                timeElapsed = 0;
            }
        }
    }

    protected override void OnSuccess()
    {
        Tile finalTile = path[0];
        if (obj.GetComponent<MonsterController>() != null)
        {
            obj.GetComponent<MonsterController>().monster.PlaceOnTile(finalTile);
        }
        if(obj.GetComponent<PlayerController>() != null)
        {
            Player player = obj.GetComponent<PlayerController>().player;
            player.PlaceOnTile(finalTile);
            if (finalTile.hovered) player.OnTileHover(finalTile);
        }
    }
}
