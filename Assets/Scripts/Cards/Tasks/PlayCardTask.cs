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
    private int lockID;
    private RectTransform rect;

    public PlayCardTask(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.PlayAnimDur;
        rect = card.controller.GetComponent<RectTransform>();
        card.controller.color = Color.white;
        card.controller.transform.SetParent(Services.UIManager.inPlayZone.transform);
        initialPos = rect.anchoredPosition;
        Services.GameManager.player.hand.Remove(card);
        Services.GameManager.player.cardsInFlux.Add(card);
        Services.UIManager.SortHand(Services.GameManager.player.hand);
        targetPos = Services.UIManager
            .GetInPlayCardPosition(Services.GameManager.player.cardsInPlay.Count + 1);
        initialScale = card.controller.transform.localScale;
        targetScale = card.controller.baseScale;
        lockID = Services.UIManager.nextLockID;
        Services.GameManager.player.LockEverything(lockID);
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardPlayAudio, 0.3f);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        card.Reposition(Vector3.Lerp(initialPos, targetPos,
            Easing.QuadEaseOut(timeElapsed / duration)), false, true);
        card.controller.transform.localScale = Vector3.Lerp(initialScale, targetScale,
            Easing.QuadEaseOut(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        Services.GameManager.player.cardsInFlux.Remove(card);
        Services.GameManager.player.cardsInPlay.Add(card);
        card.OnPlay();
        card.Reposition(targetPos, true);
        Services.GameManager.player.UnlockEverything(lockID);
        Services.UIManager.SortInPlayZone(Services.GameManager.player.cardsInPlay);
    }
}
