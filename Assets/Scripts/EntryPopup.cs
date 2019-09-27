using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using WPMF;

public class EntryPopup : MonoBehaviour {

    public Text nameText;
    public Text classText;
    public Text descriptionText;
    public Button okButton;
    public Button cancButton;
    public GameObject Cube;
    public int x, y;
    public WorldMap2D map;

    public GameObject entryPopupObject;

    private static EntryPopup entryPopup;

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
        nameText.text = "SCP-" + entry.code + ": " + entry.name;
        classText.text = "Класс: " + entry.type;
        descriptionText.text = "Описание: " + entry.description;
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
        
        map = WorldMap2D.instance;
        x = UnityEngine.Random.Range(0, 200);
        y = UnityEngine.Random.Range(-100, 0);
        Cube.transform.Translate(x, y,-2);
        float a= Convert.ToSingle(x)-105;
        float b = Convert.ToSingle(y)+62;
        Vector3 SCPCoord = new Vector3(a, b, 99);
        //map.FlyToLocation(a,b,99);
    }
}
