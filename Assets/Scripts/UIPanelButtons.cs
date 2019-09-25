using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPanelButtons : MonoBehaviour
{
    public Button Tech;
    public Button Settings;

    public Canvas Canvas;

    public void TechPressed()
    {
        SceneManager.LoadScene("Technoligies");
        Canvas.gameObject.SetActive(false);
        Debug.Log("techloaded'");
    }

    public void SetiingsPressed()
    {
        SceneManager.LoadScene("Technoligies");
    }

    // Update is called once per frame
    void Update()
    {
        Tech.onClick.AddListener(TechPressed);
    }

    void Start()
    {

    }
}
