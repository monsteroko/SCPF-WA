using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EntryPopup : MonoBehaviour {

    public Text nameText;
    public Text classText;
    public Text descriptionText;
    public Button okButton;
    public Button cancButton;
    public GameObject Cube;
    public int x, y;
    
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
        x = Random.Range(0, 200);
        y = Random.Range(-100, 0);
        Cube.transform.Translate(x, y,-2);
    }
}
