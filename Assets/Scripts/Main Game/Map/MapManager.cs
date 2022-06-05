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
            map.areas[areaName].state = AreaState.Unlocked;
            map.FocusCameraOn(map.areas[areaName]);
            unlockedAreas.Add(map.areas[areaName]);
        }
        GameManager.instance.zonesManager.RefreshAllZones();
    }

    public Vector2 GeneratePointInUnlockedAreas() {
        Area area = unlockedAreas[Random.Range(0, unlockedAreas.Count - 1)];
        Vector2 areacenter = area.center;
        Vector2 point;
        while (true) {
            float x = areacenter.x + Random.Range(-0.03f, 0.03f);
            float y = areacenter.y + Random.Range(-0.03f, 0.03f);
            point = new Vector2(x, y);
            if (new ClipperUtility().IsPointInsideSurface(point, area)) {
                return point;
            }
        }
    }

    //Mutating

    public void UpdateMapForBuiltZone(Zone zone) {
        zone.area.state = AreaState.Controlled;
        List<Area> neighbors = map.GetNeighborAreas(zone.area);
        foreach (Area area in neighbors) {
            if (area.state == AreaState.Locked) {
                area.state = AreaState.Unlocked;
            }
        }
        GameManager.instance.zonesManager.RefreshAllZones();
    }
}
