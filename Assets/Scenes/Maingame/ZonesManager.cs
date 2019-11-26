using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

public class ZonesManager : MonoBehaviour {

    private Map map;
    public GameObject zonePlaceModel;
    public GameObject basicZoneModel;
    public int selectedZoneIndex { get; private set; }
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
        obj.SetActive(state != AreaState.Locked);
        var randomScale = Random.Range(0.8f, 1.2f);
        var rotationAngle = Random.Range(0f, 360f);
        obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        obj.transform.rotation = Quaternion.Euler(90.0f, 0, 0) * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        obj.transform.SetParent(map.mapZonesObject.transform, false);
        ZoneSelect zoneSelectScript = obj.AddComponent<ZoneSelect>();
        zoneSelectScript.zoneIndex = 0; //TODO: set real
        return obj;
    }

    public void SelectZoneWithIndex(int index) {
        selectedZoneIndex = index;
        if (map.zones[index].isBuilt) {
            ZoneMenu.Instance().Open();
        } else {
            CreateZoneDialog.Instance().Open();
        }
    }

    public void BuildSelectedZone() {
        BuildZone(map.zones[selectedZoneIndex]);
    }
}
