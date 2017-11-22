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
    [SerializeField]
    private int selectionLockOutPeriod;
    private int selectionLockoutFramesLeft;

	// Update is called once per frame
	void Update () {
        if (selectionLockoutFramesLeft > 0) selectionLockoutFramesLeft -= 1;
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
        Services.EventManager.Fire(new TileHovered(tile));
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
        selectionLockoutFramesLeft = selectionLockOutPeriod;
    }

    private void OnMouseUp()
    {
        if (selectionLockoutFramesLeft == 0)
        {
            tile.OnSelect();
            selectionLockoutFramesLeft = selectionLockOutPeriod;
        }
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

    public void ShowAsPotentiallyAvailable()
    {
        defColor = (Color.blue + Color.white) / 2;
        sr.color = defColor;
    }

    public void ShowAsTargetable(bool validTarget)
    {
        if (validTarget)
        {
            defColor = Color.red;
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
