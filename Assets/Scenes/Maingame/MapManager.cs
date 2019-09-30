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
    public Dictionary<string, Area> regions = new Dictionary<string, Area>();
    public Counterpart(Country country)
    {
        name = country.name;
    }

    public void addRegion(Area region)
    {
        regions[region.name] = region;
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
		// Init data from map
		foreach (Country country in map.countries)
        {
            counterparts[map.GetCountryIndex(country.name)] = new Counterpart(country);
        }
        foreach (Province province in map.provinces)
        {
            counterparts[province.countryIndex].addRegion(new Area(province));
		}
        // Init data from save
        foreach (string regionName in mapSave.unlockedRegions)
        {
            Debug.Log(regionName);
            foreach (Counterpart counterpart in counterparts.Values)
            {
                if (counterpart.regions.ContainsKey(regionName))
                {
                    counterpart.regions[regionName].unlocked = true;
					map.ToggleProvinceSurface(regionName, false, Color.cyan);
					map.FlyToProvince(counterpart.name, regionName, 1, 0.2f);
                    unlockedAreas.Add(counterpart.regions[regionName]);
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
