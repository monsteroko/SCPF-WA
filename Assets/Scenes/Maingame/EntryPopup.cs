using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
using System.IO;
using System.Text;

public class EntryPopup : MonoBehaviour {

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button okButton;
    public Button cancButton;
    public GameObject SCP;
    public GameObject entryPopupObject;
    int ran = 0;
    private static EntryPopup entryPopup;
    public Sprite[] foundimg = new Sprite[10];
    public Image onwind;
    public string[,] fdtext= new string[10,10];//текст при нахождении объекта
    private float x, y;
    private void Start()
    {
        entryPopup = FindObjectOfType(typeof(EntryPopup)) as EntryPopup;
        entryPopupObject.SetActive(false);
    }
    public static EntryPopup Instance()
    {
        return entryPopup;
    }

    public void OpenWithEntry(EntryModel entry)
    {
        using (StreamReader sr = new StreamReader(@"Assets/Scenes/Maingame/DescrText.txt", Encoding.Default))
        {
            for (int i = 0; i < 9; i++)
                for(int j = 0; j < 9; j++)
                    fdtext[i,j] = sr.ReadLine();
        }
        ran = UnityEngine.Random.Range(0, 9);
        entry.randscpcat = ran;
        nameText.text = "Салтовские учоные сообщили о говне, произошел троленг!";
        descriptionText.text = "Описание: " + fdtext[entry.scpcategory,entry.randscpcat];
        onwind.sprite = foundimg[entry.scpcategory];
        okButton.onClick.AddListener(ClosePopup);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(ClosePopup);
        cancButton.onClick.RemoveAllListeners();
        cancButton.onClick.AddListener(ClosePopup);
        entryPopupObject.SetActive(true);
    }

    void ClosePopup()
    {
        entryPopupObject.SetActive(false);
    }
    public void OnButtonDown()
    {
        MapManager mapManager = GameManager.instance.mapManager;
        Vector2 point = mapManager.GeneratePointInUnlockedAreas();
        Vector3 ucoord = mapManager.transform.TransformPoint(point);
        SCP.transform.position = ucoord;
        mapManager.map.FocusCameraOn(point, 0.05f);
    }
}
