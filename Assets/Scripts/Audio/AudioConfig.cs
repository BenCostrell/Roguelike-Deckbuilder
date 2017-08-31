using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio Config")]
public class AudioConfig : ScriptableObject {
    [SerializeField]
    private AudioClip cardDrawAudio;
    public AudioClip CardDrawAudio { get { return cardDrawAudio; } }

    [SerializeField]
    private AudioClip cardPlayAudio;
    public AudioClip CardPlayAudio { get { return cardPlayAudio; } }

    [SerializeField]
    private AudioClip monsterHitAudio;
    public AudioClip MonsterHitAudio { get { return monsterHitAudio; } }
}
