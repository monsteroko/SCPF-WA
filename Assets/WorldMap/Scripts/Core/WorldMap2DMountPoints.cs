// World Political Map - 2D Edition for Unity - Main Script
// Copyright 2015-2017 Kronnect
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM


using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace WPMF {

	public partial class WorldMap2D : MonoBehaviour {

		const float MOUNT_POINT_HIT_PRECISION = 0.0015f;

		#region Internal variables

		// resources
		Material mountPointsMat;
		GameObject mountPointSpot, mountPointsLayer;

		#endregion



		#region System initialization

		void ReadMountPointsPackedString () {
			string mountPointsCatalogFileName = "Geodata/mountPoints";
			TextAsset ta = Resources.Load<TextAsset> (mountPointsCatalogFileName);
			if (ta!=null) {
				string s = ta.text;
				ReadMountPointsPackedString(s);
			} else {
				mountPoints = new List<MountPoint>();
			}
		}

		/// <summary>
		/// Reads the mount points data from a packed string.
		/// </summary>
		void ReadMountPointsPackedString (string s) {
			string[] mountPointsList = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			int mountPointsCount = mountPointsList.Length;
			mountPoints = new List<MountPoint> (mountPointsCount);

			for (int k=0; k<mountPointsCount; k++) {
				string[] mountPointInfo = mountPointsList [k].Split (new char[] { '$' });
				string name = mountPointInfo [0];
				string country = mountPointInfo [2];
				int countryIndex = GetCountryIndex(country);
				if (countryIndex>=0) {
					string province = mountPointInfo [1];
					int provinceIndex = GetProvinceIndex(countryIndex, province);
					int type = int.Parse (mountPointInfo [3], CultureInfo.InvariantCulture);
					float x = float.Parse (mountPointInfo [4], CultureInfo.InvariantCulture);
					float y = float.Parse (mountPointInfo [5], CultureInfo.InvariantCulture);
					Dictionary<string, string> tags = new Dictionary<string, string>();
					for (int t=6;t<mountPointInfo.Length;t++) {
						string tag = mountPointInfo[t];
						string[] tagInfo =  tag.Split (new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
						if (tagInfo!=null && tagInfo.Length>1) {
							string key = tagInfo[0];
							string value = tagInfo[1];
							if (!tags.ContainsKey(key)) tags.Add (key,value);
						}
					}
					MountPoint mountPoint = new MountPoint (name, countryIndex, provinceIndex, new Vector2(x,y), type, tags);
					mountPoints.Add (mountPoint);
				}
			}
		}
	
	#endregion

	#region Drawing stuff

		/// <summary>
		/// Redraws the mounts points but only in editor time. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
		/// </summary>
		public void DrawMountPoints() {
			// Create mount points layer
			Transform t = transform.Find ("Mount Points");
			if (t != null)
				DestroyImmediate (t.gameObject);
			if (Application.isPlaying || mountPoints==null) return;

			mountPointsLayer = new GameObject ("Mount Points");
			mountPointsLayer.transform.SetParent (transform, false);

			// Draw mount points marks
			for (int k=0; k<mountPoints.Count; k++) {
				MountPoint mp = mountPoints [k];
				GameObject mpObj = Instantiate (mountPointSpot); 
				mpObj.name = k.ToString();
				mpObj.transform.position = transform.TransformPoint(mp.unity2DLocation);
				mpObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				mpObj.transform.SetParent (mountPointsLayer.transform, true);
			}

			MountPointScaler mpScaler = mountPointsLayer.GetComponent<MountPointScaler>() ?? mountPointsLayer.AddComponent<MountPointScaler>();
			mpScaler.map = this;
			mpScaler.ScaleMountPoints();
		}


	#endregion

		#region Internal Cities API

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		public int GetMountPointNearPoint(Vector2 localPoint) {
			return GetMountPointNearPoint(localPoint, MOUNT_POINT_HIT_PRECISION);
		}

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		/// <param name="separation">Distance threshold (minimum should be MOUNT_POINT_HIT_PRECISION constant).</param>
		public int GetMountPointNearPoint(Vector2 localPoint, float separation) {
			if (mountPoints==null) return -1;
			if (separation < MOUNT_POINT_HIT_PRECISION) separation = MOUNT_POINT_HIT_PRECISION;
			float separationSqr = separation * separation;
			for (int c=0;c<mountPoints.Count;c++) {
				Vector2 mpLoc = mountPoints[c].unity2DLocation;
				float distSqr = (mpLoc-localPoint).sqrMagnitude;
				if (distSqr < separationSqr) {
					return c;
				}
			}
			return -1;
		}

		bool GetMountPointUnderMouse(int countryIndex, Vector2 localPoint, out int mountPointIndex) {
			float hitPrecission = MOUNT_POINT_HIT_PRECISION * _cityIconSize * 5.0f;
			for (int c=0;c<mountPoints.Count;c++) {
				MountPoint mp = mountPoints[c];
				if (mp.countryIndex == countryIndex) {
					if ( (mp.unity2DLocation-localPoint).magnitude < hitPrecission) {
						mountPointIndex = c;
						return true;
					}
				}
			}
			mountPointIndex = -1;
			return false;
		}


		/// <summary>
		/// Returns mount points belonging to a provided country.
		/// </summary>
		List<MountPoint>GetMountPoints(int countryIndex) {
			List<MountPoint>results = new List<MountPoint>(20);
			for (int c=0;c<mountPoints.Count;c++) {
				if (mountPoints[c].countryIndex==countryIndex) results.Add (mountPoints[c]);
			}
			return results;
		}

		/// <summary>
		/// Returns mount points belonging to a provided country and province.
		/// </summary>
		List<MountPoint>GetMountPoints(int countryIndex, int provinceIndex) {
			List<MountPoint>results = new List<MountPoint>(20);
			for (int c=0;c<mountPoints.Count;c++) {
				if (mountPoints[c].countryIndex==countryIndex && mountPoints[c].provinceIndex == provinceIndex) results.Add (mountPoints[c]);
			}
			return results;
		}


		/// <summary>
		/// Updates the mount points scale.
		/// </summary>
		public void ScaleMountPoints() {
			if (mountPointsLayer!=null) {
				MountPointScaler scaler = mountPointsLayer.GetComponent<MountPointScaler>();
				if (scaler!=null) scaler.ScaleMountPoints();
			}
		}

		#endregion
	}

}