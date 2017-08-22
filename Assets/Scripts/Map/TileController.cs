﻿using System.Collections;
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
    }

    private void OnMouseExit()
    {
        tile.OnHoverExit();
        sr.color = defColor;
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
