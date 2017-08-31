using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Attack Card Info")]
public class AttackCardInfo : TargetedCardInfo
{
    [SerializeField]
    private int damage;
    public int Damage { get { return damage; } }

    [SerializeField]
    private AudioClip onHitAudio;
    public AudioClip OnHitAudio { get { return onHitAudio; } }
}
