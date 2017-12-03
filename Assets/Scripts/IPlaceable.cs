using UnityEngine;
using System.Collections;

public interface IPlaceable
{
    void PlaceOnTile(Tile tile);

    GameObject GetPhysicalObject();

    void CreatePhysicalObject(Tile tile);

    SpriteRenderer GetSpriteRenderer();

    Tile GetCurrentTile();
}
