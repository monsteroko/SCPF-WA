using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
using System.IO;
using System.Text;

/// <summary>
/// Popup when SCPs founded
/// </summary>
public class EntryPopup : MonoBehaviour {

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Button okButton;
    public Button cancButton;
    public GameObject entryPopupObject;
    public GameObject SCP;
    public Sprite[] foundimg = new Sprite[10];
    public Image onwind;

    /// <summary>
    /// Array of SCPs descriptions
    /// </summary>
    private string[] desctext;
    /// <summary>
    /// Resources of event
    /// </summary>
    private int eventClassD, eventMoney, eventScience, eventSecrecy;
    /// <summary>
    /// Coordinates of closest base
    /// </summary>
    private Vector2 baseCoordinates;
    private static EntryPopup entryPopup;
    private static ResourcesManager resManager;

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

    /// <summary>
    /// Open popup window
    /// </summary>
    /// <param name="entry">Finded SCP</param>
    public void OpenWithEntry(EntryModel entry)
    {
        GameObject[] bases = GameObject.FindGameObjectsWithTag("Base");
        GameObject baserand = bases[UnityEngine.Random.Range(0, bases.Length - 1)];
        baseCoordinates = baserand.transform.position;
        string SubPath = Path.Combine("Assets", "Data", "Entries", "Descriptions");
        if(File.Exists(SubPath + "/" + entry.scpcategory.ToString() + ".txt"))
            desctext = File.ReadAllLines(SubPath + "/" + entry.scpcategory.ToString()+ ".txt");
        else
            desctext = File.ReadAllLines(SubPath + "/-1.txt");
        double correction= (1/(GameManager.instance.resourcesManager.LevelCoefficient))/(1+(0.1f*GameManager.instance.zonesManager.CountofBases));
        eventClassD = (int)(UnityEngine.Random.Range(-10, 10)* correction);
        eventMoney = (int)(UnityEngine.Random.Range(0, 10000000) * correction);
        eventScience = (int)(UnityEngine.Random.Range(0,0) * correction);
        eventSecrecy = (int)(UnityEngine.Random.Range(0, 10) * correction);
        string descstring = desctext[UnityEngine.Random.Range(0, desctext.Length - 1)];
        nameText.text = "SCP Found!";
        descriptionText.text = "Description: " + GenerateDescription(descstring, baseCoordinates) + "\n" + "Resources needed: " + "Class D: " + eventClassD.ToString() + ", Money: " + eventMoney.ToString() + ", Science: " + eventScience.ToString() + ", Secrecy: " + eventSecrecy.ToString();
        onwind.sprite = foundimg[entry.scpcategory];
        okButton.onClick.AddListener(ClosePopup);
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(ClosePopup);
        cancButton.onClick.RemoveAllListeners();
        cancButton.onClick.AddListener(ClosePopup);
        GameManager.instance.timeManager.Pause();
        entryPopupObject.SetActive(true);
    }

    void ClosePopup()
    {
        entryPopupObject.SetActive(false);
        GameManager.instance.timeManager.UnPause();
    }
    /// <summary>
    /// Accept event and spawn SCP
    /// </summary>
    public void FocusOnSCP()
    {
        if(resManager.ResourcesChange(eventClassD, eventScience, eventMoney, eventSecrecy))
        {
            SpawnSCP(baseCoordinates);
            GameManager.instance.timeManager.UnpauseRealTime();
            GameManager.instance.timeManager.Play();
        }
        else
        {
            descriptionText.text = "You don't have enough resources!" + "\n" + "Resources needed: " + "ClassD: " + eventClassD.ToString() + ", Money: " + eventMoney.ToString() + ", Science: " + eventScience.ToString() + ", Secrecy: " + eventSecrecy.ToString();
        }
    }

    /// <summary>
    /// Spawn SCP object
    /// </summary>
    /// <param name="coordinates"></param>
    private void SpawnSCP(Vector2 coordinates)
    {
        Instantiate(SCP, new Vector3(coordinates.x + GenerateDevitation(0.2f, 0.4f), coordinates.y + GenerateDevitation(0.2f, 0.4f), 0), new Quaternion(0,0,0,0));
    }

    /// <summary>
    /// Make SCP description
    /// </summary>
    /// <param name="line">pattern of description</param>
    /// <param name="baseCoordinates">base coordinates</param>
    /// <returns></returns>
    private string GenerateDescription(string line, Vector2 baseCoordinates)
    {
        string desc = line;
        string[] names = File.ReadAllLines(@"Assets/Data/Other/names.txt");
        string regName = GameManager.instance.mapManager.onWhatRegion(baseCoordinates).name;
        string agentName = names[UnityEngine.Random.Range(0, names.Length - 1)];
        while ((desc.LastIndexOf('(') != -1) || (desc.LastIndexOf(')') != -1))
        {
            int index1 = desc.LastIndexOf('(');
            int index2 = desc.LastIndexOf(')');
            string substring = desc.Substring(index1 + 1, index2 - index1 - 1);
            desc = desc.Remove(index1, index2 - index1 + 1);
            string[] substrings = substring.Split(',');
            desc = desc.Insert(index1, substrings[UnityEngine.Random.Range(0, substrings.Length - 1)]);

        }
        desc = desc.Replace("&", regName);
        desc = desc.Replace("#", agentName);
        return desc;
    }

    private float GenerateDevitation(float f1, float f2)
    {
        float[] points = new float[2];
        points[0] = UnityEngine.Random.Range(f1,f2);
        points[1] = UnityEngine.Random.Range(-f1, -f2);
        float devitation = points[UnityEngine.Random.Range(0, points.Length - 1)];
        return devitation;
    }
}
