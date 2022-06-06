using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System;

public class CreateZoneDialog : MonoBehaviour {
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI Message;
    public GameObject createZoneDialogObject;
    private static CreateZoneDialog createZoneDialog;

    void Start() {
        createZoneDialog = FindObjectOfType(typeof(CreateZoneDialog)) as CreateZoneDialog;
        createZoneDialogObject.SetActive(false);
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);
    }

    public static CreateZoneDialog Instance() {
        return createZoneDialog;
    }

    public void Open() {
        createZoneDialogObject.SetActive(true);
    }

    void ClosePopup() {
        createZoneDialogObject.SetActive(false);
    }

    void Confirm() {
        if (ResourcesManager.Money >= 1000000)
        {
            GameManager.instance.zonesManager.BuildSelectedZone();
            ResourcesManager.Money -= 1000000;
            ClosePopup();
        }
        else
        {
            Message.text = "You don't have enough money!";
        }
    }

    void Cancel() {
        Message.text = "Are you want to build base?";
        ClosePopup();
    }
}
