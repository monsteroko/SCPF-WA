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
    public float timer;
    public void Play()
    {
        timer = 1f;
    }

    public void Stop()
    {
        timer = 0;
    }
    public void TwoPl()
    {
        timer = 2f;
    }
    public void FourPl()
    {
        timer = 4f;   
    }
    void Start()
    {
        cv.gameObject.SetActive(true);
        dt = new DateTime(1890,7,12,12,0,0);
    }
    void Update()
    {
        Time.timeScale = timer;
        dt = dt.AddSeconds(Time.timeScale);
        string s = dt.ToLongTimeString();
        string sd = dt.ToLongDateString();
        timetxt.text = s;
        dttxt.text = sd;
    }
}
