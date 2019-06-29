using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class TimeScr : MonoBehaviour
{
    public Text timetxt;
    public Text dttxt;
    private DateTime dt;
    public Canvas cv;
    public void Play()
    {
        Time.timeScale = 1.0f;
    }

    public void Stop()
    {
        Time.timeScale = 0.0f;
    }
    public void TwoPl()
    {
        Time.timeScale = 2.0f;
    }
    public void FourPl()
    {
        Time.timeScale = 4.0f;
    }
    void Start()
    {
        cv.gameObject.SetActive(true);
        dt = new DateTime(1890,7,12,12,0,0);
    }
    void Update()
    {
        dt = dt.AddSeconds(Time.timeScale);
        string s = dt.ToLongTimeString();
        string sd = dt.ToLongDateString();
        timetxt.text = s;
        dttxt.text = sd;
    }
}
