using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MogSCPFoundPanel : MonoBehaviour
{
    public GameObject SCPMogCanv;
    public GameObject Baza;
    public GameObject SCP;
    public GameObject Car;
    public Button yesButton;
    public Button noButton;
    private bool resofch;
    public static int nbsofSCPs = 0;
    // Start is called before the first frame update
    void Start()
    {
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);
    }

    // Update is called once per frame
    void Update()
    {
        if (resofch)
        {
            Car.transform.position = Vector3.MoveTowards(Baza.transform.position, SCP.transform.position, 1.0f);
            if(Car.transform.position == SCP.transform.position)
            {
                Car.transform.position = Vector3.MoveTowards(SCP.transform.position, Baza.transform.position, 1.0f);
                SCP.SetActive(false);
            }
            if((Car.transform.position == Baza.transform.position)&&(SCP.activeSelf==false))
                Car.SetActive(false);

        }   
    }

    void ClosePopup()
    {
        SCPMogCanv.SetActive(false);
    }

    void Confirm()
    {
        nbsofSCPs++;
        Baza = GameObject.FindGameObjectWithTag("Base");
        Car.SetActive(true);
        resofch = true;
        ClosePopup();
    }

    void Cancel()
    {
        Baza = GameObject.FindGameObjectWithTag("Base");
        resofch = false;
        Car.SetActive(false);
        ClosePopup();
    }
}
