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
    string[] text;

    void Start()
    {
        using (StreamReader sr = new StreamReader(@"Assets/Scenes/Maingame/movingText.txt"))
        {
            text = sr.ReadToEnd().Split(';');
        }
        StartCoroutine(TextGen());
    }

    IEnumerator TextGen()
    {
        while (true)
        {
            movingText.transform.localPosition = new Vector3(1920, 0);
            movingText.text = text[Random.Range(0, text.Length)];
            StartCoroutine(MovePanel());
            yield return new WaitForSecondsRealtime(60f);
        }

    }
    IEnumerator MovePanel()
    {
        for (int i = 1280; i >= 0; i--)
        {
            movingText.transform.Translate(Vector3.left);
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
