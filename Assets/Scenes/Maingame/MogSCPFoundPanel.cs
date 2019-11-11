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
            Car.transform.position = Vector3.MoveTowards(Baza.transform.position, SCP.transform.position, Time.deltaTime);
            if(Car.transform.position == SCP.transform.position)
            {
                Car.transform.position = Vector3.MoveTowards(SCP.transform.position, Baza.transform.position, Time.deltaTime);
                SCP.SetActive(false);
            }
            if((Car.transform.position == Baza.transform.position)&&(SCP.active==false))
                Car.SetActive(false);

        }   
    }

    void ClosePopup()
    {
        SCPMogCanv.SetActive(false);
    }

    void Confirm()
    {
        Baza = GameObject.Find("Base(Clone)");
        Car.SetActive(true);
        resofch = true;
        ClosePopup();
    }

    void Cancel()
    {
        Baza = GameObject.Find("Base(Clone)");
        resofch = false;
        Car.SetActive(false);
        ClosePopup();
    }
}
