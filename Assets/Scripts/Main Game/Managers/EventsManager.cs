using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public void CreateEvents()
    {
        List<DateTime> eventsDates= new List<DateTime>();
        for (int i = 0; i < UnityEngine.Random.Range(50,100); i++)
        {
            
            eventsDates.Add(new DateTime(UnityEngine.Random.Range(1891, 1900), UnityEngine.Random.Range(1, 12), UnityEngine.Random.Range(1, 28), 12, 0, 0));
        }
        foreach (DateTime eventdate in eventsDates)
        {
            IEnumerator coroutine = EventActivate(eventdate);
            StartCoroutine("coroutine", 3f);
        }
    }
    public void StartEvent()
    {
        if (!GameManager.instance.timeManager.isOnPause()) {
            var popup = EntryPopup.Instance();
            popup.OpenWithEntry(GameManager.instance.entryManager.GetRandomEntry());
        }
    }

    IEnumerator EventActivate(DateTime eventDT)
    {
        while (true)
        {
            /*if (eventDT == GameManager.instance.timeManager.GetTime())
                StartEvent();*/
            yield return null;
        }
    }
}
