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
    public GameObject entryPopupObject;
    private int eventClassD, eventMoney, eventScience, eventSecrecy;
    public GameObject SCP;
    int ran = 0;
    private static EntryPopup entryPopup;
    private static ResourcesManager resManager;
    public Sprite[] foundimg = new Sprite[10];
    public Image onwind;
    public string[,] fdtext= new string[10,10];
    private float x, y;
    private void Start()
    {
        entryPopup = FindObjectOfType(typeof(EntryPopup)) as EntryPopup;
        resManager = FindObjectOfType(typeof(ResourcesManager)) as ResourcesManager;
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
        nameText.text = "SCP Found!";
        descriptionText.text = "Описание: " + fdtext[entry.scpcategory,entry.randscpcat];
        onwind.sprite = foundimg[entry.scpcategory];
        okButton.onClick.AddListener(ClosePopup);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(ClosePopup);
        cancButton.onClick.RemoveAllListeners();
        cancButton.onClick.AddListener(ClosePopup);
        eventClassD = UnityEngine.Random.Range(-10,10);
        eventMoney = UnityEngine.Random.Range(0, 10000000);
        eventScience = UnityEngine.Random.Range(-10, 10);
        eventSecrecy = UnityEngine.Random.Range(0, 10);
        entryPopupObject.SetActive(true);
        GameManager.instance.timeManager.PauseRealTime();
        GameManager.instance.timeManager.Stop();
    }

    void ClosePopup()
    {
        entryPopupObject.SetActive(false);
        GameManager.instance.timeManager.UnpauseRealTime();
        GameManager.instance.timeManager.Play();
    }
    public void FocusOnSCP()
    {
        if(resManager.ResourcesChange(eventClassD, eventScience, eventMoney, eventSecrecy))
        {
            GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
            GameObject baserand = bases[UnityEngine.Random.Range(0, bases.Length - 1)];
            SpawnSCP(baserand.transform.position);
            GameManager.instance.timeManager.UnpauseRealTime();
            GameManager.instance.timeManager.Play();
        }
    }

    private void SpawnSCP(Vector2 coordinates)
    {
        Instantiate(SCP, new Vector3(coordinates.x + UnityEngine.Random.Range(-0.3f, 0.3f), coordinates.y+UnityEngine.Random.Range(-0.3f, 0.3f), 0), new Quaternion(270,0,0,0));
    }
}
