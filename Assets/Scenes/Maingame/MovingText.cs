using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MovingText : MonoBehaviour
{
    public GameObject panel;
    public Text movingText;
    string[] text;
    int v = 0;

    void Start()
    {
        TextGen();
    }

    void TextGen()
    {
        using (StreamReader sr = new StreamReader(@"Assets/Scenes/Maingame/movingText.txt"))
        {
            text = sr.ReadToEnd().Split(';');
        }
        movingText.text = text[Random.Range(0, text.Length)];        
    }
    
    void Update()
    {
        if (v >= 1900)
        {
            panel.transform.Translate(1900, 0, 0);
            v = 0;
            TextGen();
        }
        panel.transform.Translate(Vector3.left);
        v++;

    }
}
