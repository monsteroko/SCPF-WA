﻿using System.Collections;
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
