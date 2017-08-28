using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Monster Info")]
public class MonsterInfo : ScriptableObject
{
    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private string name_;
    public string Name { get { return name_; } }

    [SerializeField]
    private Monster.MonsterType monsterType;
    public Monster.MonsterType MonsterType { get { return monsterType; } }

    [SerializeField]
    private int startingHealth;
    public int StartingHealth { get { return startingHealth; } }

    [SerializeField]
    private int attackRange;
    public int AttackRange { get { return attackRange; } }

    [SerializeField]
    private int attackDamage;
    public int AttackDamage { get { return attackDamage; } }

    [SerializeField]
    private int movementSpeed;
    public int MovementSpeed { get { return movementSpeed; } }
}
