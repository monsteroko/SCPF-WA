using UnityEngine;
using System.Collections;
using WPMF;

public class DemoOrthoViewport : MonoBehaviour
{

	// Use this for initialization
	WorldMap2D map;

	void Start ()
	{
		map = WorldMap2D.instance;
	}
	
	int c = 0;

	void Update ()
	{
		Vector3 screenPos = Camera.main.WorldToScreenPoint(map.renderViewport.transform.position);
		Vector3 mapPos;

		if (map.GetLocalHitFromScreenPos(screenPos, out mapPos)) {
			int d = map.GetCountryIndex (mapPos);
			if (d != c) {
				c = d;
				if (c >= 0)
					Debug.Log ("Country at center of viewport: " + map.countries [c].name);
			}
		}
	}
}
