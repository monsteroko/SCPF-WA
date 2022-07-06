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
    public GameObject SCPs;
    public GameObject Parameters;
    public GameObject Employees;

    public Button SCPsButton;
    public Button ParaButton;
    public Button EmpButton;

    public TextMeshProUGUI zoneNameText;
    public RectTransform ZoneManagementObject;
    private static ZoneMenu zoneMenu;
    public Animation anim;
    private bool trigger = false, isMoving = false;

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
        isMoving = false;
    }
    void MoveIn()
    {
        isMoving = true;
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
        BaseModel activeBase = GameManager.instance.zonesResourcesManager.GetBase(zoneName);
        zoneNameText.text = activeBase.name;
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
