using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    public Monster monster { get; private set; }
    [SerializeField]
    private GameObject healthUIobj;
    private TextMesh healthUI;
    private SpriteRenderer sr;

    public void Init(Monster monster_)
    {
        monster = monster_;
        healthUI = healthUIobj.GetComponent<TextMesh>();
        sr = GetComponent<SpriteRenderer>();
        MeshRenderer mr = healthUIobj.GetComponent<MeshRenderer>();
        mr.sortingLayerID = sr.sortingLayerID;
        mr.sortingOrder = sr.sortingOrder;
    }

    public void PlaceOnTile(Tile tile)
    {
        transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
    }

    public void UpdateHealthUI()
    {
        healthUI.text = monster.currentHealth + "/" + monster.maxHealth;
    }

}
