using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MovingText : MonoBehaviour
{
    public GameObject Panel;
    public Text movingText;
    string[] text;
    int v = 0;
    // Start is called before the first frame update
    void Start()
    {
        using (StreamReader sr = new StreamReader(@"Assets/Scenes/Maingame/movingText.txt"))
        {
            text = sr.ReadToEnd().Split(';');
        }
        TextGen();
    }

    void TextGen()
    {

        movingText.text = text[Random.Range(0, text.Length)];
        
        //movingText.transform.Translate(Vector3.left * Time.deltaTime * 10);
        
    }
    

    // Update is called once per frame

    void Update()
    {
        if (v >= 2000)
        {
           Panel.transform.Translate(2000, 0, 0);
            v = 0;
        }
        Panel.transform.Translate(Vector3.left);
        v++;

    }
}
