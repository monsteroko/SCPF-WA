using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    public Canvas Canvas;

    string lname;
    private void Start()
    {
        //Canvas.gameObject.SetActive(false);
        lname = SceneManager.GetActiveScene().name;
        if (lname == "Technologies")
        {
            SceneManager.LoadScene("Menu");
        }
    }

    public void PlayPressed()
    {
        SceneManager.LoadScene("CounterpartsSelection");
    }
    public void ExitPressed()
    {
        Application.Quit();
    }
}
