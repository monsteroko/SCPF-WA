using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneMenuOpen : MonoBehaviour
{
    public GameObject ZoneManagement;
    private int Current = 0, Limit = 0, v = 50, width = (int)(Screen.width / 3.3f);
    private bool trigger = false;

    void Start()
    {
        ZoneManagement = GameObject.Find("ZoneManagenent");
    }

    void Update()
    {
        if (Current < Limit && Limit == width)
        {
            if (Limit - Current >= v)
            {
                ZoneManagement.transform.Translate(v * Vector3.left);
                Current += v;
            }
            else
            {
                ZoneManagement.transform.Translate((Limit - Current) * Vector3.left);
                Current = Limit;
            }
        }

        if (Current > Limit && Limit == 0)
        {
            if (Current >= v)
            {
                ZoneManagement.transform.Translate(v * Vector3.right);
                Current -= v;
            }
            else
            {
                ZoneManagement.transform.Translate(Current * Vector3.right);
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

    void OnMouseDown()
    {
        trigger = true;
    }
}
