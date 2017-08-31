using UnityEngine;
using System.Collections;

public class AttackAnimation : Task
{
    private GameObject attacker;
    private GameObject target;
    private float timeElapsed;
    private float attackDuration;
    private float recoveryDuration;
    private Vector3 basePos;
    private Vector3 targetPos;

    public AttackAnimation(GameObject attacker_, GameObject target_)
    {
        attacker = attacker_;
        target = target_;
    }

    protected override void Init()
    {
        basePos = attacker.transform.position;
        attackDuration = Services.MonsterConfig.AttackAnimTime;
        recoveryDuration = Services.MonsterConfig.RecoveryAnimTime;
        timeElapsed = 0;
        targetPos = basePos + ((target.transform.position - basePos)
            * Services.MonsterConfig.AttackAnimDist);
    }

    internal override void Update()
    {
        timeElapsed += Time.deltaTime;

        if(timeElapsed <= attackDuration)
        {
            attacker.transform.position = Vector3.Lerp(basePos, targetPos,
                Easing.BackEaseIn(timeElapsed / attackDuration));
        }
        else
        {
            attacker.transform.position = Vector3.Lerp(targetPos, basePos,
                Easing.ExpoEaseOut((timeElapsed - attackDuration) / recoveryDuration));
        }

        if (timeElapsed >= attackDuration + recoveryDuration) SetStatus(TaskStatus.Success);
    }

    protected override void OnSuccess()
    {
        Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.MonsterHitAudio, 0.1f);
    }
}
