using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ResourcesManager : MonoBehaviour
{
    /// <summary>
    /// Total amount of Class D 
    /// </summary>
    public static int ClassD = 0;
    /// <summary>
    /// Total amount of science
    /// </summary>
    public static double Science = 0;
    /// <summary>
    /// Total amount of money
    /// </summary>
    public static double Money = 20000000;
    /// <summary>
    /// Amount of secrecy
    /// </summary>
    public static double Secrecy = 100;
    /// <summary>
    /// Complexity, less - harder
    /// </summary>
    private static double levelCoefficient = 1.0f;
    public double LevelCoefficient
    {
        get { return levelCoefficient; }
    }
    public TextMeshProUGUI ClassDText;
    public TextMeshProUGUI ScienceText;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI SecrecyText;

    void Start()
    {
        InvokeRepeating("AddValues", 0, 1f);
        InvokeRepeating("GetValues", 0, 1f);
    }
    /// <summary>
    /// Check if resources can be changed, if can, change
    /// </summary>
    /// <param name="CD">Class D</param>
    /// <param name="Sc">Science</param>
    /// <param name="M">Money</param>
    /// <param name="Se">Secrecy</param>
    /// <returns></returns>
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
    /// <summary>
    /// Add values every time interval
    /// </summary>
    private void AddValues()
    {
        if (Secrecy > 0)
            Secrecy -= TimeManager.timer * LevelCoefficient * UnityEngine.Random.Range(0.0001f, 0.00002f) * GameManager.instance.zonesManager.CountofBases;
        if (ClassD < 10000)
            ClassD += (int)(TimeManager.timer * LevelCoefficient * UnityEngine.Random.Range(1, 2) * GameManager.instance.zonesManager.CountofBases); 
        if (Money < 20000000)
            Money += TimeManager.timer * LevelCoefficient * UnityEngine.Random.Range(200f, 500f) * GameManager.instance.zonesManager.CountofBases;

    }
    /// <summary>
    /// Update info about resources
    /// </summary>
    public void GetValues()
    {
        ClassDText.text = "Class D: "+ ClassD.ToString();
        ScienceText.text = "Science: "+ Math.Round(Science).ToString();
        MoneyText.text = "Money:" + Math.Round(Money).ToString();
        SecrecyText.text ="Secrecy: "+ Math.Round(Secrecy).ToString();
    }
}
