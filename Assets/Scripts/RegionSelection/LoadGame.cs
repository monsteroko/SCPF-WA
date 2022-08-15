using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGame : MonoBehaviour
{
    public GameObject canvasChoice;
    public GameObject canvasLoad;
    public TextMeshProUGUI tipsText;

    private string[] text;

    void Start()
    {
        string txtpath = Application.streamingAssetsPath + "/Data/Other/tips.txt";
        using (StreamReader sr = new StreamReader(txtpath))
        {
            text = sr.ReadToEnd().Split(';');
        }
        tipsText.text = text[Random.Range(0, text.Length)];
    }

    public void StartGame()
    {
        canvasChoice.SetActive(false);
        canvasLoad.SetActive(true);
        SceneManager.LoadScene("Game");
    }
}
