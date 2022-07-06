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
    private static float pauseTimer=1f;

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
    /// <summary>
    /// Pause time (ex. for events)
    /// </summary>
    public void Pause()
    {
        pauseTimer = timer;
        timer = 0;
    }
    /// <summary>
    /// Unpause time
    /// </summary>
    public void UnPause()
    {
        timer = pauseTimer;
    }
    /// <summary>
    /// Get time with multiplier for events
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
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
        dt = new DateTime(1891,1,1,0,0,0);
        UnpauseRealTime();
    }
    void Update()
    {
        dt = dt.AddSeconds(timer);
        DateTime.text = dt.ToShortTimeString() + " "+ dt.ToShortDateString(); 
    }
    public DateTime GetTime()
    {
        return dt;
    }



}
