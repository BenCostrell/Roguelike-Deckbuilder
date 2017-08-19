using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Monster Config")]
public class MonsterConfig : ScriptableObject
{
    [SerializeField]
    private MonsterInfo[] monsters;
    public MonsterInfo[] Monsters { get { return monsters; } }

    [SerializeField]
    private int minDistFromMonsters;
    public int MinDistFromMonsters { get { return minDistFromMonsters; } }

    [SerializeField]
    private int minDistFromCards;
    public int MinDistFromCards { get { return minDistFromCards; } }

    [SerializeField]
    private int minDistFromPlayer;
    public int MinDistFromPlayer { get { return minDistFromPlayer; } }

    [SerializeField]
    private int spawnRange;
    public int SpawnRange { get { return spawnRange; } }

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
