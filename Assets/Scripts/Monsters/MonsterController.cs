using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    private Monster monster;

    public void Init(Monster monster_)
    {
        monster = monster_;
    }

    public void PlaceOnTile(Tile tile)
    {
        transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
    }

}
