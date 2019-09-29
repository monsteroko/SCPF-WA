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
        gameManager = GameObject.Find("GameManagerObject").GetComponent<GameManager>();
        MapManager mapManager = gameManager.mapManager;
        map = WorldMap2D.instance;
        Region region = mapManager.unlockedRegions[0].regions[0];
        Rect rect = region.rect2D;
        x = rect.xMin + (float)UnityEngine.Random.Range(0, 100) / 100.0f * rect.xMax;
        y = rect.yMin + (float)UnityEngine.Random.Range(0, 100) / 100.0f * rect.yMax;
        Debug.Log(x);
        Debug.Log(y);
        Vector3 ucoord = map.transform.TransformPoint(new Vector2(x, y));
        Debug.Log(ucoord);
        Cube.transform.position = ucoord;
        Vector3 SCPCoord = new Vector3(x, y, 99);
        map.FlyToLocation(x,y,1);
    }
}
