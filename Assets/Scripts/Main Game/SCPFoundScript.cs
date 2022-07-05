using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPFoundScript : MonoBehaviour
{
    public GameObject SCPMogCanv;
    void Start()
    {
        SCPMogCanv = GameObject.Find("MogCanvas");
    }
    void OnMouseDown()
    {
        if(SCPMogCanv.GetComponent<Canvas>().enabled == false)
            SCPMogCanv.GetComponent<Canvas>().enabled = true;
        else
            SCPMogCanv.GetComponent<Canvas>().enabled = false;
    }
}
