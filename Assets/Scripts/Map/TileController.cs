using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour {

    private Tile tile;
    private SpriteRenderer sr;
    [SerializeField]
    private Color onHoverColor;
    private Color defColor;
	
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

    private void OnMouseEnter()
    {
        tile.OnHoverEnter();
        sr.color = onHoverColor;
        if (tile.containedMonster != null)
        {
            Services.UIManager.ShowUnitUI(
                tile.containedMonster.info.Name,
                tile.containedMonster.currentHealth,
                tile.containedMonster.maxHealth,
                tile.containedMonster.info.Sprite);
        }
        //if (tile.containedCard != null) tile.containedCard.controller.ShowBoardCardOnHover();
    }

    private void OnMouseExit()
    {
        tile.OnHoverExit();
        sr.color = defColor;
        Services.UIManager.HideUnitUI();
        //if (tile.containedCard != null) tile.containedCard.controller.DisplayCardOnBoard();
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

    public void ShowAsTargetable()
    {
        defColor = (Color.red + Color.white)/2;
        sr.color = defColor;
    }

    public void ShowAsUntargetable()
    {
        defColor = Color.white;
        sr.color = defColor;
    }
}
