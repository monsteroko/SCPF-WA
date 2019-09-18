using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ZoneMenuOpen : MonoBehaviour
{
    public GameObject ZoneManagement;
    int Current = 0, Limit = 0, v = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Current < Limit && Limit == 300)
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
        Debug.Log("Gavno!");
        if (Current == 0)
        {
            Limit = 300;
        }

        if (Current == 300)
        {
            Limit = 0;
        }
        
        
    }
}
