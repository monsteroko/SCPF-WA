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
    }

    void Update()
    {
        foreach (var e in eventsDates)
        { 
            if (DateTime.Compare(e,GameManager.instance.timeManager.GetTime())<=0)
            {
                eventsDates.Remove(e);
                StartEvent();
            }  
        }
    }

    public void CreateEvents()
    {

        for (int i = 0; i < UnityEngine.Random.Range(100,300); i++)
        {
            
            eventsDates.Add(new DateTime(UnityEngine.Random.Range(1891, 1900), UnityEngine.Random.Range(1, 12), UnityEngine.Random.Range(1, 28), 12, 0, 0));
            Debug.Log(eventsDates[i]);
        }
        eventsDates.Add(new DateTime(1890, 7, 12, 16, 0, 0));

    }
    public void StartEvent()
    {
        if (!GameManager.instance.timeManager.isOnPause() && GameObject.FindGameObjectWithTag("Base")!=null)
        {
            var popup = EntryPopup.Instance();
            popup.OpenWithEntry(GameManager.instance.entryManager.GetRandomEntry());
        }
    }
}
