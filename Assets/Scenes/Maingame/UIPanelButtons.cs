using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPanelButtons : MonoBehaviour
{
    public Button tech;
    public Button menu;

    public void TechPressed()
    {
        SceneManager.LoadScene("Tech", LoadSceneMode.Additive);
    }

    public void SetiingsPressed()
    {

    }


    void Start()
    {
        tech.onClick.AddListener(TechPressed);
    }
    
}
