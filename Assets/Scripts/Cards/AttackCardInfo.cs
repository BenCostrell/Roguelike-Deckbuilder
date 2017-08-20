using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Attack Card Info")]
public class AttackCardInfo : CardInfo
{
    [SerializeField]
    private int range;
    public int Range { get { return range; } }

    [SerializeField]
    private int damage;
    public int Damage { get { return damage; } }
}
