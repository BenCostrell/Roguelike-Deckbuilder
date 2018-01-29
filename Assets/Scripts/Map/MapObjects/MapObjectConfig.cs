using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Map Object Config")]
public class MapObjectConfig : ScriptableObject
{
    [SerializeField]
    private MapObjectInfo[] mapObjects;
    public MapObjectInfo[] MapObjects { get { return mapObjects; } }

    [SerializeField]
    private float plantGrowthTime;
    public float PlantGrowthTime { get { return plantGrowthTime; } }

    public MapObjectInfo GetMapObjectOfType(MapObject.ObjectType objectType)
    {
        foreach (MapObjectInfo info in mapObjects)
        {
            if (info.ObjectType == objectType)
            {
                return info;
            }
        }
        Debug.Assert(false); // we should never be here if map objects are properly configured
        return null;
    }

    public MapObject CreateMapObjectOfType(MapObject.ObjectType type)
    {
        switch (type)
        {
            case MapObject.ObjectType.SpikeTrap:
                return new SpikeTrap();
            case MapObject.ObjectType.Fountain:
                return new Fountain();
            case MapObject.ObjectType.LightBrush:
                return new LightBrush();
            case MapObject.ObjectType.HeavyBrush:
                return new HeavyBrush();
            case MapObject.ObjectType.Chest:
                return new Chest();
            case MapObject.ObjectType.Sprout:
                return new Sprout();
            case MapObject.ObjectType.Door:
                return new Door();
            case MapObject.ObjectType.Apple:
                return new Apple();
            case MapObject.ObjectType.AppleTree:
                return new AppleTree();
            default:
                return null;
        }
    }
}
