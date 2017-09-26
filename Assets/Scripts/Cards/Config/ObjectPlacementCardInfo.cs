using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Object Placement Card Info")]
public class ObjectPlacementCardInfo : TargetedCardInfo
{
    [SerializeField]
    private MapObject.ObjectType objectType;
    public MapObject.ObjectType ObjectType { get { return objectType; } }
}
