using UnityEngine;
using System.Collections;

public class SpawnMonster : Task
{
    private float timeElapsed;
    private float duration;
    private MonsterCard monsterCard;
    private Monster monster;
    private Vector2 spawnTilePos;
    private Vector2 initialPos;
    private const float zOffset = 5f;

    public SpawnMonster(MonsterCard monsterCard_)
    {
        monsterCard = monsterCard_;
    }

    protected override void Init()
    {
        initialPos = monsterCard.controller.transform.position + Vector3.back * zOffset;
        monster = monsterCard.SpawnMonster();
        if(monster == null)
        {
            SetStatus(TaskStatus.Success);
            return;
        }
        spawnTilePos = monster.controller.transform.position;
        timeElapsed = 0;
        monster.controller.transform.position = initialPos;
        duration = Services.MonsterConfig.SpawnAnimTime;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        monster.controller.transform.position = Vector3.Lerp(initialPos, spawnTilePos,
            Easing.QuadEaseIn(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
