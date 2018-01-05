using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayDungeonCard : Task
{
    private float timeElapsed;
    private float duration;
    private DungeonCard card;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 midpointPos;
    private Vector3 baseScale;
    private Vector3 midpointScale;
    private RectTransform rect;
    private TaskManager subTaskManager;
    private bool effectStarted;

    public PlayDungeonCard(DungeonCard card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.DungeonPlayAnimDur;
        rect = card.controller.GetComponent<RectTransform>();
        card.controller.transform.SetParent(Services.UIManager.dungeonPlayZone.transform);
        card.controller.EnterDungeonPlayingMode();
        initialPos = rect.anchoredPosition;
        midpointPos = Services.CardConfig.DungeonPlayAnimMidpointOffset;
        targetPos = Services.UIManager
            .GetInPlayCardPosition(Services.Main.dungeonDeck.playedCards.Count + 1);
        baseScale = card.controller.transform.localScale;
        midpointScale = baseScale * Services.CardConfig.DungeonPlayAnimScale;
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardPlayAudio, 0.3f);
        subTaskManager = new TaskManager();
        //Debug.Log("playing card " + card.GetType() + " at time " + Time.time);
    }

    internal override void Update()
    {
        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            if(timeElapsed < duration / 2)
            {
                card.Reposition(Vector3.Lerp(initialPos, midpointPos,
                Easing.ExpoEaseOut(timeElapsed / (duration/2))), false, true);
                card.controller.transform.localScale = Vector3.Lerp(baseScale, midpointScale,
                    Easing.ExpoEaseOut(timeElapsed / (duration / 2)));
            }
            else
            {
                card.Reposition(Vector3.Lerp(midpointPos, targetPos,
                Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2))), false, true);
                card.controller.transform.localScale = Vector3.Lerp(midpointScale, baseScale,
                    Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2)));
            }
        }
        else if (!effectStarted)
        {
            subTaskManager.AddTask(card.DungeonOnPlay());
            effectStarted = true;
        }

        subTaskManager.Update();

        if (timeElapsed >= duration && subTaskManager.tasksInProcessCount == 0 && effectStarted)
            SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        Services.Main.dungeonDeck.playedCards.Add(card);
        card.Reposition(targetPos, true);
        card.controller.EnterPlayedMode();
        Services.UIManager.SortInPlayZone(Services.Main.dungeonDeck.playedCards);
        card.controller.RotateTo(Quaternion.identity);
        card.controller.baseParent = card.controller.transform.parent;
    }
}
