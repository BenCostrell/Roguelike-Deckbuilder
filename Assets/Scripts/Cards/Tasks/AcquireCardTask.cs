using UnityEngine;
using System.Collections;

public class AcquireCardTask : Task
{
    private float timeElapsed;
    private float duration;
    private Card card;
    private Vector3 initialPos;
    private Vector3 targetPos;
    private Vector3 initialScale;
    private Vector3 targetScale;

    public AcquireCardTask(Card card_)
    {
        card = card_;
    }

    protected override void Init()
    {
        timeElapsed = 0;
        duration = Services.CardConfig.AcquireAnimDur;
        card.controller.transform.SetParent(Services.UIManager.bottomCorner);
        initialPos = card.controller.transform.position;
        targetPos = Services.UIManager.discardZone.transform.position;
        initialScale = card.controller.transform.localScale;
        targetScale = Vector3.zero;
        //card.GetPickedUp();
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        card.Reposition(Vector3.Lerp(initialPos, targetPos, 
            Easing.QuartEaseIn(timeElapsed / duration)), false, true);
        card.controller.transform.localScale = Vector3.Lerp(initialScale, targetScale,
            Easing.QuartEaseIn(timeElapsed / duration));

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        card.DestroyPhysicalCard();
    }
}
