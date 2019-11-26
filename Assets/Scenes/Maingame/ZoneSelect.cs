using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSelect : MonoBehaviour {

    public int zoneIndex;

    void OnMouseDown() {
        GameManager.instance.zonesManager.SelectZoneWithIndex(zoneIndex);
    }
}
