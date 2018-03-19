using UnityEngine;
using System.Collections;

public class DestroyCard : Task
{
    private Card card;
    private const float duration = 0.5f;
    private float timeElapsed;

    public DestroyCard(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        card.controller.SetTotalColor(Color.Lerp(Color.white, new Color(1, 1, 1, 0),
            Easing.QuadEaseOut(timeElapsed / duration)));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Aborted);
    }

    protected override void OnAbort()
    {
        card.DestroyPhysicalCard();
    }
}
