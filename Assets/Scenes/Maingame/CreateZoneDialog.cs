using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class CreateZoneDialog : MonoBehaviour {
    public Button yesButton;
    public Button noButton;

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
        GameManager.instance.zonesManager.BuildSelectedZone();
        ClosePopup();
    }

    void Cancel() {
        ClosePopup();
    }
}
