﻿using UnityEngine;
using System.Collections;

public class PlayCardTask : Task
{
    private float timeElapsed;
    private float duration;
    private Card card;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 initialScale;
    private Vector3 targetScale;

    public PlayCardTask(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.PlayAnimDur;
        card.controller.sr.color = Color.white;
        card.controller.transform.parent = Services.UIManager.inPlayZone.transform;
        initialPos = card.controller.transform.localPosition;
        targetPos = Services.UIManager
            .GetInPlayCardPosition(Services.GameManager.player.cardsInPlay.Count);
        initialScale = card.controller.transform.localScale;
        targetScale = card.controller.baseScale;
        card.Disable();
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        card.Reposition(Vector3.Lerp(initialPos, targetPos,
            Easing.QuadEaseOut(timeElapsed / duration)), false);
        card.controller.transform.localScale = Vector3.Lerp(initialScale, targetScale,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        card.OnPlay();
        card.Reposition(targetPos, true);
    }
}