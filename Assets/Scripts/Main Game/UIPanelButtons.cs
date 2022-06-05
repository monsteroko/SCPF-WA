using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPanelButtons : MonoBehaviour
{
    public void TechPressed()
    {
        SceneManager.LoadScene("Tech", LoadSceneMode.Additive);
    }

    public void PausePressed()
    {
        SceneManager.LoadScene("Menu");
    }  
}
