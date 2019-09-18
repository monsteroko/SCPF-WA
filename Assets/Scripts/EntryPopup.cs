using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EntryPopup : MonoBehaviour {

    public float timer;
    public Text nameText;
    public Text classText;
    public Text descriptionText;
    public Button okButton;
    public Button cancButton;

    public GameObject entryPopupObject;

    private static EntryPopup entryPopup;

    private void Start() {
        entryPopup = FindObjectOfType(typeof(EntryPopup)) as EntryPopup;
        entryPopupObject.SetActive(false);
    }

    public static EntryPopup Instance() {
        return entryPopup;
    }

    public void OpenWithEntry(EntryModel entry) {
        nameText.text = "SCP-" + entry.code + ": " + entry.name;
        classText.text = "Класс: " + entry.type;
        descriptionText.text = "Описание: " + entry.description;
        okButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(ClosePopup);
        cancButton.onClick.RemoveAllListeners();
        cancButton.onClick.AddListener(ClosePopup);
        entryPopupObject.SetActive(true);
        timer = 0;
        Time.timeScale = timer;
    }

    void ClosePopup() {
        timer = 1f;
        Time.timeScale = timer;
        entryPopupObject.SetActive(false);
    }
}
