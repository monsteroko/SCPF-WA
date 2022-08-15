using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MovingText : MonoBehaviour
{
    public TextMeshProUGUI movingText;
    /// <summary>
    /// Array of messages
    /// </summary>
    private string[] text;

    void Start()
    {
        text= File.ReadAllLines(Application.streamingAssetsPath +"/Data/Other/movingText.txt", System.Text.Encoding.Default);
        StartCoroutine(TextGen());
    }
    /// <summary>
    /// Take text from array
    /// </summary>
    /// <returns></returns>
    IEnumerator TextGen()
    {
        while (true)
        {
            movingText.transform.localPosition = new Vector3(1920, 0);
            movingText.text = text[Random.Range(0, text.Length-1)];
            StartCoroutine(MovePanel());
            yield return new WaitForSecondsRealtime(60f);
        }

    }
    /// <summary>
    /// Move panel with text
    /// </summary>
    /// <returns></returns>
    IEnumerator MovePanel()
    {
        for (int i = 1340; i >= 0; i--)
        {
            movingText.transform.Translate(Vector3.left);
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
