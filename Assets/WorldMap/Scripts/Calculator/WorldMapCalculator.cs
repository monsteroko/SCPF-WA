using UnityEngine;
using System;
using System.Collections;

namespace WPMF {
	public enum UNIT_TYPE {
		Degrees,
		DecimalDegrees,
		PlaneCoordinates
	}

	[Serializable]
	[RequireComponent(typeof(WorldMap2D))]
	public class WorldMap2D_Calculator : MonoBehaviour {

		public UNIT_TYPE fromUnit = UNIT_TYPE.Degrees;

		// From: latitude (degree)
		public float fromLatDegrees;
		public int fromLatMinutes;
		public float fromLatSeconds;
		// From: longitude (degree)
		public float fromLonDegrees;
		public int fromLonMinutes;
		public float fromLonSeconds;
		// From: decimal degrees
		public float fromLatDec, fromLonDec;
		// From: spherical coordinates
		public float fromX, fromY;
		// To: latitude (degree)
		public float toLatDegree;
		public int toLatMinute;
		public float toLatSeconds;
		// To: longitude (degree)
		public float toLonDegree;
		public int toLonMinute;
		public float toLonSecond;
		// To: decimal degrees
		public float toLatDec, toLonDec;
		// To: spherical coordinates
		public float toX, toY;
		public bool captureCursor;
		public bool isDirty;

		public int cityDistanceFrom=-1, cityDistanceTo=-1;
		public string cityDistanceResult = "";

		public WorldMap2D map { get { return GetComponent<WorldMap2D> (); } }

		public Vector3 toPlaneLocation { get { return new Vector2(toX, toY); } }

		Vector3 lastCursorPos;
		public string errorMsg;

		void Update () {
			if (captureCursor) {
				if (map != null && map.cursorLocation != lastCursorPos) {
					lastCursorPos = map.cursorLocation;
					fromX = map.cursorLocation.x;
					fromY = map.cursorLocation.y;
					Convert ();
				}
				if (Input.GetKey(KeyCode.C)) {
					captureCursor = false;
				}
			}
		}

		public bool Convert () {
			errorMsg = "";
			try {
				if (fromUnit == UNIT_TYPE.Degrees) {
					toLatDegree = fromLatDegrees;
					toLatMinute = fromLatMinutes;
					toLatSeconds = fromLatSeconds;
					toLonDegree = fromLonDegrees;
					toLonMinute = fromLonMinutes;
					toLonSecond = fromLonSeconds;
					toLatDec = fromLatDegrees + fromLatMinutes / 60.0f + fromLatSeconds / 3600.0f;
					toLonDec = fromLonDegrees + fromLonMinutes / 60.0f + fromLonSeconds / 3600.0f;
					toX = (toLonDec+180)/360 - 0.5f;
					toY = toLatDec/180;
				} else if (fromUnit == UNIT_TYPE.DecimalDegrees) {
					toLatDec = fromLatDec;
					toLonDec = fromLonDec;
					toLatDegree = (int)fromLatDec;
					toLatMinute = (int)(Mathf.Abs (fromLatDec) * 60) % 60;
					toLatSeconds = (Mathf.Abs (fromLatDec) * 3600) % 60;
					toLonDegree = (int)fromLonDec;
					toLonMinute = (int)(Mathf.Abs (fromLonDec) * 60) % 60;
					toLonSecond = (Mathf.Abs (fromLonDec) * 3600) % 60;
					toX = (toLonDec+180)/360 - 0.5f;
					toY = toLatDec/180;
				} else if (fromUnit == UNIT_TYPE.PlaneCoordinates) {
					toLatDec = 180.0f * fromY;
					toLonDec = 360.0f * (fromX + 0.5f) - 180.0f;
					toLatDegree = (int)toLatDec;
					toLatMinute = (int)(Mathf.Abs (toLatDec) * 60) % 60;
					toLatSeconds = (Mathf.Abs (toLatDec) * 3600) % 60;
					toLonDegree = (int)toLonDec;
					toLonMinute = (int)(Mathf.Abs (toLonDec) * 60) % 60;
					toLonSecond = (Mathf.Abs (toLonDec) * 3600) % 60;
					toX = fromX;
					toY = fromY;
				}
			} catch (ApplicationException ex) {
				errorMsg = ex.Message;
			}
			isDirty = true;
			return errorMsg.Length == 0;
		}

		/// <summary>
		/// Returns distance in meters from two lat/lon coordinates
		/// </summary>
		public float Distance(float latDec1, float lonDec1, float latDec2, float lonDec2) {
			float R = 6371000; // metres
			float phi1 = latDec1 * Mathf.Deg2Rad;
			float phi2 = latDec2 * Mathf.Deg2Rad;
			float deltaPhi = (latDec2-latDec1)* Mathf.Deg2Rad;
			float deltaLambda = (lonDec2-lonDec1)* Mathf.Deg2Rad;
			
			float a = Mathf.Sin(deltaPhi/2) * Mathf.Sin(deltaPhi/2) +
				Mathf.Cos(phi1) * Mathf.Cos(phi2) *
					Mathf.Sin(deltaLambda/2) * Mathf.Sin(deltaLambda/2);
			float c = 2.0f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1.0f-a));
			return R * c;
		}

		public float Distance(City city1, City city2) {
			float latDec1 = 180.0f * city1.unity2DLocation.y;
			float lonDec1 = 360.0f * (city1.unity2DLocation.x + 0.5f) - 180.0f;
			float latDec2 = 180.0f * city2.unity2DLocation.y;
			float lonDec2 = 360.0f * (city2.unity2DLocation.x + 0.5f) - 180.0f;
			return Distance (latDec1, lonDec1, latDec2, lonDec2);

		}
	
	}



}
