using UnityEngine;
using System.Collections;

public interface IPlaceable
{
    void PlaceOnTile(Tile tile);

    GameObject GetPhysicalObject();

    SpriteRenderer GetSpriteRenderer();
}
