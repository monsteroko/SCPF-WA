using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeScr : MonoBehaviour
{
    public Text timetxt;
    public Text dttxt;
    private DateTime dt;
    public Canvas cv;
    public static float timer;
    public GameObject entryPopupObject;
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

    
    void Start()
    {
        cv.gameObject.SetActive(true);
        dt = new DateTime(1890,7,12,12,0,0);
    }
    void Update()
    {
        if (entryPopupObject.activeSelf==true)
            Time.timeScale = 0;
        else
            Time.timeScale = timer;
        dt = dt.AddSeconds(Time.timeScale);
        string s = dt.ToLongTimeString();
        string sd = dt.ToLongDateString();
        timetxt.text = s;
        dttxt.text = sd;

        
    }
}
