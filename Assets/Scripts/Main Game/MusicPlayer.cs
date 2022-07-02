using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] tracks;
    public TextMeshProUGUI clipTitleText;
    private int currentTrack;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (source.isPlaying)
            return;
        currentTrack--;
        if (currentTrack < 0)
            currentTrack = tracks.Length - 1;
        ShowCurrentTitle();
        StartCoroutine(WaitForMusicEnd());
    }

    IEnumerator WaitForMusicEnd()
    {
        while(source.isPlaying)
            yield return null;
        NextTitle();
    }

    public void NextTitle()
    {
        source.Stop();
        currentTrack++;
        if (currentTrack > tracks.Length - 1)
            currentTrack = 0;
        source.clip = tracks[currentTrack];
        source.Play();
        ShowCurrentTitle();
        StartCoroutine(WaitForMusicEnd());
    }

    public void PreviousTitle()
    {
        source.Stop();
        currentTrack--;
        if (currentTrack < 0)
            currentTrack = tracks.Length - 1;
        source.clip = tracks[currentTrack];
        source.Play();
        ShowCurrentTitle();
        StartCoroutine(WaitForMusicEnd());
    }

    public void StopMusic()
    {
        StopCoroutine("WaitForMusicEnd");
        source.Stop();
    }

    void ShowCurrentTitle()
    {
        clipTitleText.text = source.clip.name;
    }
}
