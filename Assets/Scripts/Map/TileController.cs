using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileController : MonoBehaviour {

    private Tile tile;
    private SpriteRenderer sr;
    [SerializeField]
    private Color onHoverColor;
    private Color defColor;
    [SerializeField]
    private float detectionDistance;

	// Update is called once per frame
	void Update () {
    }

    public void Init(Tile tile_)
    {
        tile = tile_;
        transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
        sr = GetComponent<SpriteRenderer>();
        defColor = sr.color;
    }

    private void OnMouseOver()
    {
        if (Vector2.Distance(transform.position,
            Services.GameManager.player.controller.transform.position) < detectionDistance)
        {
            tile.OnHoverEnter();
        }
    }

    private void OnMouseEnter()
    {
        if (Vector2.Distance(transform.position,
            Services.GameManager.player.controller.transform.position) < detectionDistance)
        {
            sr.color = onHoverColor;
            if (tile.containedMonster != null)
            {
                Services.UIManager.ShowUnitUI(tile.containedMonster);
            }
            if (tile.containedMapObject != null)
            {
                Services.UIManager.ShowMapObjUI(tile.containedMapObject);
            }
        }
    }

    private void OnMouseExit()
    {
        tile.OnHoverExit();
        sr.color = defColor;
        Services.UIManager.HideUnitUI();
        Services.UIManager.HideMapObjUI();
    }

    private void OnMouseDown()
    {
        tile.OnSelect();
    }

    public void ShowAsAvailable()
    {
        defColor = Color.green;
        sr.color = defColor;
    }

    public void ShowAsUnavailable()
    {
        defColor = Color.white;
        sr.color = defColor;
    }

    public void ShowAsTargetable(bool validTarget)
    {
        if (validTarget)
        {
            defColor = (Color.blue + Color.white) / 2;
        }
        else
        {
            defColor = (Color.red + Color.white) / 2;
        }
        sr.color = defColor;
    }

    public void ShowAsUntargetable()
    {
        defColor = Color.white;
        sr.color = defColor;
    }
}
