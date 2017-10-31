using UnityEngine;
using System.Collections;

public class DiscardCard : Task
{
    private float duration;
    private float timeElapsed;
    private Vector2 startPos;
    private Vector2 targetPos;
    private Card card;
    private Transform cardTransform;

    public DiscardCard(Card card_)
    {
        card = card_;
        cardTransform = card.controller.transform;
    }

    protected override void Init()
    {
        startPos = cardTransform.position;
        if (card is DungeonCard)
        {
            targetPos = Services.UIManager.dungeonDiscardZone.transform.position;
        }
        else
        {
            targetPos = Services.UIManager.discardZone.transform.position;
        }
        timeElapsed = 0;
        duration = Services.CardConfig.DiscardAnimDur;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        cardTransform.position = Vector2.Lerp(startPos, targetPos,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        card.DestroyPhysicalCard();
    }
}
