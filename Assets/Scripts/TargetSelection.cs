﻿using UnityEngine;
using System.Collections;

public class TargetSelection : Task
{
    private TargetedCard card;
    private GameObject targetReticle;

    public TargetSelection(TargetedCard card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        Services.EventManager.Register<TileSelected>(OnSelection);
        Services.GameManager.player.DisableCardsWhileTargeting();
        targetReticle = 
            GameObject.Instantiate(Services.Prefabs.TargetReticle, Services.Main.transform);
    }

    internal override void Update()
    {
        Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        targetReticle.transform.position = new Vector3(
            mousePos.x,
            mousePos.y,
            0);
    }

    void OnSelection(TileSelected e)
    {
        Tile tileSelected = e.tile;
        if (card.IsTargetValid(tileSelected))
        {
            card.OnTargetSelected(tileSelected);
            SetStatus(TaskStatus.Success);
        }
    }

    protected override void OnSuccess()
    {
        Services.EventManager.Unregister<TileSelected>(OnSelection);
        Services.GameManager.player.ReenableCardsWhenDoneTargeting();
        GameObject.Destroy(targetReticle);
    }
}
