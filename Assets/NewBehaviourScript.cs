using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    public float timer;

    public void MenuPressed()
    {
        SceneManager.LoadScene("Menu");

    }
    public void NextPR()
    {
        timer = 1f;
    }
    public void ExitPressed()
    {
        Application.Quit();
        Debug.Log("Exit pressed!");
    }
    public void IcoPR()
    {
        timer = 0;
    }
    void Update()
    {
        Time.timeScale = timer;
    }
}
