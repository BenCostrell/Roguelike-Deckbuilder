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
    private RectTransform rect;
    private TaskManager subTaskManager;

    public PlayDungeonCard(DungeonCard card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.PlayAnimDur;
        rect = card.controller.GetComponent<RectTransform>();
        card.controller.transform.SetParent(Services.UIManager.dungeonPlayZone.transform);
        initialPos = rect.anchoredPosition;
        targetPos = Services.UIManager
            .GetInPlayCardPosition(Services.Main.dungeonDeck.playedCards.Count + 1);
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardPlayAudio, 0.3f);
        subTaskManager = new TaskManager();
        subTaskManager.AddTask(card.DungeonOnPlay());
    }

    internal override void Update()
    {
        if (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            card.Reposition(Vector3.Lerp(initialPos, targetPos,
                Easing.QuadEaseOut(timeElapsed / duration)), false, true);
        }

        Debug.Log("playing card " + card.GetType() + " at time " + Time.time);

        subTaskManager.Update();

        if (timeElapsed >= duration && subTaskManager.tasksInProcessCount == 0)
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
