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
        eventClassD = UnityEngine.Random.Range(-10, 10);
        eventMoney = UnityEngine.Random.Range(0, 10000000);
        eventScience = UnityEngine.Random.Range(0,0);
        eventSecrecy = UnityEngine.Random.Range(0, 10);
        nameText.text = "SCP Found!";
        descriptionText.text = "Description: " + fdtext[entry.scpcategory,entry.randscpcat] + "\n" + "Resources needed: " + "ClassD: " + eventClassD.ToString() + ", Money: " + eventMoney.ToString() + ", Science: " + eventScience.ToString() + ", Secrecy: " + eventSecrecy.ToString();
        onwind.sprite = foundimg[entry.scpcategory];
        okButton.onClick.AddListener(ClosePopup);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(ClosePopup);
        cancButton.onClick.RemoveAllListeners();
        cancButton.onClick.AddListener(ClosePopup);
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
        else
        {
            descriptionText.text = "You don't have enough resources!" + "\n" + "Resources needed: " + "ClassD: " + eventClassD.ToString() + ", Money: " + eventMoney.ToString() + ", Science: " + eventScience.ToString() + ", Secrecy: " + eventSecrecy.ToString();
        }
    }

    private void SpawnSCP(Vector2 coordinates)
    {
        Instantiate(SCP, new Vector3(coordinates.x + UnityEngine.Random.Range(-0.4f, 0.4f), coordinates.y+UnityEngine.Random.Range(-0.3f, 0.3f), 0), new Quaternion(0,0,0,0));
    }
}
