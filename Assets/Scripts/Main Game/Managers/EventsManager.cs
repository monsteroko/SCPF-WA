using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    List<DateTime> eventsDates = new List<DateTime>();

    void Start()
    {
        CreateEvents();
        InvokeRepeating("StartEvent", 0, 1.0f);
    }

    public void CreateEvents()
    {
        for (int i = 0; i < UnityEngine.Random.Range(100,300); i++)
        {
            eventsDates.Add(new DateTime(UnityEngine.Random.Range(1891, 1900), UnityEngine.Random.Range(1, 12), UnityEngine.Random.Range(1, 28), 12, 0, 0));
        }
        eventsDates.Add(new DateTime(1891, 1, 1, 12, 0, 0));
        eventsDates.Sort();
    }
    public void StartEvent()
    {
        if ((DateTime.Compare(eventsDates[0], GameManager.instance.timeManager.GetTime()) < 0)&& (!GameManager.instance.timeManager.isOnPause()) && (GameObject.FindGameObjectWithTag("Base") != null))
        {
            eventsDates.RemoveAt(0);
            var popup = EntryPopup.Instance();
            popup.OpenWithEntry(GameManager.instance.entryManager.GetRandomEntry());
        }
    }
}
