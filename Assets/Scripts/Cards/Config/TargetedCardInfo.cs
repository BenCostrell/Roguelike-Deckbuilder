using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Targeted Card Info")]
public class TargetedCardInfo : CardInfo
{
    [SerializeField]
    private int range;
    public int Range { get { return range; } }
}
