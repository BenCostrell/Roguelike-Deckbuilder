using UnityEngine;
using System.Collections;

public class DrawCardTask : Task
{
    private float timeElapsed;
    private float duration;
    private float timeToMidpoint;
    private Card card;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 midpointPos;
    private Vector3 initialScale;
    private Vector3 zoomScale;

    public DrawCardTask(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.DrawAnimDur;
        timeToMidpoint = Services.CardConfig.DrawAnimTimeToMidpoint;
        targetPos = Services.UIManager.GetCardHandPosition(
            Services.GameManager.player.hand.Count);
        card.CreatePhysicalCard(Services.UIManager.handZone.transform);
        initialScale = card.controller.transform.localScale;
        zoomScale = Services.CardConfig.DrawAnimScale * initialScale;
        Vector3 rawDiff = Services.UIManager.deckZone.transform.position - 
            Services.UIManager.handZone.transform.position;
        initialPos = new Vector3(rawDiff.x, rawDiff.y, targetPos.z);
        midpointPos = initialPos + Services.CardConfig.DrawAnimMidpointOffset;
        card.Reposition(initialPos, false);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        Vector3 pos;

        if (timeElapsed <= timeToMidpoint)
        {
            pos = Vector3.Lerp(initialPos, midpointPos,
            Easing.QuadEaseOut(timeElapsed / timeToMidpoint));
            card.controller.transform.localScale = Vector3.Lerp(initialScale, zoomScale,
                Easing.QuadEaseOut(timeElapsed / timeToMidpoint));
        }
        else
        {
            pos = Vector3.Lerp(midpointPos, targetPos,
                Easing.QuadEaseOut((timeElapsed - timeToMidpoint) / 
                (duration - timeToMidpoint)));
            card.controller.transform.localScale = Vector3.Lerp(zoomScale, initialScale,
                Easing.QuadEaseOut((timeElapsed - timeToMidpoint) /
                (duration - timeToMidpoint)));
        }
       
        card.Reposition(pos, false);

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        card.Reposition(targetPos, true);
        card.Enable();
        Services.GameManager.player.hand.Add(card);
    }

}
