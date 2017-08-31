using UnityEngine;
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

    public PlayCardTask(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.PlayAnimDur;
        card.controller.color = Color.white;
        card.controller.transform.parent = Services.UIManager.inPlayZone.transform;
        initialPos = card.controller.transform.localPosition;
        Services.GameManager.player.hand.Remove(card);
        Services.GameManager.player.cardsInFlux.Add(card);
        Services.UIManager.SortHand(Services.GameManager.player.hand);
        targetPos = Services.UIManager
            .GetInPlayCardPosition(Services.GameManager.player.cardsInPlay.Count + 1);
        initialScale = card.controller.transform.localScale;
        targetScale = card.controller.baseScale;
        card.Disable();
        lockID = Services.UIManager.nextLockID;
        Services.GameManager.player.LockEverything(lockID);
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardPlayAudio);
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
        Services.GameManager.player.cardsInFlux.Remove(card);
        Services.GameManager.player.cardsInPlay.Add(card);
        card.OnPlay();
        card.Reposition(targetPos, true);
        Services.GameManager.player.UnlockEverything(lockID);
    }
}
