using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

public class ZonesManager : MonoBehaviour {

    private Map map;
    public GameObject zonePlaceModel;
    public GameObject basicZoneModel;
    public string selectedZoneName { get; private set; }
    // Start is called before the first frame update
    void OnEnable() {
        map = GameManager.instance.mapManager.map;
    }

    public void RefreshAllZones() {
        foreach (Zone zone in map.zones.Values) {
            zone.SetModelObject(ZoneModelObjectForState(zone.area.state));
        }
    }

    public void BuildZone(Zone zone) {
        if (zone.isBuilt) {
            Debug.Log("Tried to build already built zone. Eto ne dolzhno tak rabotat'.");
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
        obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        obj.transform.rotation = Quaternion.Euler(270.0f, 0, 0) * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        obj.transform.SetParent(map.mapZonesObject.transform, false);
        obj.AddComponent<ZoneSelect>();
        obj.SetActive(state != AreaState.Locked);
        return obj;
    }

    public void SelectZoneWithName(string name) {
        selectedZoneName = name;
        if (map.zones[name].isBuilt) {
            ZoneMenu.Instance().Open();
        } else {
            CreateZoneDialog.Instance().Open();
        }
    }

    public void BuildSelectedZone() {
        BuildZone(map.zones[selectedZoneName]);
    }
}
