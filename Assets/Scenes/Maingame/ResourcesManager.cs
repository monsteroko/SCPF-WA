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
    public int ClassD = 0;
    public double Science = 0;
    public double Influence = 0;
    public double Secrecy = 100;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        ClDTXT.text = "ClassD: " + ClassD.ToString();
        ScTXT.text = "Science: " + Science.ToString();
        InfTXT.text = "Influence: " + Influence.ToString();
        SecrTXT.text = "Secrecy: " + Secrecy.ToString();
    }
}
