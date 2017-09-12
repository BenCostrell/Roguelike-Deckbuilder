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

    public SpawnMonster(MonsterCard monsterCard_)
    {
        monsterCard = monsterCard_;
    }

    protected override void Init()
    {
        initialPos = monsterCard.controller.transform.position;
        monster = monsterCard.SpawnMonster();
        spawnTilePos = monster.controller.transform.position;
        timeElapsed = 0;
        monster.controller.transform.position = initialPos;
        duration = Services.MonsterConfig.SpawnAnimTime;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        monster.controller.transform.position = Vector2.Lerp(initialPos, spawnTilePos,
            Easing.QuadEaseIn(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }
}
