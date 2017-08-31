using UnityEngine;
using System.Collections;

public class TileTargetSelection : Task
{
    private TileTargetedCard card;
    private GameObject targetReticle;
    private int lockID;

    public TileTargetSelection(TileTargetedCard card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        lockID = Services.UIManager.nextLockID;
        Services.EventManager.Register<TileSelected>(OnSelection);
        Services.GameManager.player.LockEverything(lockID);
        Services.GameManager.player.targeting = true;
        targetReticle = 
            GameObject.Instantiate(Services.Prefabs.TargetReticle, Services.Main.transform);
        card.OnSelect();
    }

    internal override void Update()
    {
        Vector3 mousePos = 
            Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
        targetReticle.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
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
        Services.GameManager.player.UnlockEverything(lockID);
        Services.GameManager.player.targeting = false;
        GameObject.Destroy(targetReticle);
        card.OnUnselect();
    }
}
