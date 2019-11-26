using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneMenu : MonoBehaviour
{
    public GameObject ZoneManagementObject;
    private static ZoneMenu zoneMenu;
    private int Current = 0, Limit = 0, v = 50, width = (int)(Screen.width / 3.3f);
    private bool trigger = false;

    void Start()
    {
        ZoneManagementObject = GameObject.Find("ZoneManagenent");
        zoneMenu = FindObjectOfType(typeof(ZoneMenu)) as ZoneMenu;
    }

    public static ZoneMenu Instance() {
        return zoneMenu;
    }

    void Update()
    {
        if (Current < Limit && Limit == width)
        {
            if (Limit - Current >= v)
            {
                ZoneManagementObject.transform.Translate(v * Vector3.left);
                Current += v;
            }
            else
            {
                ZoneManagementObject.transform.Translate((Limit - Current) * Vector3.left);
                Current = Limit;
            }
        }

        if (Current > Limit && Limit == 0)
        {
            if (Current >= v)
            {
                ZoneManagementObject.transform.Translate(v * Vector3.right);
                Current -= v;
            }
            else
            {
                ZoneManagementObject.transform.Translate(Current * Vector3.right);
                Current = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Current > 0 && !EventSystem.current.IsPointerOverGameObject())
            {
                Limit = 0;
            }
            else
            {
                if (Current == 0 && trigger)
                {
                    Limit = width;
                    trigger = false;
                }
            }
        }
    }

    public void Open()
    {
        trigger = true;
    }
}
