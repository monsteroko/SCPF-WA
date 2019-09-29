using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public class SCPFoundScript : MonoBehaviour
{
    public GameObject SCPobj;
    public WorldMap2D map;

    void OnMouseDown()
    {
        map = WorldMap2D.instance;
        SCPobj.SetActive(false);
        map.CenterMap();
    }
}
