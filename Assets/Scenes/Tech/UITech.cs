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

    // Start is called before the first frame update
    void Start()
    {
        sec.onClick.AddListener(SecurityTech);
        con.onClick.AddListener(ContainmentTech);
        pro.onClick.AddListener(ProtectionTech);
        secB.onClick.AddListener(SecurityTech);
        conB.onClick.AddListener(ContainmentTech);
        proB.onClick.AddListener(ProtectionTech);
        back.onClick.AddListener(Back);
    }

    // Update is called once per frame
    void Update()
    {
        if (lim > now && lim == 70)
        {
            Move();
            now++;
            now++;
        }

        if (lim < now && lim == 0)
        {
            Move();
            now--;
            now--;
        }
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
        Debug.Log(lim);
        Debug.Log(now);
        Debug.Log(vector);
        vector = Vector3.up;
        lim = 0;
        Debug.Log(lim);
        Debug.Log(vector);
    }

    public void ContainmentTechB()
    {
        vector = Vector3.down + Vector3.left * 2;
        lim = 0;
    }

    public void ProtectionTechB()
    {
        vector = Vector3.down + Vector3.right * 2;
        lim = 0;
    }


    public void Back()
    {
        SceneManager.UnloadSceneAsync("Tech");
    }
}
