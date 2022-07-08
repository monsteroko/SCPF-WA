using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZoneSelect : MonoBehaviour {

    public string zoneName;

    void OnMouseDown() {
        if (!EventSystem.current.IsPointerOverGameObject())
            GameManager.instance.zonesManager.SelectZoneWithName(zoneName);
    }
}
