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
    private Monster monster;
    private Player player;
    private bool stopped;
    private Tile finalTile;
    private bool noMovement;

    public MoveObjectAlongPath(GameObject obj_, List<Tile> path_)
    {
        obj = obj_;
        path = path_;
        stopped = false;
        monster = null;
        player = null;
        if (obj.GetComponent<MonsterController>() != null)
        {
            isMonster = true;
            monster = obj.GetComponent<MonsterController>().monster;
            if (path.Count > 0)
            {
                finalTile = path[0];
            }
            else
            {
                finalTile = monster.currentTile;
                noMovement = true;
            }
            finalTile.monsterMovementTarget = true;
        }
        else
        {
            player = obj.GetComponent<PlayerController>().player;
            finalTile = path[0];
        }
    }

    protected override void Init()
    {
        if (noMovement)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
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
            player.moving = true;
            player.HideArrow();
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
                MapObject mapObj = path[pathIndex].containedMapObject;
                if (mapObj != null) {
                    if (isMonster)
                    {
                        stopped = mapObj.OnStep(monster);
                    }
                    else
                    {
                        stopped = mapObj.OnStep(player);
                        if (stopped) player.movesAvailable = 0;
                    }
                    if (stopped)
                    {
                        if (isMonster) finalTile.monsterMovementTarget = false;
                        finalTile = path[pathIndex];
                        SetStatus(TaskStatus.Success);
                    }
                }
                curPos = nextPos;
                pathIndex -= 1;
                nextPos = path[pathIndex].controller.transform.position;
                timeElapsed = 0;
            }
        }
    }

    protected override void OnSuccess()
    {
        if (isMonster)
        {
            finalTile.monsterMovementTarget = false;
            if (!monster.markedForDeath) monster.PlaceOnTile(finalTile);
            if(finalTile.containedMapObject != null && !stopped)
            {
                finalTile.containedMapObject.OnStep(monster);
            }
            monster.targetTile = null;
        }
        else
        {
            player.PlaceOnTile(finalTile, true);
            player.moving = false;
            if (finalTile.containedMapObject != null)
            {
                stopped = finalTile.containedMapObject.OnStep(player);
                if (stopped) player.movesAvailable = 0;
            }
            if (finalTile.hovered) player.OnTileHover(finalTile);
            if (finalTile.isExit && player.hasKey) Services.Main.ExitLevel();
        }
    }
}
