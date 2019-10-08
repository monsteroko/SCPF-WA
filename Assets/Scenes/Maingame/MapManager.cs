using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public enum AreaState
{
    Locked = 0,
    Unlocked = 1,
    Controlled = 2,
}

public class Area
{
    public readonly string name;
    private AreaState _state = AreaState.Locked;
    public List<Region> regions;
    public int cityIndex = -1;
    public int index = -1;
    private WorldMap2D map;
    public AreaState state {
        get {
            return _state;
        }
        set {
            if (value == _state) return;
            map.ToggleProvinceSurface(name, value == AreaState.Locked);
            if (cityIndex >= 0 && cityIndex <= map.cities.Count)
            {
                switch (value)
                {
                    case AreaState.Locked:
                        map.cities[cityIndex].population = 2;
                        map.cities[cityIndex].cityClass = CITY_CLASS.COUNTRY_CAPITAL;
                        break;
                    case AreaState.Unlocked:
                        map.cities[cityIndex].population = 2;
                        map.cities[cityIndex].cityClass = CITY_CLASS.REGION_CAPITAL;
                        break;
                    case AreaState.Controlled:
                        map.cities[cityIndex].population = -1;
                        map.cities[cityIndex].cityClass = CITY_CLASS.REGION_CAPITAL;
                        break;
                }
            }
            map.DrawCities();
            _state = value;
        }
    }
    
    public Area(WorldMap2D map, Province province, int index)
    {
        this.map = map;
        this.index = index;
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
        map.minPopulation = 1;
        map.OnCityClick += OnCityClick;
        // Init data from map
        foreach (Country country in map.countries)
        {
            counterparts[map.GetCountryIndex(country.name)] = new Counterpart(country);
        }
        int index = 0;
        foreach (Province province in map.provinces)
        {
            counterparts[province.countryIndex].addRegion(new Area(map, province, index));
            map.ToggleProvinceSurface(index, true, new Color(0.1f, 0.1f, 0.1f, 0.8f));
            index++;
        }
        for (int i = 0; i < map.cities.Count; i++)
        {
            City city = map.cities[i];
            counterparts[city.countryIndex].areas[city.province].cityIndex = i;
        }
        // Init data from save
        foreach (string areaName in mapSave.unlockedAreas)
        {
            foreach (Counterpart counterpart in counterparts.Values)
            {
                if (counterpart.areas.ContainsKey(areaName))
                {
                    counterpart.areas[areaName].state = AreaState.Unlocked;
                    map.FlyToProvince(counterpart.name, areaName, 1, 0.05f);
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

    //Mutating

    int selectedCounterpartIndex = -1;
    string selectedAreaName = null;

    private void CreateZoneCompletion(bool confirmed)
    {
        if (!confirmed) return;
        Area area = counterparts[selectedCounterpartIndex].areas[selectedAreaName];
        area.state = AreaState.Controlled;
        foreach (Province province in map.ProvinceNeighbours(area.index))
        {
            counterparts[province.countryIndex].areas[province.name].state = AreaState.Unlocked;
        }
    }

    //Events
    
    void OnCityClick(int cityIndex) {
        selectedCounterpartIndex = map.cities[cityIndex].countryIndex;
        selectedAreaName = map.cities[cityIndex].province;
        CreateZoneDialog.Instance().Open(CreateZoneCompletion);
    }
}
