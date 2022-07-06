using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MogSCPFoundPanel : MonoBehaviour
{
    public GameObject CarPrefab;

    private GameObject SCPMogCanv;
    private GameObject Base;
    private GameObject SCP;
    private GameObject Car;

    void Start()
    {
        SCPMogCanv = GameObject.Find("MogCanvas");
    }

    void ClosePopup()
    {
        SCPMogCanv.GetComponent<Canvas>().enabled = false;
    }

    public void Confirm()
    {
        Base = GameObject.FindGameObjectWithTag("Base");
        SCP = GameObject.FindGameObjectWithTag("SCP");
        Instantiate(CarPrefab, new Vector3(Base.transform.position.x, Base.transform.position.y, 0), new Quaternion(0, 0, 0, 0));
        Car = GameObject.FindGameObjectWithTag("Car");
        ClosePopup();
        StartCoroutine("MoveCar");
    }

    public void Cancel()
    {
        ClosePopup();
    }

    IEnumerator MoveCar()
    {
        Car.transform.LookAt(SCP.transform);
        while (Car.transform.position != SCP.transform.position)
            Car.transform.position = Vector3.MoveTowards(Car.transform.position, SCP.transform.position, 0.0005f);
        Destroy(SCP);
        Car.transform.LookAt(Base.transform);
        while (Car.transform.position != Base.transform.position)
            Car.transform.position = Vector3.MoveTowards(Car.transform.position, Base.transform.position, 0.0005f);
        Destroy(Car);
        yield return null;
    }
}
