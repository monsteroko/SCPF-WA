using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public class RegionModel
{
    public readonly string name;
    public bool unlocked = false;
    public List<Region> regions;

    public RegionModel(Province province)
    {
        name = province.name;
        regions = province.regions;
    }
}

public class CounterpartModel
{
    public readonly string name;
    public Dictionary<string, RegionModel> regions = new Dictionary<string, RegionModel>();
    public CounterpartModel(Country country)
    {
        name = country.name;
    }

    public void addRegion(RegionModel region)
    {
        regions[region.name] = region;
    }
}

public class MapManager
{
    Dictionary<int, CounterpartModel> counterparts = new Dictionary<int, CounterpartModel>();
	WorldMap2D map = WorldMap2D.instance;
    public List<RegionModel> unlockedRegions = new List<RegionModel>();

	public MapManager(MapSaveModel mapSave) 
    {
		// Setup map
		map.showProvinces = true;
		// Init data from map
		foreach (Country country in map.countries)
        {
            counterparts[map.GetCountryIndex(country.name)] = new CounterpartModel(country);
        }
        foreach (Province province in map.provinces)
        {
            counterparts[province.countryIndex].addRegion(new RegionModel(province));
		}
        // Init data from save
        foreach (string regionName in mapSave.unlockedRegions)
        {
            Debug.Log(regionName);
            foreach (CounterpartModel counterpart in counterparts.Values)
            {
                if (counterpart.regions.ContainsKey(regionName))
                {
                    counterpart.regions[regionName].unlocked = true;
					map.ToggleProvinceSurface(regionName, false, Color.cyan);
					map.FlyToProvince(counterpart.name, regionName, 1, 0.2f);
                    unlockedRegions.Add(counterpart.regions[regionName]);
				}
            }
        }
    }
}
