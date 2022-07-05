using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] tracks;
    public TextMeshProUGUI clipTitleText;
    public GameObject panel;
    private int currentTrack;
    public AudioSource source;

    void Start()
    {
        panel.SetActive(false);
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
        //clipTitleText.text = source.clip.name;
    }

    public void OpenClosePanel()
    {
        bool isActive = panel.activeSelf;
        panel.SetActive(!isActive);
    }
}
