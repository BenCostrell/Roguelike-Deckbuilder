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
    private bool isMonster;

    public MoveObjectAlongPath(GameObject obj_, List<Tile> path_)
    {
        obj = obj_;
        path = path_;
        if (obj.GetComponent<MonsterController>() != null)
        {
            isMonster = true;
            curPos = obj.transform.position;
            path[0].monsterMovementTarget = true;
        }
    }

    protected override void Init()
    {
        pathIndex = path.Count - 1;
        timeElapsed = 0;
        curPos = obj.transform.position;
        nextPos = path[pathIndex].controller.transform.position;
        duration = Services.MonsterConfig.MoveAnimTime;
        if (isMonster)
        {
            if (path.Count * duration > Services.MonsterConfig.MaxMoveAnimDur)
            {
                duration = Services.MonsterConfig.MaxMoveAnimDur / path.Count;
            }
        }
        else
        {
            obj.GetComponent<PlayerController>().player.moving = true;
            obj.GetComponent<PlayerController>().player.HideArrow();
        }
    }

    internal override void Update()
    {
        if (obj == null || (isMonster && obj.GetComponent<MonsterController>().monster.markedForDeath))
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        timeElapsed += Time.deltaTime;

        obj.transform.position = Vector3.Lerp(curPos, nextPos,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration)
        {
            if(isMonster)
            {
                obj.GetComponent<MonsterController>().monster.PlaceOnTile(path[pathIndex]);
            }
            if (path[pathIndex].containedMapObject != null)
            {
                if (isMonster)
                {
                    path[pathIndex].containedMapObject.OnStep(obj.GetComponent<MonsterController>().monster);
                }
                else
                {
                    path[pathIndex].containedMapObject.OnStep(obj.GetComponent<PlayerController>().player);
                }
            }
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
        finalTile.monsterMovementTarget = false;
        if (obj == null || (isMonster && obj.GetComponent<MonsterController>().monster.markedForDeath)) return;
        if (isMonster)
        {
            obj.GetComponent<MonsterController>().monster.PlaceOnTile(finalTile);
        }
        else
        {
            Player player = obj.GetComponent<PlayerController>().player;
            player.PlaceOnTile(finalTile);
            player.moving = false;
            if (finalTile.hovered) player.OnTileHover(finalTile);
            if (finalTile.isExit && player.hasKey) Services.Main.ExitLevel();
        }
    }
}
