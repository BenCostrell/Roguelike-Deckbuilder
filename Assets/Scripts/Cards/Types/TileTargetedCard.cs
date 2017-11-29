using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileTargetedCard : Card
{
    public int range { get; protected set; }
    protected List<Tile> targets;
    protected int numRequiredTargets;

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
        List<Tile> tilesInRange = TilesInRange();
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsTargetable(IsTargetValid(tile));
        }
    }

    public override void OnUnselect()
    {
        List<Tile> tilesInRange = TilesInRange();
        foreach (Tile tile in tilesInRange)
        {
            tile.controller.ShowAsUntargetable();
        }
        player.ShowAvailableMoves();
        ClearTargets();
    }

    protected List<Tile> TilesInRange()
    {
        return AStarSearch.FindAllAvailableGoals(Services.GameManager.player.currentTile,
            range, true);
    }
}
