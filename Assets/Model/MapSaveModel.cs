using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSaveModel
{
    public readonly List<string> unlockedAreas;
    
    private MapSaveModel(List<string> unlockedAreas)
    {
        this.unlockedAreas = unlockedAreas;
    }

    public static MapSaveModel NewGameSave(string counterpartname) 
    {
        List<string> unlockedAreas = new List<string>();
        switch (counterpartname)
        {
            case "eeu":
                unlockedAreas.Add("Kharkiv");
                break;
            case "na":
                unlockedAreas.Add("Iowa");
                break;
            case "sa":
                unlockedAreas.Add("Buenos Aires");
                break;
            default:
                Debug.Log("Wrong starting counterpart");
                break;
        }
        return new MapSaveModel(unlockedAreas);
    }
}
