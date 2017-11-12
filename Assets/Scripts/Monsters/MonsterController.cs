using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{
    public Monster monster { get; private set; }
    [SerializeField]
    private GameObject healthUIobj;
    private TextMesh healthUI;
    public SpriteRenderer sr { get; private set; }
    private SpriteMask mask;
    public SpriteRenderer maskSprite { get; private set; }

    public void Init(Monster monster_)
    {
        monster = monster_;
        healthUI = healthUIobj.GetComponent<TextMesh>();
        sr = GetComponent<SpriteRenderer>();
        MeshRenderer mr = healthUIobj.GetComponent<MeshRenderer>();
        mr.sortingLayerID = sr.sortingLayerID;
        mr.sortingOrder = sr.sortingOrder;
        mask = GetComponentInChildren<SpriteMask>();
        maskSprite = mask.gameObject.GetComponent<SpriteRenderer>();
        mask.sprite = monster.info.Sprite;
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
