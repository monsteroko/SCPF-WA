using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace WPMF {

	public partial class WorldMap2D_Editor : MonoBehaviour {

		public int GUIMountPointIndex;
		public string GUIMountPointName = "";
		public string GUIMountPointNewName = "";
		public string GUIMountPointNewType = "";
		public string GUIMountPointNewTagKey = "";
		public string GUIMountPointNewTagValue = "";
		public int GUIMountPointMassPopulationMin = 1;
		public int GUIMountPointMassPopulationMax = 3;
		public float GUIMountPointMassPopulationSeparation = 10;
		public string GUIMountPointMassPopulationType = "";
		public int GUIMountPointMassPopulationTypeInteger { get { int tt = 0; int.TryParse(GUIMountPointMassPopulationType, out tt); return tt; } }
		public int mountPointIndex = -1;
		public bool mountPointChanges;  // if there's any pending change to be saved
		public Dictionary<string, string>mountPointTags;

		// private fields
		int lastMountPointCount = -1;
		string[] _mountPointNames;
				    

		public string[] mountPointNames {
			get {
				if (map.mountPoints!=null && lastMountPointCount != map.mountPoints.Count) {
					mountPointIndex =-1;
					ReloadMountPointNames ();
				}
				return _mountPointNames;
			}
		}

		
		#region Editor functionality

		
		public void ClearMountPointSelection() {
			map.HideMountPointHighlights();
			mountPointIndex = -1;
			GUIMountPointIndex = -1;
			GUIMountPointName = "";
			GUIMountPointNewName = "";
			GUIMountPointNewTagKey = "";
			GUIMountPointNewTagValue = "";
			GUIMountPointNewType = "";
		}


		/// <summary>
		/// Adds a new mount point to current country.
		/// </summary>
		public void MountPointCreate(Vector3 newPoint) {
			if (countryIndex<0) return;
			GUIMountPointName = "New Mount Point " + (map.mountPoints.Count+1);
			map.MountPointAdd(newPoint, GUIMountPointName, countryIndex, provinceIndex, 0);
			map.DrawMountPoints();
			lastMountPointCount = -1;
			ReloadMountPointNames();
			mountPointChanges = true;
		}

		public bool MountPointUpdateType () {
			if (mountPointIndex<0) return false;
			int type =map.mountPoints[mountPointIndex].type;
			int.TryParse(GUIMountPointNewType, out type);
			map.mountPoints[mountPointIndex].type = type;
			mountPointChanges = true;
			return true;
		}


		public bool MountPointRename () {
			if (mountPointIndex<0) return false;
			string prevName = map.mountPoints[mountPointIndex].name;
			GUIMountPointNewName = GUIMountPointNewName.Trim ();
			if (prevName.Equals(GUIMountPointNewName)) return false;
			map.mountPoints[mountPointIndex].name = GUIMountPointNewName;
			GUIMountPointName = GUIMountPointNewName;
			lastMountPointCount = -1;
			ReloadMountPointNames();
			map.DrawMountPoints();
			mountPointChanges = true;
			return true;
		}

		public bool MountPointAddNewTag() {
			if (mountPointIndex<0) return false;
			MountPoint mp = map.mountPoints[mountPointIndex];
			if (!mp.customTags.ContainsKey(GUIMountPointNewTagKey)) {
				mp.customTags.Add (GUIMountPointNewTagKey, GUIMountPointNewTagValue);
				GUIMountPointNewTagKey = "";
				GUIMountPointNewTagValue = "";
			    mountPointChanges = true;
				return true;
			}
			return false;
		}

		public void MountPointMove(Vector2 destination) {
			if (mountPointIndex<0) return;
			map.mountPoints[mountPointIndex].unity2DLocation = destination;
			Transform t = map.transform.Find("Mount Points/" + mountPointIndex.ToString());
			if (t!=null) t.localPosition = destination * 1.001f;
			mountPointChanges = true;
		}

		public void MountPointSelectByCombo (int selection) {
			GUIMountPointIndex = selection;
			GUIMountPointName = "";
			GetMountPointIndexByGUISelection();
			MountPointSelect ();
		}

		bool GetMountPointIndexByGUISelection() {
			if (GUIMountPointIndex<0 || GUIMountPointIndex>=mountPointNames.Length) return false;
			string[] s = mountPointNames [GUIMountPointIndex].Split (new char[] {
				'(',
				')'
			}, System.StringSplitOptions.RemoveEmptyEntries);
			if (s.Length >= 2) {
				GUIMountPointName = s [0].Trim ();
				if (int.TryParse (s [1], out mountPointIndex)) {
					return true;
				}
			}
			return false;
		}

		public void MountPointSelect() {
			if (mountPointIndex < 0 || mountPointIndex > map.mountPoints.Count)
				return;

			// If no country is selected (the mount point could be at sea) select it
			MountPoint mp = map.mountPoints[mountPointIndex];
			int mpCountryIndex = mp.countryIndex;
			if (mpCountryIndex<0) {
				SetInfoMsg("Country not found in this country file.");
			}

			if (countryIndex!=mpCountryIndex && mpCountryIndex>=0) {
				ClearSelection();
				countryIndex = mpCountryIndex;
				countryRegionIndex = map.countries[countryIndex].mainRegionIndex;
				CountryRegionSelect();
			} 

			// Just in case makes GUICountryIndex selects appropiate value in the combobox
			GUIMountPointName = mp.name;
			SyncGUIMountPointSelection();
			if (mountPointIndex>=0) {
				GUIMountPointNewName = mp.name;
				GUIMountPointNewType = mp.type.ToString();
				MountPointHighlightSelection();
			}
		}

		public bool MountPointSelectByScreenClick(Ray ray) {
			int targetMountPointIndex;
			if (map.GetMountPointIndex (ray, out targetMountPointIndex)) {
				mountPointIndex = targetMountPointIndex;
				MountPointSelect();
				return true;
			}
			return false;
		}

		void MountPointHighlightSelection() {

			if (mountPointIndex<0 || mountPointIndex>=map.mountPoints.Count) return;

			// Colorize mount point
			map.HideMountPointHighlights();
			map.ToggleMountPointHighlight(mountPointIndex, Color.green, true);
	    }

		
		public void ReloadMountPointNames () {
			if (map == null || map.mountPoints == null) {
				lastMountPointCount = -1;
				return;
			}
			lastMountPointCount = map.mountPoints.Count; // check this size, and not result from GetCityNames because it could return additional rows (separators and so)
			_mountPointNames = map.GetMountPointNames(countryIndex, provinceIndex);
			SyncGUIMountPointSelection();
			MountPointSelect(); // refresh selection
		}

		void SyncGUIMountPointSelection() {
			// recover GUI mount point index selection
			if (GUIMountPointName.Length>0) {
				for (int k=0; k<mountPointNames.Length; k++) { 
					if (_mountPointNames [k].TrimStart ().StartsWith (GUIMountPointName)) {
						GUIMountPointIndex = k;
						mountPointIndex = map.GetMountPointIndex(countryIndex, provinceIndex, GUIMountPointName);
						return;
					}
				}
				SetInfoMsg("Mount point " + GUIMountPointName + " not found in database.");
			}
			GUIMountPointIndex = -1;
			GUIMountPointName = "";
		}

		/// <summary>
		/// Deletes current mount point
		/// </summary>
		public void DeleteMountPoint() {
			if (map.mountPoints==null || mountPointIndex<0 || mountPointIndex>=map.mountPoints.Count) return;

			map.HideMountPointHighlights();
			map.mountPoints.RemoveAt(mountPointIndex);
			mountPointIndex = -1;
			GUIMountPointName = "";
			SyncGUIMountPointSelection();
			map.DrawMountPoints();
			mountPointChanges = true;
		}

		/// <summary>
		/// Deletes all mount points of current selected country
		/// </summary>
		public void DeleteCountryMountPoints() {
			if (countryIndex<0) return;
			
			map.HideMountPointHighlights();
			if (map.mountPoints != null) {
				int k=-1;
				while(++k<map.mountPoints.Count) {
					if (map.mountPoints[k].countryIndex == countryIndex) {
						map.mountPoints.RemoveAt(k);
						k--;
					}
				}
			}
			mountPointIndex = -1;
			GUIMountPointName = "";
			SyncGUIMountPointSelection();
			map.DrawMountPoints();
			mountPointChanges = true;
		}

		void TransferMountPoints(int countryIndex, int targetCountryIndex) {
			int mpCount = _map.mountPoints.Count;
			for (int k=0;k<mpCount;k++) {
				if (_map.mountPoints[k].countryIndex == countryIndex) {
					_map.mountPoints[k].countryIndex = targetCountryIndex;
					mountPointChanges = true;
				}
			}
		}

		void TransferRegionMountPoints(int countryIndex, Region countryRegion, int targetCountryIndex) {
			int mpCount = _map.mountPoints.Count;
			for (int k=0;k<mpCount;k++) {
				MountPoint mp = _map.mountPoints[k];
				if (mp.countryIndex == countryIndex && countryRegion.Contains(mp.unity2DLocation)) {
					mp.countryIndex = targetCountryIndex;
					mountPointChanges = true;
				}
			}
		}

        void TransferProvinceRegionMountPoints(int provinceIndex, int targetProvinceIndex)
        {
            int mpCount = _map.mountPoints.Count;
            for (int k = 0; k < mpCount; k++)
            {
                MountPoint mp = _map.mountPoints[k];
                if (mp.provinceIndex == provinceIndex)
                {
                    mp.provinceIndex = targetProvinceIndex;
                    mountPointChanges = true;
                }
            }
        }

        #endregion

        #region IO stuff

        /// <summary>
        /// Returns the file name corresponding to the current mount point data file
        /// </summary>
        public string GetMountPointGeoDataFileName() {
			return "mountPoints.txt";
		}
		
		/// <summary>
		/// Exports the geographic data in packed string format.
		/// </summary>
		public string GetMountPointsGeoData () {
			StringBuilder sb = new StringBuilder ();
			for (int k=0; k<map.mountPoints.Count; k++) {
				MountPoint mp = map.mountPoints[k];
				if (k > 0)
					sb.Append ("|");
				sb.Append (DataEscape(mp.name) + "$");
				string province = "";
				if (mp.provinceIndex>=0 && mp.provinceIndex<map.provinces.Length) province = map.provinces[mp.provinceIndex].name;
				string country = "";
				if (mp.countryIndex>=0 && mp.countryIndex<map.countries.Length) country = map.countries[mp.countryIndex].name;
				sb.Append (province + "$");
				sb.Append (country + "$");
				sb.Append (mp.type + "$");
				sb.Append (mp.unity2DLocation.x.ToString(CultureInfo.InvariantCulture) + "$");
				sb.Append (mp.unity2DLocation.y.ToString(CultureInfo.InvariantCulture) + "$");
				int tc = 0;
				foreach(string key in mp.customTags.Keys) {
					if (tc++>0) sb.Append("$");
					sb.Append(key);
					sb.Append("&");
					sb.Append(DataEscape(mp.customTags[key]));
				}
			}
			return sb.ToString ();
		}

		#endregion

	}
}
