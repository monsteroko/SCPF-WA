using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MogSCPFoundPanel : MonoBehaviour
{
    public GameObject CarPrefab;
    public Button yesButton;
    public Button noButton;

    private GameObject SCPMogCanv;
    private GameObject Base;
    private GameObject SCP;
    private GameObject Car;

    void Start()
    {
        SCPMogCanv = GameObject.Find("MogCanvas");
        yesButton.onClick.AddListener(Confirm);
        noButton.onClick.AddListener(Cancel);
    }

    void ClosePopup()
    {
        SCPMogCanv.GetComponent<Canvas>().enabled = false;
    }

    void Confirm()
    {
        Base = GameObject.FindGameObjectWithTag("Base");
        SCP = GameObject.FindGameObjectWithTag("SCP");
        Instantiate(CarPrefab, new Vector3(Base.transform.position.x, Base.transform.position.y, 0), new Quaternion(0, 0, 0, 0));
        Car = GameObject.FindGameObjectWithTag("Car");
        ClosePopup();
        while (Car.transform.position != SCP.transform.position)
            Car.transform.position = Vector3.MoveTowards(Car.transform.position, SCP.transform.position, 0.0005f);
       Destroy(SCP);
        while (Car.transform.position != Base.transform.position)
            Car.transform.position = Vector3.MoveTowards(Car.transform.position, Base.transform.position, 0.0005f);
        Destroy(Car);
    }

    void Cancel()
    {
        ClosePopup();
    }
}
