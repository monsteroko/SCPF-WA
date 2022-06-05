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
    public TextMeshProUGUI zoneNameText;
    public string zoneName;
    public RectTransform ZoneManagementObject;
    private static ZoneMenu zoneMenu;
    public Animation anim;
    private bool trigger = false, isMoving = false;

    void Start()
    {
        zoneMenu = FindObjectOfType(typeof(ZoneMenu)) as ZoneMenu;
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

    public void RollOut()
    {
        
        zoneNameText.text = zoneName;
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
