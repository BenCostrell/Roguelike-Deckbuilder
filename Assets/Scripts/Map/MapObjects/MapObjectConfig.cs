using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Map Object Config")]
public class MapObjectConfig : ScriptableObject
{
    [SerializeField]
    private MapObjectInfo[] mapObjects;
    public MapObjectInfo[] MapObjects { get { return mapObjects; } }

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
            default:
                return null;
        }
    }
}
