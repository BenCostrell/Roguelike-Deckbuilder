using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager {

    private AudioSource musicSource;

    public void CreateAndPlayAudio(AudioClip clip)
    {
        GameObject audioObj = new GameObject();
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();
        GameObject.Destroy(audioObj, clip.length);
    }

    public void CreateAndPlayAudio(AudioClip clip, float volume)
    {
        GameObject audioObj = new GameObject();
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        GameObject.Destroy(audioObj, clip.length);
    }

    public void PlayMusic(AudioClip clip, float volume)
    {
        if(musicSource == null)
        {
            musicSource = new GameObject().AddComponent<AudioSource>();
        }
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
        musicSource.loop = true;
    }

    public void SetMusicVolume(float volume)
    {
        Services.GameManager.StopCoroutine(FadeInMusic(volume));
        Services.GameManager.StartCoroutine(FadeInMusic(volume));
    }

    IEnumerator FadeInMusic(float targetVolume)
    {
        float baseVolume = musicSource.volume;
        float timeElapsed = 0;
        while(Mathf.Abs(musicSource.volume - targetVolume) > 0.01f)
        {
            musicSource.volume = Mathf.Lerp(baseVolume, targetVolume,
                Easing.QuadEaseIn(timeElapsed / Services.AudioConfig.MusicFadeInTime));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}

