using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public class SCPFoundScript : MonoBehaviour
{
    public GameObject SCPMogCanv;
    public WorldMap2D map;

    void OnMouseDown()
    {
        if(SCPMogCanv.active==false)
            SCPMogCanv.SetActive(true);
        else
            SCPMogCanv.SetActive(false);
    }
}
