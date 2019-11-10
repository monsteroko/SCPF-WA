using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WPMF;

public class Zone {

}

public class ZonesManager : MonoBehaviour
{
    public GameObject basicZoneObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Zone CreateZoneAtPlace(City city) {
        Zone zone = new Zone();
        GameObject zoneObject = Instantiate(basicZoneObject);
        WorldMap2D map = WorldMap2D.instance;
        zoneObject.AddComponent<ZoneMenuOpen>();
        Debug.Log(city.unity2DLocation);
        zoneObject.transform.position = map.transform.TransformPoint(city.unity2DLocation);
        var randomScale = Random.Range(0.04f, 0.06f);
        zoneObject.transform.localScale = new Vector3(randomScale, 0.05f, randomScale);
        var rotationAngle = Random.Range(0f, 360f);
        zoneObject.transform.rotation = Quaternion.Euler(-90f, 0.0f, 0.0f) * Quaternion.Euler(0.0f, rotationAngle, 0.0f);
        BoxCollider boxCollider = zoneObject.AddComponent<BoxCollider>();
        boxCollider.size= new Vector3(8.0f, 2.0f, 9.0f);
        return zone;
    }
}
