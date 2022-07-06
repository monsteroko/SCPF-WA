using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CreateZoneDialog : MonoBehaviour {
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI Message;
    public GameObject createZoneDialogObject;

    public int defaultCostOfBase = 10000000;

    private int actualBaseCost;
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
        GameManager.instance.timeManager.Pause();
        actualBaseCost = (int)(defaultCostOfBase / GameManager.instance.resourcesManager.LevelCoefficient * (1 + 0.25 * GameManager.instance.zonesManager.CountofBases));
        createZoneDialogObject.SetActive(true);
        Message.text = "Are you want to build base here? \n It cost " + Convert.ToString(actualBaseCost);
    }

    void ClosePopup() {
        GameManager.instance.timeManager.UnPause();
        createZoneDialogObject.SetActive(false);
    }

    void Confirm() {
        actualBaseCost = (int)(defaultCostOfBase / GameManager.instance.resourcesManager.LevelCoefficient * (1 + 0.1 * GameManager.instance.zonesManager.CountofBases));
        if (ResourcesManager.Money >= actualBaseCost)
        {
            GameManager.instance.zonesManager.BuildSelectedZone();
            ResourcesManager.Money -= actualBaseCost;
            GameManager.instance.resourcesManager.GetValues();
            ClosePopup();
        }
        else
        {
            Message.text = "You don't have enough money!";
        }
    }

    void Cancel() {
        ClosePopup();
    }
}
