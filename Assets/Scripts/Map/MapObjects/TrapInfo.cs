using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Trap Info")]
public class TrapInfo : MapObjectInfo
{
    [SerializeField]
    private int damage;
    public int Damage { get { return damage; } }
}
