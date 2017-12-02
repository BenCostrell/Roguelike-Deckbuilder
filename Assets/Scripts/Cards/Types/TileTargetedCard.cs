using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileTargetedCard : Card
{
    public int range { get; protected set; }
    protected List<Tile> targets;
    protected int numRequiredTargets;
    protected List<Tile> currentTileRange;

    public virtual void ClearTargets()
    {
        targets = new List<Tile>();
        numRequiredTargets = 1;
    }

    public override bool CanPlay()
    {
        List<Tile> tilesWithinRange = TilesInRange();
        for (int i = 0; i < tilesWithinRange.Count; i++)
        {
            Tile tile = tilesWithinRange[i];
            if (IsTargetValid(tile)) return true;
        }
        return false;
    }

    public bool SelectionComplete()
    {
        return targets.Count == numRequiredTargets;
    }

    public override void OnPlay()
    {
        base.OnPlay();
    }

    public virtual bool IsTargetValid(Tile tile)
    {
        return tile.coord.Distance(player.currentTile.coord) <= range;
    }

    public virtual void OnTargetSelected(Tile tile)
    {
        targets.Add(tile);
    }

    protected override void InitValues()
    {
        base.InitValues();
        TargetedCardInfo targetedCardInfo = info as TargetedCardInfo;
        range = targetedCardInfo.Range;
    }

    public override void OnSelect()
    {
        ClearTargets();
        currentTileRange = TilesInRange();
        foreach (Tile tile in currentTileRange)
        {
            tile.controller.ShowAsTargetable(IsTargetValid(tile));
        }
    }

    public override void OnUnselect()
    {
        UnhighlightTilesInRange();
        player.ShowAvailableMoves();
        //ClearTargets();
    }

    protected void UnhighlightTilesInRange()
    {
        foreach (Tile tile in currentTileRange)
        {
            tile.controller.ShowAsUntargetable();
        }
    }

    protected List<Tile> TilesInRange()
    {
        return AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, true);
    }
}
