using UnityEngine;
using System.Collections;

public class TargetedCardInfo : CardInfo
{
    [SerializeField]
    private int range;
    public int Range { get { return range; } }
}
