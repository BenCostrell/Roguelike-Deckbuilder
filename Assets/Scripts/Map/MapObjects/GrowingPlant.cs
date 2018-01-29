using UnityEngine;
using System.Collections;

public abstract class GrowingPlant : DamageableObject
{
    protected int growthTime;
    protected int growthStage;

    protected override void InitValues()
    {
        base.InitValues();
        GrowingPlantInfo plantInfo = info as GrowingPlantInfo;
        growthTime = plantInfo.GrowthTime;
    }

    public override void PlaceOnTile(Tile tile)
    {
        base.PlaceOnTile(tile);
        Services.MapManager.AddGrowingPlant(this);
    }

    public virtual void Grow()
    {
        if (growthStage < growthTime)
        {
            growthStage += 1;
            SetSprite(growthStage);
            if(growthStage == growthTime)
            {
                OnFullyGrown();
            }
        }
    }

    protected virtual void OnFullyGrown()
    {

    }

    public override void RemoveThis(bool animate)
    {
        Services.MapManager.RemoveGrowingPlant(this);
        base.RemoveThis(animate);
    }
}
