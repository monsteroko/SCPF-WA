using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

public class ZonesManager : MonoBehaviour {

    private Map map;
    public GameObject zonePlaceModel;
    public GameObject basicZoneModel;
    // Start is called before the first frame update
    void OnEnable() {
        map = GameManager.instance.mapManager.map;
    }

    public void RefreshAllZones() {
        foreach (Zone zone in map.zones) {
            zone.SetModelObject(ZoneModelObjectForState(zone.area.state));
        }
    }

    public void BuildZone(Zone zone) {
        zone.SetModelObject(ZoneModelObjectForState(AreaState.Controlled));
    }

    public GameObject ZoneModelObjectForState(AreaState state) {
        GameObject obj = Instantiate(state == AreaState.Controlled ? basicZoneModel : zonePlaceModel);
        obj.SetActive(state != AreaState.Locked);
        obj.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
        obj.transform.SetParent(map.mapZonesObject.transform, false);
        return obj;
    }
}
