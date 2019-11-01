using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using WPMF;
using TMPro;

public class EntryPopup : MonoBehaviour {

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI classText;
    public TextMeshProUGUI descriptionText;
    public Button okButton;
    public Button cancButton;
    public GameObject Cube;
    float x, y;
    public GameManager gameManager;
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
        gameManager = GameManager.instance;
        MapManager mapManager = gameManager.mapManager;
        Vector2 point = mapManager.GeneratePointInUnlockedAreas();
        map = WorldMap2D.instance;
        Vector3 ucoord = map.transform.TransformPoint(point);
        Cube.transform.position = ucoord;
        map.FlyToLocation(point, 1);
    }
}
