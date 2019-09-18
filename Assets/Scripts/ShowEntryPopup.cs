using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowEntryPopup : MonoBehaviour
{
    public float timer;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        Time.timeScale = timer;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timer;
    }
}
