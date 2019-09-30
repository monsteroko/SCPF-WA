using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ZoneManagementButtons : MonoBehaviour
{
    public GameObject SCPs;
    public GameObject Parameters;
    public GameObject Employees;

    public Button SCPsButton; 
    public Button ParaButton; 
    public Button EmpButton; 

    // Start is called before the first frame update
    void Start()
    {
        Employees.SetActive(false);
        SCPs.SetActive(false);
        Parameters.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        SCPsButton.onClick.AddListener(SCPList);
        EmpButton.onClick.AddListener(Personnel);
        ParaButton.onClick.AddListener(BaseManagement);
    }

    void Personnel()
    {
        Employees.SetActive(true);
        SCPs.SetActive(false);
        Parameters.SetActive(false);
    }

    void SCPList()
    {
        Employees.SetActive(false);
        SCPs.SetActive(true);
        Parameters.SetActive(false);
    }

    void BaseManagement()
    {
        Employees.SetActive(false);
        SCPs.SetActive(false);
        Parameters.SetActive(true);
    }
}
