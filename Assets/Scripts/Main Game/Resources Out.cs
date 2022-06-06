using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SCPResourcesManager : MonoBehaviour
{
    public TextMeshProUGUI ClDTXT;
    public TextMeshProUGUI ScTXT;
    public TextMeshProUGUI InfTXT;
    public TextMeshProUGUI SecrTXT;


    void Update()
    {
        ClDTXT.text = "ClassD: " + ResourcesManager.ClassD.ToString();
        ScTXT.text = "Science: " + ResourcesManager.Science.ToString();
        InfTXT.text = "Money: " + ResourcesManager.Money.ToString();
        SecrTXT.text = "Secrecy: " + ResourcesManager.Secrecy.ToString();
    }

}
