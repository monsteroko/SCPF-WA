using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MogSCPFoundPanel : MonoBehaviour
{
    public GameObject CarPrefab;
    public TextMeshProUGUI mogMsg;

    private GameObject SCPMogCanv;
    private GameObject[] Bases;
    private GameObject Base;
    private GameObject SCP;
    private GameObject Car;

    private static EntryModel model;
    private static MogSCPFoundPanel mogSCPFoundPanel;

    void Start()
    {
        mogSCPFoundPanel = FindObjectOfType(typeof(MogSCPFoundPanel)) as MogSCPFoundPanel;
        SCPMogCanv = GameObject.Find("MogCanvas");
    }

    public static MogSCPFoundPanel Instance()
    {
        return mogSCPFoundPanel;
    }
    public void OpenWithEntry(EntryModel entry)
    {
        model = entry;
        SCPMogCanv.GetComponent<Canvas>().enabled = true;
        mogMsg.text = "To catch this SCP you need " + model.grabcoef + " MOG squads. Wanna try?";
    }

    public void Open()
    {
        SCPMogCanv.GetComponent<Canvas>().enabled = true;
    }
    void ClosePopup()
    {
        SCPMogCanv.GetComponent<Canvas>().enabled = false;
    }

    public void Confirm()
    {
        Bases = GameObject.FindGameObjectsWithTag("Base");
        Base = FindClosestBase(Bases);
        string s = Base.name.Substring(5);
        BaseModel baza = GameManager.instance.zonesResourcesManager.GetBase(s);
        if (baza.amountofMog >= model.grabcoef)
        {
            baza.listofSCPs.Add(model);
            baza.amountofMog-=model.grabcoef;
            SCP = GameObject.FindGameObjectWithTag("SCP");
            Instantiate(CarPrefab, new Vector3(Base.transform.position.x, Base.transform.position.y, 0), new Quaternion(0, 0, 0, 0));
            Car = GameObject.FindGameObjectWithTag("Car");
            GameManager.instance.zonesResourcesManager.UpdateBase(baza);
            model.isCatched = true;
            GameManager.instance.entryManager.UpdateEntry(model);
            ClosePopup();
            StartCoroutine("MoveCar");
        }
        else
        {
            mogMsg.text = "To catch this SCP you need " + model.grabcoef + " MOG squads at "+ s +" base, but you have " + baza.amountofMog;
        }
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

    GameObject FindClosestBase(GameObject[] objects)
    {
        GameObject closest = new GameObject();
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in objects)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }
}
