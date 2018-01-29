using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Growing Plant Info")]
public class GrowingPlantInfo : MapObjectInfo
{
    [SerializeField]
    private int growthTime;
    public int GrowthTime { get { return growthTime; } }
}
