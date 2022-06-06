using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ResourcesManager : MonoBehaviour
{

    public static int ClassD;
    public static double Science;
    public static double Money;
    public static double Secrecy;
    public TextMeshProUGUI ClassDText;
    public TextMeshProUGUI ScienceText;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI SecrecyText;

    void Start()
    {
        Secrecy = 100;
        Money = 20000000;
        Science = 0;
        ClassD = 0;
    }
    void Update()
    {
        addValues();
        GetValues();
    }


    public bool ResourcesChange(int CD, double Sc, double M, double Se)
    {
        if (ClassD - CD < 0)
            return false;
        if (Science - Sc < 0)
            return false;
        if (Money - M < 0)
            return false;
        if (Secrecy - Se < 0)
            return false;
        ClassD -= CD;
        Science -= Sc;
        Money -= M;
        Secrecy -= Se;
        return true;
    }

    private void addValues()
    {
        if (Secrecy > 0)
            Secrecy -= TimeManager.timer * UnityEngine.Random.Range(0.0001f, 0.00002f);
        if (ClassD < 10000)
            ClassD += (int)TimeManager.timer * UnityEngine.Random.Range(1, 2);
        if (Money < 20000000)
            Money += TimeManager.timer * UnityEngine.Random.Range(200f, 500f);

    }

    private void GetValues()
    {
        ClassDText.text = "Class D: "+ ClassD.ToString();
        ScienceText.text = "Science: "+ Math.Round(Science).ToString();
        MoneyText.text = "Money:" + Math.Round(Money).ToString();
        SecrecyText.text ="Secrecy: "+ Math.Round(Secrecy).ToString();
    }
}
