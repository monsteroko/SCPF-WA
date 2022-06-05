using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class ResourcesManager : MonoBehaviour
{
    public TextMeshProUGUI ClDTXT;
    public TextMeshProUGUI ScTXT;
    public TextMeshProUGUI InfTXT;
    public TextMeshProUGUI SecrTXT;
    private int counter = 0;
    public static int ClassDInc = 0;


    void Update()
    {
        if(Time.timeScale == 4f)
        {
            counter++;
            if((counter% 512) == 0)
                GameManager.ClassD += ClassDInc;
        }
        if (Time.timeScale == 16f)
        {
            counter++;
            if ((counter % 256) == 0)
                GameManager.ClassD += ClassDInc;
        }
        if (Time.timeScale == 32f)
        {
            counter++;
            if ((counter % 64) == 0)
                GameManager.ClassD += ClassDInc;
        }
        ClDTXT.text = "ClassD: " + GameManager.ClassD.ToString();
        ScTXT.text = "Science: " + GameManager.Science.ToString();
        InfTXT.text = "Money: " + GameManager.Money.ToString();
        SecrTXT.text = "Secrecy: " + GameManager.Secrecy.ToString();
    }
}
