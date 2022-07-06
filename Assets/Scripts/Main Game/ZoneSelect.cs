using UnityEngine;
using UnityEngine.UI;

public class ZoneSelect : MonoBehaviour {

    public string zoneName;

    void OnMouseDown() {
        GameManager.instance.zonesManager.SelectZoneWithName(zoneName);
    }
}
