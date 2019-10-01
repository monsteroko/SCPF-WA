using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public class Area
{
    public readonly string name;
    public bool unlocked = false;
    public List<Region> regions;

    public Area(Province province)
    {
        name = province.name;
        regions = province.regions;
    }
}

public class Counterpart
{
    public readonly string name;
    public Dictionary<string, Area> areas = new Dictionary<string, Area>();
    public Counterpart(Country country)
    {
        name = country.name;
    }

    public void addRegion(Area area)
    {
        areas[area.name] = area;
    }
}

public class MapManager
{
    Dictionary<int, Counterpart> counterparts = new Dictionary<int, Counterpart>();
	WorldMap2D map = WorldMap2D.instance;
    public List<Area> unlockedAreas = new List<Area>();

	public MapManager(MapSaveModel mapSave) 
    {
		// Setup map
		map.showProvinces = true;
        map.enableCountryHighlight = false;
        map.enableProvinceHighlight = false;
		// Init data from map
		foreach (Country country in map.countries)
        {
            counterparts[map.GetCountryIndex(country.name)] = new Counterpart(country);
        }
        int index = 0;
        foreach (Province province in map.provinces)
        {
            counterparts[province.countryIndex].addRegion(new Area(province));
            map.ToggleProvinceSurface(index, true, new Color(0.1f, 0.1f, 0.1f, 0.8f));
            index++;
        }
        foreach (Counterpart counterpart in counterparts.Values) {
            Debug.Log(counterpart.name);
            Debug.Log(counterpart.areas.Count);
        }
        Debug.Log(map.provinces.Length);
        // Init data from save
        foreach (string areaName in mapSave.unlockedAreas)
        {
            Debug.Log(areaName);
            foreach (Counterpart counterpart in counterparts.Values)
            {
                if (counterpart.areas.ContainsKey(areaName))
                {
                    counterpart.areas[areaName].unlocked = true;
                    map.ToggleProvinceSurface(areaName, false, Color.cyan);
					map.FlyToProvince(counterpart.name, areaName, 1, 0.1f);
                    unlockedAreas.Add(counterpart.areas[areaName]);
				}
            }
        }
    }

    public Vector2 GeneratePointInUnlockedAreas() {
        Area area = unlockedAreas[Random.Range(0, unlockedAreas.Count - 1)];
        Region region = area.regions[0];
        Rect rect = region.rect2D;
        Vector2 point;
        while (true) {
            float x = rect.xMin + Random.Range(0.0f, 1.0f) * (rect.xMax - rect.xMin);
            float y = rect.yMin + Random.Range(0.0f, 1.0f) * (rect.yMax - rect.yMin);
            point = new Vector2(x, y);
            if (region.Contains(point)) {
                return point;
            }
        }
    } 
}
