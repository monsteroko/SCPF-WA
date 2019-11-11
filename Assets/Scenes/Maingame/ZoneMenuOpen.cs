using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoneMenuOpen : MonoBehaviour
{
    public GameObject ZoneManagement;
    private int Current = 0, Limit = 0, v = 100, width = 235;
    private bool trigger = false;

    void Start()
    {
        ZoneManagement = GameObject.Find("ZoneManagenent");
    }

    void Update()
    {
        if (Current < Limit && Limit == width)
        {
            ZoneManagement.transform.Translate(v * Vector3.left);
            Current += v;
        }

        if (Current > Limit && Limit == 0)
        {
            ZoneManagement.transform.Translate(v * Vector3.right);
            Current -= v;
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
