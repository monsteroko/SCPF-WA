using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ZoneMenuOpen : MonoBehaviour
{
    public GameObject ZoneManagement;
    private int Current = 0, Limit = 0, v = 100, width = 280;

    // Update is called once per frame
    void Update()
    {
        if (Current < Limit && Limit == width)
        {
            ZoneManagement.transform.Translate(v * -1, 0, 0);
            Current += v;
        }

        if (Current > Limit && Limit == 0)
        {
            ZoneManagement.transform.Translate(v * 1, 0, 0);
            Current -= v;
        }
    }

    void OnMouseDown()
    {
        if (Current == 0)
        {
            Limit = width;
        }

        if (Current > 0)
        {
            Limit = 0;
        }              
    }
}
