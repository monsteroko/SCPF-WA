using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveModel
{
    public readonly List<string> unlockedRegions;
    
    private MapSaveModel(List<string> unlockedRegions)
    {
        this.unlockedRegions = unlockedRegions;
    }

    public static MapSaveModel NewGameSave(string counterpartname) 
    {
        List<string> unlockedRegions = new List<string>();
        switch (counterpartname)
        {
            case "eeu":
                unlockedRegions.Add("Kharkiv");
                break;
            case "na":
                unlockedRegions.Add("Iowa");
                break;
            case "sa":
                unlockedRegions.Add("Buenos Aires");
                break;
            default:
                Debug.Log("Wrong starting counterpart");
                break;
        }
        return new MapSaveModel(unlockedRegions);
    }
}
