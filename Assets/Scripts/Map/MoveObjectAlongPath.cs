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
    private bool stopped;

    public MoveObjectAlongPath(GameObject obj_, List<Tile> path_)
    {
        obj = obj_;
        path = path_;
        stopped = false;
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
        timeElapsed += Time.deltaTime;

        obj.transform.position = Vector3.Lerp(curPos, nextPos,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration)
        {
            if (pathIndex == 0) SetStatus(TaskStatus.Success);
            else
            {
                if (isMonster)
                {
                    stopped = obj.GetComponent<MonsterController>().monster.PlaceOnTile(path[pathIndex]);
                }
                else
                {
                    stopped = obj.GetComponent<PlayerController>().player.PlaceOnTile(path[pathIndex], false);
                    if (stopped) obj.GetComponent<PlayerController>().player.movesAvailable = 0;
                }
                if (stopped) SetStatus(TaskStatus.Success);
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
        Player player;
        if (!isMonster)
        {
            player = obj.GetComponent<PlayerController>().player;
            player.moving = false;
            if (finalTile.hovered) player.OnTileHover(finalTile);
            if (finalTile.isExit && player.hasKey) Services.Main.ExitLevel();
            if(!stopped) player.PlaceOnTile(finalTile, true);
        }
        else if (!stopped)
        {
            Monster monster = obj.GetComponent<MonsterController>().monster;
            monster.PlaceOnTile(finalTile);
            monster.targetTile = null;
        }
    }
}
