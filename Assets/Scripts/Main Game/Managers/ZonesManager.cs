using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

public class ZonesManager : MonoBehaviour {
    private int countofBases = 0;
    private Map map;

    public int CountofBases
    {
        get
        {
            return countofBases;
        }
    }
    public GameObject zonePlaceModel;
    public GameObject basicZoneModel;
    public string selectedZoneName { get; private set; }
    void OnEnable() {
        map = GameManager.instance.mapManager.map;
    }

    public void RefreshAllZones() {
        foreach (Zone zone in map.zones.Values) {
            if(zone.area.state==AreaState.Unlocked)
                zone.SetModelObject(ZoneModelObjectForState(zone.area.state));
        }
    }

    public void BuildZone(Zone zone) {
        if (zone.isBuilt) {
            Debug.Log("Tried to build already built zone!.");
            return;
        }
        zone.SetModelObject(ZoneModelObjectForState(AreaState.Controlled));
        zone.isBuilt = true;
        GameManager.instance.mapManager.UpdateMapForBuiltZone(zone);
    }

    public GameObject ZoneModelObjectForState(AreaState state) {
        GameObject obj = Instantiate(state == AreaState.Controlled ? basicZoneModel : zonePlaceModel);
        var randomScale = Random.Range(0.8f, 1.2f);
        var rotationAngle = Random.Range(0f, 360f);
        if (state==AreaState.Controlled && !obj.name.StartsWith("Base "))
        {
            obj.name = "Base " + Random.Range(0, 1000);
            obj.tag = "Base";
        }
        if (state == AreaState.Unlocked)
        {
            obj.name = "Polygon";
        }
        obj.transform.localScale = new Vector3(randomScale, 100f, randomScale);
        obj.transform.rotation = Quaternion.Euler(270.0f, 0, 0) * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        obj.transform.SetParent(map.mapZonesObject.transform, false);
        obj.AddComponent<ZoneSelect>();
        obj.SetActive(state != AreaState.Locked);
        return obj;
    }

    public void SelectZoneWithName(string name) {
        selectedZoneName = name;
        if (map.zones[name].isBuilt) {
            ZoneMenu.Instance().RollOut(selectedZoneName);
        } else {
            CreateZoneDialog.Instance().Open();
        }
    }
    public void BuildSelectedZone() {
        BuildZone(map.zones[selectedZoneName]);
        Debug.Log(selectedZoneName);
        GameManager.instance.zonesResourcesManager.AddBase(selectedZoneName);
        countofBases++;
    }
}
