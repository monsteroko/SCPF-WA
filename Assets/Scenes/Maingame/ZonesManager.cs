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
        var randomScale = Random.Range(0.8f, 1.2f);
        var rotationAngle = Random.Range(0f, 360f);
        obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        obj.transform.rotation = Quaternion.Euler(90.0f, 0, 0) * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        obj.transform.SetParent(map.mapZonesObject.transform, false);
        BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(8.0f, 2.0f, 9.0f);
        switch(state) {
            case AreaState.Unlocked:
                obj.AddComponent<ZoneMenuOpen>();
                break;
            case AreaState.Controlled:
                break;
            default:
                break;
        }
        return obj;
    }
}
