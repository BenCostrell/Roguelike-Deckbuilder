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
    private Quaternion targetRot;
    private readonly bool playerDeck;
    private TaskManager subTaskManager;
    private bool delayStart;

    public DrawCardTask(bool playerDeck_)
    {
        playerDeck = playerDeck_;
    }

    protected override void Init()
    {
        subTaskManager = new TaskManager();
        if (playerDeck && Services.GameManager.player.deckCount == 0)
        {
            subTaskManager.AddTask(new ReshuffleDeck(true));
            delayStart = true;
        }
        else if (!playerDeck && Services.Main.dungeonDeck.deckCount == 0)
        {
            subTaskManager.AddTask(new ReshuffleDeck(false));
            delayStart = true;
        }
        if (!delayStart) StartDraw();
   }

    void StartDraw()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.DrawAnimDur;
        timeToMidpoint = Services.CardConfig.DrawAnimTimeToMidpoint;
        Vector3 rawDiff;
        if (playerDeck)
        {
            card = Services.GameManager.player.GetRandomCardFromDeck();
            Services.UIManager.UpdateDeckCounter();
            Services.UIManager.UpdateDiscardCounter();
            targetPos = Services.UIManager.GetHandCardPosition(
                Services.GameManager.player.hand.Count,
                Services.GameManager.player.hand.Count);
            targetRot = Services.UIManager.GetHandCardRotation(
                Services.GameManager.player.hand.Count,
                Services.GameManager.player.hand.Count);
            rawDiff = Services.UIManager.deckZone.transform.position -
                Services.UIManager.handZone.transform.position;
            card.CreatePhysicalCard(Services.UIManager.handZone.transform);
        }
        else
        {
            card = Services.Main.dungeonDeck.GetRandomCardFromDeck();
            Services.UIManager.UpdateDungeonDeckCounter();
            Services.UIManager.UpdateDungeonDiscardCounter();
            targetPos = Services.UIManager.GetHandCardPosition(
                Services.Main.dungeonDeck.hand.Count,
                Services.Main.dungeonDeck.hand.Count);
            targetRot = Services.UIManager.GetHandCardRotation(
                Services.Main.dungeonDeck.hand.Count,
                Services.Main.dungeonDeck.hand.Count);
            rawDiff = Services.UIManager.dungeonDeckZone.transform.position -
                Services.UIManager.dungeonPlayZone.transform.position;
            card.CreatePhysicalCard(Services.UIManager.dungeonHandZone.transform);
        }
        initialScale = card.controller.transform.localScale;
        zoomScale = Services.CardConfig.DrawAnimScale * initialScale;
        initialPos = new Vector3(rawDiff.x, rawDiff.y, targetPos.z);
        if (playerDeck)
        {
            midpointPos = initialPos + Services.CardConfig.DrawAnimMidpointOffset;
        }
        else
        {
            midpointPos = initialPos + Services.CardConfig.DungeonDrawAnimMidpointOffset;
        }
        card.Reposition(initialPos, false, true);
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardDrawAudio);
        card.controller.EnterDrawingState();
        subTaskManager.AddTask(card.OnDraw());
    }

    internal override void Update()
    {
        subTaskManager.Update();
        if(subTaskManager.tasksInProcessCount == 0 && delayStart)
        {
            StartDraw();
            delayStart = false;
        }

        if (!delayStart)
        {
            if (timeElapsed < duration)
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
                    card.controller.transform.localRotation = Quaternion.Lerp(Quaternion.identity,
                        targetRot, Easing.QuadEaseOut((timeElapsed - timeToMidpoint) /
                        (duration - timeToMidpoint)));
                }

                card.Reposition(pos, false, true);
            }

            if (timeElapsed >= duration && subTaskManager.tasksInProcessCount == 0)
                SetStatus(TaskStatus.Success);
        }
    }

    protected override void OnSuccess()
    {
        card.Reposition(targetPos, true);
        if (playerDeck)
        {
            Services.GameManager.player.cardsInFlux.Remove(card);
            Services.GameManager.player.hand.Add(card);
            Services.UIManager.SortHand(Services.GameManager.player.hand);
        }
        else
        {
            Services.Main.dungeonDeck.cardsInFlux.Remove(card as DungeonCard);
            Services.Main.dungeonDeck.hand.Add(card as DungeonCard);
            Services.UIManager.SortHand(Services.Main.dungeonDeck.hand);
        }
    }

}
