using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager {

    public void CreateAndPlayAudio(AudioClip clip)
    {
        GameObject audioObj = new GameObject();
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
        GameObject.Destroy(audioObj, clip.length);
    }
}

