using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public TextMeshProUGUI DateTime;
    private DateTime dt;
    public static float timer;

    public void Play()
    {
        timer = 4f;
    }
    public void Stop()
    {
        timer = 0;
    }
    public void TwoPl()
    {
        timer = 16f;
    }
    public void FourPl()
    {
        timer = 32f;   
    }
    public float GetTime(float time)
    {
        if (timer != 0)
            return time * timer;
        else
            return -1;
    }
    public float GetSpeedforResources()
    {
         return timer;
    }
    public bool isOnPause()
    {
        if (timer == 0)
            return true;
        else
            return false;
    }
    public void PauseRealTime()
    {
        Time.timeScale = 0;
    }
    public void UnpauseRealTime()
    {
        Time.timeScale = 1f;
    }
    void Start()
    {
        dt = new DateTime(1890,7,12,12,0,0);
    }
    void Update()
    {
        dt = dt.AddSeconds(timer);
        DateTime.text = dt.ToShortTimeString() + " "+ dt.ToShortDateString(); 
    }

}
