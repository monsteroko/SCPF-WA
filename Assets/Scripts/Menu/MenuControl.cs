using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    public Sprite[] frames; 
    public Image background;

    void Start()
    {
        InvokeRepeating("ImageChange", 30.0f, 30.0f);
    }
    public void PlayPressed()
    {
        SceneManager.LoadScene("CounterpartsSelection");
    }
    public void ExitPressed()
    {
        Application.Quit();
    }
    public void LinkPressed()
    {
        Application.OpenURL("http://scpfoundation.net/");
    }
    public void ImageChange()
    {
        background.sprite = frames[(int)Random.Range(0,frames.Length-1)];
    }
}
