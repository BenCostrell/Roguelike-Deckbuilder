using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollMessageBanner : Task {

    private float duration;
    private float timeElapsed;
    private readonly string bannerMessage;
    
    public ScrollMessageBanner(string message)
    {
        bannerMessage = message;
    }

    protected override void Init()
    {
        Services.UIManager.SetMessageBanner(bannerMessage);
        timeElapsed = 0;
        duration = Services.UIManager.bannerScrollDuration;
        Services.UIManager.MoveMessageBanner(1600 * Vector2.left);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed < duration / 2)
        {
            Services.UIManager.MoveMessageBanner(Vector2.Lerp(
                1600 * Vector2.left, Vector2.zero,
                Easing.ExpoEaseOut(timeElapsed / (duration / 2))));
        }
        else
        {
            Services.UIManager.MoveMessageBanner(Vector2.Lerp(
                 Vector2.zero, 1600 * Vector2.right,
                Easing.ExpoEaseIn((timeElapsed - (duration / 2)) / (duration / 2))));
        }

        if (timeElapsed >= duration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        Services.UIManager.TurnOffMessageBanner();
    }
}
