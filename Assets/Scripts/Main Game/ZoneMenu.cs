using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ZoneMenu : MonoBehaviour
{
    public Sprite[] foundimg = new Sprite[10];
    public GameObject SCPs;
    public GameObject Parameters;
    public GameObject Employees;

    public Button SCPsButton;
    public Button ParaButton;
    public Button EmpButton;

    public Button UpgradeButton;

    public TextMeshProUGUI zoneNameText;
    public RectTransform ZoneManagementObject;

    public TextMeshProUGUI mogHiretext;
    public TextMeshProUGUI mogAmounttext;

    public TextMeshProUGUI classDHiretext;

    public TextMeshProUGUI ManagementText;

    public TextMeshProUGUI SCPUnknownText;
    public TextMeshProUGUI SCPText;
    public Image SCPImage;
    public Button SCPChoice;
    public TMP_Dropdown SCPDropdown;
    public Button SCPResearchButton;
    public ScrollRect SCPScrollrect;
    public Animation anim;

    private float buycoefficient;
    private static ZoneMenu zoneMenu;
    private bool trigger = false, isMoving = false;
    private BaseModel activeBase;
    void Start()
    {
        zoneMenu = FindObjectOfType(typeof(ZoneMenu)) as ZoneMenu;
        Employees.SetActive(false);
        SCPs.SetActive(false);
        Parameters.SetActive(true);
        SCPsButton.onClick.AddListener(SCPList);
        EmpButton.onClick.AddListener(Personnel);
        ParaButton.onClick.AddListener(BaseManagement);
    }

    public static ZoneMenu Instance() {
        return zoneMenu;
    }
    void MoveOut()
    {
        isMoving = true;
        anim.Play("Zone Management");
        InvokeRepeating("UpdateValues", 0, 3f);
        isMoving = false;
    }
    void MoveIn()
    {
        isMoving = true;
        CancelInvoke("UpdateValues");
        anim.Play("ZM");
        isMoving = false;
    }

    void Personnel()
    {
        Employees.SetActive(true);
        SCPs.SetActive(false);
        Parameters.SetActive(false);
    }

    void SCPList()
    {
        Employees.SetActive(false);
        SCPs.SetActive(true);
        Parameters.SetActive(false);
    }

    void BaseManagement()
    {
        Employees.SetActive(false);
        SCPs.SetActive(false);
        Parameters.SetActive(true);
    }
    public void RollOut(string zoneName)
    {
        if((zoneNameText.text!= zoneName)&&(trigger))
        {
            buycoefficient = (float)(1000 * GameManager.instance.resourcesManager.LevelCoefficient * (1 + (0.1 * GameManager.instance.zonesManager.CountofBases)));
            activeBase = GameManager.instance.zonesResourcesManager.GetBase(zoneName);
            zoneNameText.text = activeBase.name;
            UpdateValues();
            UpdateSCPData();
        }
        else
        {
            buycoefficient = (float)(1000 * GameManager.instance.resourcesManager.LevelCoefficient * (1 + (0.1 * GameManager.instance.zonesManager.CountofBases)));
            activeBase = GameManager.instance.zonesResourcesManager.GetBase(zoneName);
            zoneNameText.text = activeBase.name;
            UpdateValues();
            UpdateSCPData();
            if (Input.GetKeyDown(KeyCode.Mouse0) && !isMoving)
            {
                if (!trigger && !EventSystem.current.IsPointerOverGameObject())
                {
                    MoveOut();
                    trigger = true;
                }
                else
                {
                    if (trigger)
                    {
                        MoveIn();
                        trigger = false;
                    }
                }
            }
        }
        
    }
    public void RollOut()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isMoving)
        {
            if (!trigger && !EventSystem.current.IsPointerOverGameObject())
            {
                MoveOut();
                trigger = true;
            }
            else
            {
                if (trigger)
                {
                    MoveIn();
                    trigger = false;
                }
            }
        }
    }

    public void HireMog()
    {
        if((GameManager.instance.resourcesManager.ResourcesChange((int)(activeBase.amountofMog / 10 * buycoefficient), 0,0,0))&&(activeBase.amountofMog<=100))
        {
            activeBase.amountofMog += 10;
            GameManager.instance.zonesResourcesManager.UpdateBase(activeBase);
            mogHiretext.text = "Mog's hired!";
            GameManager.instance.resourcesManager.GetValues();
            mogAmounttext.text = "You have " + activeBase.amountofMog +"/100 MOG groups in this zone";
        }
        else
        {
            if(activeBase.amountofMog >= 100)
                mogHiretext.text = "You have maximum amount of MOG groups!";
            else
                mogHiretext.text = "You don't have enough Class D to do that!";
        }
    }

    void UpdateValues()
    {
        mogAmounttext.text = "You have " + activeBase.amountofMog + "/100 MOG groups in this zone";
        mogHiretext.text = "Hire 10 MOGs from" + (int)(activeBase.amountofMog/10* buycoefficient) + " Class D";
        classDHiretext.text= "Hire 100 class D for " + (int)(activeBase.amountofMog / 10 * buycoefficient)+ " money and " + (int)(activeBase.amountofMog / 100 * (buycoefficient/1000))+ " influence";
        if (activeBase.listofSCPs.Count != 0)
        {
            SCPDropdown.gameObject.SetActive(true);
            SCPChoice.gameObject.SetActive(true);
        }
        else
        {
            SCPDropdown.gameObject.SetActive(false);
            SCPChoice.gameObject.SetActive(false);
            SCPUnknownText.gameObject.SetActive(false);
            SCPScrollrect.gameObject.SetActive(false);
            SCPText.gameObject.SetActive(false);
            SCPImage.gameObject.SetActive(false);
            SCPResearchButton.gameObject.SetActive(false);
        }
    }
    public void HireClassD()
    {
        if (GameManager.instance.resourcesManager.ResourcesChange(-100, 0, (int)(activeBase.amountofMog / 10 * buycoefficient), (int)(activeBase.amountofMog / 100 * (buycoefficient/1000))))
        {
            classDHiretext.text = "Class D hired!";
            GameManager.instance.resourcesManager.GetValues();
        }
        else
        {
            classDHiretext.text = "You don't have enough resources D to do that!";
        }
    }
    public void UpgradeBase()
    {

    }

    public void SCPSelected()
    {
        SCPDropdown.gameObject.SetActive(false);
        SCPChoice.gameObject.SetActive(false);
        SCPUnknownText.gameObject.SetActive(false);
        SCPText.gameObject.SetActive(false);
        SCPScrollrect.gameObject.SetActive(false);
        SCPImage.gameObject.SetActive(false);
        SCPResearchButton.gameObject.SetActive(false);
        EntryModel CurrentSCP = GameManager.instance.entryManager.GetEntryByName(SCPDropdown.options[SCPDropdown.value].text);
        SCPDropdown.gameObject.SetActive(true);
        SCPChoice.gameObject.SetActive(true);
        if (CurrentSCP.isResearched)
        {
            SCPImage.sprite = foundimg[CurrentSCP.scpcategory];
            SCPImage.gameObject.SetActive(true);
            SCPText.text = "Name: " + CurrentSCP.name + " " + CurrentSCP.code + ", Description: " + CurrentSCP.description + "\n";
            SCPText.gameObject.SetActive(true);
            SCPScrollrect.gameObject.SetActive(true);
        }
        else
        {
            SCPUnknownText.text = CurrentSCP.descriptionInit;
            SCPUnknownText.gameObject.SetActive(true);
            SCPResearchButton.gameObject.SetActive(true);
        }
    }

    public void ResearchSCP()
    {
        EntryModel CurrentSCP = GameManager.instance.entryManager.GetEntryByName(SCPDropdown.options[SCPDropdown.value].text);
        CurrentSCP.isResearched = true;
        GameManager.instance.entryManager.UpdateEntry(CurrentSCP);
        SCPSelected();
    }
    void UpdateSCPData()
    {
        List<string> Scps = new List<string>();
        foreach (EntryModel entry in activeBase.listofSCPs)
        {
            Scps.Add(entry.name);
        }
        SCPDropdown.ClearOptions();
        SCPDropdown.AddOptions(Scps);
    }


}
