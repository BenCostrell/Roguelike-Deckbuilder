using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Monster Config")]
public class MonsterConfig : ScriptableObject
{
    [SerializeField]
    private MonsterInfo[] monsters;
    public MonsterInfo[] Monsters { get { return monsters; } }

    [SerializeField]
    private float baseMonstersPerLevel;
    public float BaseMonstersPerLevel { get { return baseMonstersPerLevel; } }

    [SerializeField]
    private float monstersPerLevel;
    public float MonstersPerLevel { get { return monstersPerLevel; } }

    [SerializeField]
    private int dungeonTimerThreshold;
    public int DungeonTimerThreshold { get { return dungeonTimerThreshold; } }

    [SerializeField]
    private float addMonsterCardDuration;
    public float AddMonsterCardDuration { get { return addMonsterCardDuration; } }

    [SerializeField]
    private float addMonsterCardMidpointTimeProportion;
    public float AddMonsterCardMidpointTimeProportion { get { return addMonsterCardMidpointTimeProportion; } }

    [SerializeField]
    private float addMonsterCardScaleUpFactor;
    public float AddMonsterCardScaleUpFactor { get { return addMonsterCardScaleUpFactor; } }

    [SerializeField]
    private Vector3 addMonsterCardMidpointOffset;
    public Vector3 AddMonsterCardMidpointOffset { get { return addMonsterCardMidpointOffset; } }

    [SerializeField]
    private int minDistFromMonsters;
    public int MinDistFromMonsters { get { return minDistFromMonsters; } }

    [SerializeField]
    private int spawnRange;
    public int SpawnRange { get { return spawnRange; } }

    [SerializeField]
    private float moveAnimTime;
    public float MoveAnimTime { get { return moveAnimTime; } }

    [SerializeField]
    private float maxMoveAnimDur;
    public float MaxMoveAnimDur { get { return maxMoveAnimDur; } }

    [SerializeField]
    private float attackAnimTime;
    public float AttackAnimTime { get { return attackAnimTime; } }

    [SerializeField]
    private float recoveryAnimTime;
    public float RecoveryAnimTime { get { return recoveryAnimTime; } }

    [SerializeField]
    private float attackAnimDist;
    public float AttackAnimDist { get { return attackAnimDist; } }

    [SerializeField]
    private float deathAnimTime;
    public float DeathAnimTime { get { return deathAnimTime; } }

    [SerializeField]
    private float spawnAnimTime;
    public float SpawnAnimTime { get { return spawnAnimTime; } }

    [SerializeField]
    private int maxSpawnRadius;
    public int MaxSpawnRadius { get { return maxSpawnRadius; } }

    public MonsterInfo GetMonsterOfType(Monster.MonsterType monsterType)
    {
        foreach (MonsterInfo monsterInfo in monsters)
        {
            if (monsterInfo.MonsterType == monsterType)
            {
                return monsterInfo;
            }
        }
        Debug.Assert(false); // we should never be here if monsters are properly configured
        return null;
    }
}
