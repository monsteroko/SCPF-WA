using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCPFoundScript : MonoBehaviour
{
    public GameObject SCPMogCanv;

    void OnMouseDown()
    {
        if(SCPMogCanv.activeSelf==false)
            SCPMogCanv.SetActive(true);
        else
            SCPMogCanv.SetActive(false);
    }
}
