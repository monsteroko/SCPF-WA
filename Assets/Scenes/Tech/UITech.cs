using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UITech : MonoBehaviour
{
    public Button sec;
    public Button con;
    public Button pro;
    public Button secB;
    public Button conB;
    public Button proB;
    public Button back;

    public GameObject panel;

    private int lim, now = 0;
    private Vector3 vector;

    private int scrHeight;
    private int scrWidth;


    void Start()
    {
        sec.onClick.AddListener(SecurityTech);
        con.onClick.AddListener(ContainmentTech);
        pro.onClick.AddListener(ProtectionTech);
        secB.onClick.AddListener(SecurityTechB);
        conB.onClick.AddListener(ContainmentTechB);
        proB.onClick.AddListener(ProtectionTechB);
        back.onClick.AddListener(Back);
        now = 0;
        scrWidth = Screen.currentResolution.width;
        scrHeight = Screen.currentResolution.height;
    }


    void Update()
    {
        if (lim > now && lim >= 70)
        {
            Move();
            now++;
            now++;
        }

        if (lim < now && lim <= 0)
        {
            Move();
            now--;
            now--;
        }

        //panel.transform.position = Vector3.up * 271 + Vector3.right * 482;
    }

    private void Move()
    {
        panel.transform.Translate(vector * 2);
    }

    public void SecurityTech()
    {
        vector = Vector3.down;
        lim = 70;
    }

    public void ContainmentTech()
    {
        vector = Vector3.up + Vector3.right * 2;
        lim = 70;
    }

    public void ProtectionTech()
    {
        vector = Vector3.up + Vector3.left * 2;
        lim = 70;
    }

    public void SecurityTechB()
    {
        panel.transform.position = Vector3.down * 290 + Vector3.right * 482;
        vector = Vector3.up;
        lim = 0;
    }

    public void ContainmentTechB()
    {
        panel.transform.position = Vector3.up * 757 + Vector3.right * (1462);
        vector = Vector3.down + Vector3.left * 2;
        lim = 0;
    }

    public void ProtectionTechB()
    {
        panel.transform.position = Vector3.up * 831 + Vector3.left * (640);
        vector = Vector3.down + Vector3.right * 2;
        lim = 0;
    }


    public void Back()
    {
        SceneManager.UnloadSceneAsync("Tech");
    }
}
