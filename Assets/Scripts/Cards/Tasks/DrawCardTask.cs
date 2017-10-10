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
    private int deckSizeAtTimeOfDraw;
    private int discardSizeAtTimeOfDraw;
    private readonly bool playerDeck;

    public DrawCardTask(Card card_, int deckSize, int discardSize, bool playerDeck_)
    {
        card = card_;
        deckSizeAtTimeOfDraw = deckSize;
        discardSizeAtTimeOfDraw = discardSize;
        playerDeck = playerDeck_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.DrawAnimDur;
        timeToMidpoint = Services.CardConfig.DrawAnimTimeToMidpoint;
        Vector3 rawDiff;
        if (playerDeck)
        {
            Services.UIManager.UpdateDeckCounter(deckSizeAtTimeOfDraw);
            Services.UIManager.UpdateDiscardCounter(discardSizeAtTimeOfDraw);
            targetPos = Services.UIManager.GetCardHandPosition(
                Services.GameManager.player.hand.Count);
            rawDiff = Services.UIManager.deckZone.transform.position -
                Services.UIManager.handZone.transform.position;
            card.CreatePhysicalCard(Services.UIManager.handZone.transform);
            card.Disable();
        }
        else
        {
            Services.UIManager.UpdateDungeonDeckCounter(deckSizeAtTimeOfDraw);
            Services.UIManager.UpdateDungeonDiscardCounter(discardSizeAtTimeOfDraw);
            targetPos = Services.UIManager.GetCardHandPosition(
                Services.Main.dungeonDeck.playedCards.Count);
            rawDiff = Services.UIManager.dungeonDeckZone.transform.position -
                Services.UIManager.dungeonPlayZone.transform.position;
            card.CreatePhysicalCard(Services.UIManager.dungeonPlayZone.transform);
        }
        
        initialScale = card.controller.transform.localScale;
        zoomScale = Services.CardConfig.DrawAnimScale * initialScale;
        initialPos = new Vector3(rawDiff.x, rawDiff.y, targetPos.z);
        midpointPos = initialPos + Services.CardConfig.DrawAnimMidpointOffset;
        card.Reposition(initialPos, false, true);
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardDrawAudio);
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
       
        card.Reposition(pos, false, true);

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        card.Reposition(targetPos, true);
        if (playerDeck)
        {
            Services.GameManager.player.cardsInFlux.Remove(card);
            Services.GameManager.player.hand.Add(card);
        }
        else
        {
            Services.Main.dungeonDeck.cardsInFlux.Remove(card);
            Services.Main.dungeonDeck.playedCards.Add(card);
        }
    }

}
