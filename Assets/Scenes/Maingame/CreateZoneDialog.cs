using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public delegate void CreateZoneDialogDelegate(bool confirmed);

public class CreateZoneDialog : MonoBehaviour
{
    public Button yesButton;
    public Button noButton;

    public GameObject createZoneDialogObject;
    private static CreateZoneDialog createZoneDialog;

    void Start()
    {
        createZoneDialog = FindObjectOfType(typeof(CreateZoneDialog)) as CreateZoneDialog;
        createZoneDialogObject.SetActive(false);
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);
    }

    public static CreateZoneDialog Instance()
    {
        return createZoneDialog;
    }

    private CreateZoneDialogDelegate currentCompletion;
    public void Open(CreateZoneDialogDelegate completion)
    {
        currentCompletion = completion;
        createZoneDialogObject.SetActive(true);
    }

    void ClosePopup()
    {
        createZoneDialogObject.SetActive(false);
    }

    void Confirm()
    {
        currentCompletion(true);
        ClosePopup();
    }

    void Cancel()
    {
        currentCompletion(false);
        ClosePopup();
    }
}
