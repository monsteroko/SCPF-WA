using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSelect : MonoBehaviour {

    public string zoneName;

    void OnMouseDown() {
        GameManager.instance.zonesManager.SelectZoneWithName(zoneName);
    }
}
