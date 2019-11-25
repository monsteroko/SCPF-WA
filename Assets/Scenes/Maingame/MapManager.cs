using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

public class MapManager: MonoBehaviour
{
    public Map map;
    public List<Area> unlockedAreas = new List<Area>();

    public MapManager()
    {
        //TODO
        //map.OnCityClick += OnCityClick;
    }

    public void InitWithSave(MapSaveModel mapSave) {
        foreach (string areaName in mapSave.unlockedAreas) {
            foreach (Area area in map.areas) {
                if (area.name == areaName) {
                    area.state = AreaState.Unlocked;
                    map.FocusCameraOn(area);
                    unlockedAreas.Add(area);
                }
            }
        }
        GameManager.instance.zonesManager.RefreshAllZones();
    }

    public Vector2 GeneratePointInUnlockedAreas() {
        Area area = unlockedAreas[Random.Range(0, unlockedAreas.Count - 1)];
        Rect rect = area.borderRect;
        Vector2 point;
        while (true) {
            float x = rect.xMin + Random.Range(0.0f, 1.0f) * (rect.xMax - rect.xMin);
            float y = rect.yMin + Random.Range(0.0f, 1.0f) * (rect.yMax - rect.yMin);
            point = new Vector2(x, y);
            if (new ClipperUtility().IsPointInsideSurface(point, area)) {
                return point;
            }
        }
    }

    //Mutating

    int selectedCounterpartIndex = -1;
    string selectedAreaName = null;

    /*private void CreateZoneCompletion(bool confirmed)
    {
        if (!confirmed) return;
        Area area = counterparts[selectedCounterpartIndex].areas[selectedAreaName];
        area.state = AreaState.Controlled;
        GameManager.instance.zonesManager.CreateZoneAtPlace(map.cities[area.cityIndex]);
        foreach (Province province in map.ProvinceNeighbours(area.index))
        {
            if (counterparts[province.countryIndex].areas[province.name].state == AreaState.Locked) {
                counterparts[province.countryIndex].areas[province.name].state = AreaState.Unlocked;
            }
        }
    }*/

    //Events
    
    /*void OnCityClick(int cityIndex) {
        selectedCounterpartIndex = map.cities[cityIndex].countryIndex;
        selectedAreaName = map.cities[cityIndex].province;
        CreateZoneDialog.Instance().Open(CreateZoneCompletion);
    }*/
}
