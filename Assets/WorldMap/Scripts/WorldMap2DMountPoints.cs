// World Political Map - 2D Edition for Unity - Main Script
// Copyright 2015-2017 Kronnect
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM
// ***************************************************************************
// This is the public API file - every property or public method belongs here
// ***************************************************************************

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace WPMF {

	/* Public WPM Class */
	public partial class WorldMap2D : MonoBehaviour {

		/// <summary>
		/// Complete list of mount points.
		/// </summary>
		[NonSerialized]
		public List<MountPoint>	mountPoints;


	#region Public API area	

		/// <summary>
		/// Clears any mount point highlighted (color changed) and resets them to default city color (used from Editor)
		/// </summary>
		public void HideMountPointHighlights () {
			if (mountPointsLayer == null)
				return;
			Renderer[] rr = mountPointsLayer.GetComponentsInChildren<Renderer>(true);
			for (int k=0;k<rr.Length;k++) 
				rr[k].sharedMaterial = mountPointsMat;
		}
		
		/// <summary>
		/// Toggles the mount point highlight.
		/// </summary>
		/// <param name="mountPointIndex">Moint point index in the mount points collection.</param>
		/// <param name="color">Color.</param>
		/// <param name="highlighted">If set to <c>true</c> the color of the mount point will be changed. If set to <c>false</c> the color of the mount point will be reseted to default color</param>
		public void ToggleMountPointHighlight (int mountPointIndex, Color color, bool highlighted) {
			if (mountPointsLayer == null)
				return;
			Transform t = mountPointsLayer.transform.Find (mountPointIndex.ToString());
			if (t == null)
				return;
			Renderer rr = t.gameObject.GetComponent<Renderer> ();
			if (rr == null)
				return;
			Material mat;
			if (highlighted) {
				mat = Instantiate (rr.sharedMaterial);
				mat.name = rr.sharedMaterial.name;
				mat.hideFlags = HideFlags.DontSave;
				mat.color = color;
				rr.sharedMaterial = mat;
			} else {
				rr.sharedMaterial = mountPointsMat;
			}
		}

		
		/// <summary>
		/// Returns an array with the mount points names.
		/// </summary>
		public string[] GetMountPointNames () {
			return GetMountPointNames(-1,-1);
		}
		
		/// <summary>
		/// Returns an array with the mount points names.
		/// </summary>
		public string[] GetMountPointNames (int countryIndex) {
			return GetMountPointNames(countryIndex,-1);		}

		
		/// <summary>
		/// Returns an array with the mount points names.
		/// </summary>
		public string[] GetMountPointNames (int countryIndex, int provinceIndex) {
			List<string> c = new List<string> (20);
			for (int k=0; k<mountPoints.Count; k++) {
				if ( (mountPoints[k].countryIndex == countryIndex || countryIndex==-1) &&
				    (mountPoints[k].provinceIndex == provinceIndex || provinceIndex==-1)) {
					c.Add (mountPoints [k].name + " (" + k + ")");
				}
			}
			c.Sort ();
			return c.ToArray ();
		}

		/// <summary>
		/// Returns the index of a mount point in the global mount points collection. Note that country (and optionally province) index can be supplied due to repeated mount point names.
		/// Pass -1 to countryIndex or provinceIndex to ignore filters.
		/// </summary>
		public int GetMountPointIndex (int countryIndex, int provinceIndex, string mountPointName) {
			if (mountPoints==null) return -1;
			for (int k=0; k<mountPoints.Count; k++) {
				if ((mountPoints [k].countryIndex == countryIndex || countryIndex==-1) && 
				    (mountPoints [k].provinceIndex == provinceIndex || provinceIndex==-1) && 
				    mountPoints[k].name.Equals (mountPointName)) {
					return k;
				}
			}
			return -1;
		}

		
		/// <summary>
		/// Returns the mount point index by screen position.
		/// </summary>
		public bool GetMountPointIndex (Ray ray, out int mountPointIndex) {
			RaycastHit[] hits = Physics.RaycastAll (ray, 5000, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector3 localHit = transform.InverseTransformPoint (hits [k].point);
						int c = GetMountPointNearPoint (localHit);
						if (c >= 0) {
							mountPointIndex = c;
							return true;
						}
					}
				}
			}
			mountPointIndex = -1;
			return false;
		}


		
		/// <summary>
		/// Deletes all mount points of current selected country's continent
		/// </summary>
		public void MountPointsDeleteFromSameContinent(string continentName) {
			HideMountPointHighlights();
			int k=-1;
			while(++k<mountPoints.Count) {
				int cindex = mountPoints[k].countryIndex;
				if (cindex>=0) {
					string mpContinent = countries[cindex].continent;
					if (mpContinent.Equals(continentName)) {
						mountPoints.RemoveAt(k);
						k--;
					}
				}
			}
		}


		public void MountPointAdd(Vector2 location, string name, int countryIndex, int provinceIndex, int type) {
			MountPoint newMountPoint = new MountPoint(name, countryIndex, provinceIndex, location, type);
			if (mountPoints==null) mountPoints = new List<MountPoint>();
			mountPoints.Add (newMountPoint);
		}

		
		/// <summary>
		/// Returns a list of mount points contained in a given region
		/// </summary>
		public List<MountPoint> GetMountPoints (Region region) {
			int mpCount = mountPoints.Count;
			List<MountPoint> cc = new List<MountPoint> ();
			for (int k = 0; k < mpCount; k++) {
				if (region.Contains (mountPoints[k].unity2DLocation))
					cc.Add (mountPoints [k]);
			}
			return cc;
		}

		#endregion


	}

}