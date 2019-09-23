using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

class Region
{
    public readonly string name;
    public bool unlocked = false;

    public Region(Province province)
    {
        name = province.name;
    }
}

class Counterpart
{
    public readonly string name;
    public Dictionary<string, Region> regions = new Dictionary<string, Region>();
    public Counterpart(Country country)
    {
        name = country.name;
    }

    public void addRegion(Region region)
    {
        regions[region.name] = region;
    }
}

public class MapManager
{
    Dictionary<int, Counterpart> counterparts = new Dictionary<int, Counterpart>();
    
    public MapManager(MapSaveModel mapSave) 
    {
        WorldMap2D map = WorldMap2D.instance;
        // Init data from map
        foreach (Country country in map.countries)
        {
            counterparts[map.GetCountryIndex(country.name)] = new Counterpart(country);
        }
        foreach (Province province in map.provinces)
        {
            counterparts[province.countryIndex].addRegion(new Region(province));
        }
        Debug.Log("kek");
        foreach (string regionName in mapSave.unlockedRegions)
        {
            Debug.Log(regionName);
            foreach (Counterpart counterpart in counterparts.Values)
            {
                if (counterpart.regions.ContainsKey(regionName))
                {
                    counterpart.regions[regionName].unlocked = true;
                    bool res = map.FlyToProvince(counterpart.name, regionName, 3, 100);
                }
            }
        }
    }
}
