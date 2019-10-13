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
        Debug.Log(city.unity2DLocation);
        zoneObject.transform.position = map.transform.TransformPoint(city.unity2DLocation);
        zoneObject.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        zoneObject.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
        return zone;
    }
}
