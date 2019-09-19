// World Political Map - 2D Edition for Unity - Main Script
// Copyright 2015-2017 Kronnect
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

//#define TRACE_CTL
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPMF
{
	public partial class WorldMap2D : MonoBehaviour
	{

		// resources
		Material provincesMat;
		Material hudMatProvince;

		// gameObjects
		GameObject provincesObj, provinceRegionHighlightedObj;
		GameObject provinceCountryOutlineRef;	// maintains a reference to the country outline to hide it in provinces mode when mouse exits the country

		// caché and gameObject lifetime control
		int countryProvincesDrawnIndex;
		Dictionary<Province, int>_provinceLookup;
		int lastProvinceLookupCount = -1;

		Dictionary<Province, int>provinceLookup {
			get {
				if (_provinceLookup != null && provinces.Length == lastProvinceLookupCount)
					return _provinceLookup;
				if (_provinceLookup == null) {
					_provinceLookup = new Dictionary<Province,int> ();
				} else {
					_provinceLookup.Clear ();
				}
				for (int k=0; k<provinces.Length; k++) {
					_provinceLookup.Add (provinces [k], k);
				}
				lastProvinceLookupCount = provinces.Length;
				return _provinceLookup;
			}
		}

		/// <summary>
		/// Used internally by the Map Editor. It will recalculate de boundaries and optimize frontiers based on new data of provinces array
		/// </summary>
		public void RefreshProvinceDefinition (int provinceIndex)
		{
			if (provinceIndex < 0 || provinceIndex >= provinces.Length)
				return;
			RefreshProvinceGeometry (provinceIndex);
			DrawProvinces (provinces [provinceIndex].countryIndex, true, true);
		}

		/// <summary>
		/// Used internally by the Map Editor. It will recalculate de boundaries and optimize frontiers based on new data of provinces array
		/// </summary>
		public void RefreshProvinceGeometry (int provinceIndex)
		{
			lastProvinceLookupCount = -1;
			if (provinceIndex < 0 || provinceIndex >= provinces.Length)
				return;
			float maxVol = 0;
			Vector2 minProvince = Misc.Vector2one * 10;
			Vector2 maxProvince = -minProvince;
			Province province = provinces [provinceIndex];
			if (province.regions == null)
				ReadProvincePackedString (province);
			int regionCount = province.regions.Count;
			for (int r=0; r<regionCount; r++) {
				Region provinceRegion = province.regions [r];
				provinceRegion.entity = province;	// just in case one country has been deleted
				provinceRegion.regionIndex = r;				// just in case a region has been deleted
				int coorCount = provinceRegion.points.Length;
				Vector3 min = Misc.Vector3one * 10;
				Vector3 max = -min;
				for (int c=0; c<coorCount; c++) {
					float x = provinceRegion.points [c].x;
					float y = provinceRegion.points [c].y;
					if (x < min.x)
						min.x = x;
					if (x > max.x)
						max.x = x;
					if (y < min.y)
						min.y = y;
					if (y > max.y)
						max.y = y;
				}
				Vector3 normRegionCenter = (min + max) * 0.5f;
				provinceRegion.center = normRegionCenter; 
				
				// Calculate country bounding rect
				if (min.x < minProvince.x)
					minProvince.x = min.x;
				if (min.y < minProvince.y)
					minProvince.y = min.y;
				if (max.x > maxProvince.x)
					maxProvince.x = max.x;
				if (max.y > maxProvince.y)
					maxProvince.y = max.y;
				provinceRegion.rect2D = new Rect (min.x, min.y, Math.Abs (max.x - min.x), Mathf.Abs (max.y - min.y));
				float vol = (max - min).sqrMagnitude;
				if (vol > maxVol) {
					maxVol = vol;
					province.mainRegionIndex = r;
					province.center = provinceRegion.center;
				}
			}
			province.regionsRect2D = new Rect (minProvince.x, minProvince.y, Math.Abs (maxProvince.x - minProvince.x), Mathf.Abs (maxProvince.y - minProvince.y));
		}

		void ReadProvincesPackedString ()
		{

			lastProvinceLookupCount = -1;
			TextAsset ta = Resources.Load<TextAsset> ("Geodata/regions");
			string s = ta.text;
			string[] provincesPackedStringData = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			char[] separatorProvinces = new char[] { '$' };
			int provinceCount = provincesPackedStringData.Length;
			List<Province> newProvinces = new List<Province> (provinceCount);
			List<Province>[] countryProvinces = new List<Province>[countries.Length];
			for (int k=0; k<provinceCount; k++) {
				string[] provinceInfo = provincesPackedStringData [k].Split (separatorProvinces, StringSplitOptions.RemoveEmptyEntries);
				if (provinceInfo.Length <= 2)
					continue;
				string name = provinceInfo [0];
				string countryName = provinceInfo [1];
				int countryIndex = GetCountryIndex (countryName);
				if (countryIndex >= 0) {
					Province province = new Province (name, countryIndex);
					province.packedRegions = provinceInfo [2];
					newProvinces.Add (province);
					if (countryProvinces [countryIndex] == null)
						countryProvinces [countryIndex] = new List<Province> (50);
					countryProvinces [countryIndex].Add (province);
				} 
			}
			_provinces = newProvinces.ToArray ();
			for (int k=0; k<countries.Length; k++) {
				if (countryProvinces [k] != null) {
					countries [k].provinces = countryProvinces [k].ToArray ();
				} 
			}
				
		}

		/// <summary>
		/// Unpacks province geodata information. Used by Map Editor.
		/// </summary>
		/// <param name="province">Province.</param>
		public void ReadProvincePackedString (Province province)
		{
			string[] regions = province.packedRegions.Split (new char[] {'*'}, StringSplitOptions.RemoveEmptyEntries);
			int regionCount = regions.Length;
			province.regions = new List<Region> (regionCount);
			float maxVol = float.MinValue;
			Vector2 minProvince = Misc.Vector2one * 10;
			Vector2 maxProvince = -minProvince;
			char[] separatorRegions = new char[] { ';' };
			for (int r=0; r<regionCount; r++) {
				string[] coordinates = regions [r].Split (separatorRegions, StringSplitOptions.RemoveEmptyEntries);
				int coorCount = coordinates.Length;
				if (coorCount < 3)
					continue;
				Vector3 min = Misc.Vector3one * 10;
				Vector3 max = -min;
				Region provinceRegion = new Region (province, province.regions.Count);
				provinceRegion.points = new Vector3[coorCount];
				for (int c=0; c<coorCount; c++) {
					float x, y;
					GetPointFromPackedString (coordinates [c], out x, out y);

					Vector2 point = new Vector2 (x, y);
					provinceRegion.points [c] = point;
					if (x < min.x)
						min.x = x;
					if (x > max.x)
						max.x = x;
					if (y < min.y)
						min.y = y;
					if (y > max.y)
						max.y = y;
				}
				Vector3 normRegionCenter = (min + max) * 0.5f;
				provinceRegion.center = normRegionCenter; 
				province.regions.Add (provinceRegion);

				// Calculate country bounding rect
				if (min.x < minProvince.x)
					minProvince.x = min.x;
				if (min.y < minProvince.y)
					minProvince.y = min.y;
				if (max.x > maxProvince.x)
					maxProvince.x = max.x;
				if (max.y > maxProvince.y)
					maxProvince.y = max.y;
				provinceRegion.rect2D = new Rect (min.x, min.y, Math.Abs (max.x - min.x), Mathf.Abs (max.y - min.y));
				float vol = (max - min).sqrMagnitude;
				if (vol > maxVol) {
					maxVol = vol;
					province.mainRegionIndex = r;
					province.center = provinceRegion.center;
				}
			}
			province.regionsRect2D = new Rect (minProvince.x, minProvince.y, Math.Abs (maxProvince.x - minProvince.x), Mathf.Abs (maxProvince.y - minProvince.y));
		}
	

	#region Drawing stuff


		/// <summary>
		/// Draws all countries provinces.
		/// </summary>
		void DrawAllProvinceBorders (bool forceRefresh)
		{

			if (!gameObject.activeInHierarchy)
				return;
			if (provincesObj != null && !forceRefresh)
				return;
			HideProvinces ();
			if (!_showProvinces || !_drawAllProvinces)
				return;

			int numCountries = countries.Length;
			List<Country> targetCountries = new List<Country> (numCountries);
			for (int k=0; k<numCountries; k++) {
				if (countries [k].allowShowProvinces) {
					targetCountries.Add (countries [k]);
				}
			}
			DrawProvinces (targetCountries, true); 
		}


		/// <summary>
		/// Draws the provinces for specified country and optional also neighbours'
		/// </summary>
		/// <returns><c>true</c>, if provinces was drawn, <c>false</c> otherwise.</returns>
		/// <param name="countryIndex">Country index.</param>
		/// <param name="includeNeighbours">If set to <c>true</c> include neighbours.</param>
		bool mDrawProvinces (int countryIndex, bool includeNeighbours, bool forceRefresh)
		{

			if (!gameObject.activeInHierarchy || provinces == null)	// asset not ready - return
				return false;

			if (countryProvincesDrawnIndex == countryIndex && provincesObj != null && !forceRefresh)	// existing gameobject containing province borders?
				return false;

			bool res;
			if (_drawAllProvinces) {
				DrawAllProvinceBorders (false);
				res = true;
			} else {
				if (!countries [countryIndex].allowShowProvinces)
					return false;

				// prepare a list with the countries to be drawn
				countryProvincesDrawnIndex = countryIndex;
				List<Country> targetCountries = new List<Country> (20);
				// add selected country
				targetCountries.Add (countries [countryIndex]);
				// add neighbour countries?
				if (includeNeighbours) {
					for (int k=0; k<countries[countryIndex].regions.Count; k++) {
						List<Region> neighbours = countries [countryIndex].regions [k].neighbours;
						for (int n=0; n<neighbours.Count; n++) {
							Country c = (Country)neighbours [n].entity;
							if (!targetCountries.Contains (c))
								targetCountries.Add (c);
						}
					}
				}
				res = DrawProvinces (targetCountries, forceRefresh);
			}

			if (res && _showOutline) {
				Country country = countries [countryIndex];
				Region region = country.regions [country.mainRegionIndex];
				provinceCountryOutlineRef = DrawCountryRegionOutline (region, provincesObj);
			}

			return res;

		}

		bool DrawProvinces (List<Country>targetCountries, bool forceRefresh)
		{

			// optimize required lines
			if (frontiersPoints == null) {
				frontiersPoints = new List<Vector3> (1000000);
			} else {
				frontiersPoints.Clear ();
			}
			if (frontiersCacheHit == null) {
				frontiersCacheHit = new Dictionary<double, Region> (500000);
			} else {
				frontiersCacheHit.Clear ();
			}
			for (int c=0; c<targetCountries.Count; c++) {
				Country targetCountry = targetCountries [c];
				if (targetCountry.provinces == null)
					continue;
				for (int p=0; p<targetCountry.provinces.Length; p++) {
					Province province = targetCountry.provinces [p];
					if (province.regions == null) { // read province data the first time we need it
						ReadProvincePackedString (province);
					}
					for (int r=0; r<province.regions.Count; r++) {
						Region region = province.regions [r];
						region.entity = province;
						region.regionIndex = r;
						region.neighbours.Clear ();
						int numPoints = region.points.Length - 1;
						for (int i = 0; i<numPoints; i++) {
							Vector3 p0 = region.points [i];
							Vector3 p1 = region.points [i + 1];
							double v = (p0.x + p1.x) + MAP_PRECISION * (p0.y + p1.y);
							if (frontiersCacheHit.ContainsKey (v)) {
								Region neighbour = frontiersCacheHit [v];
								if (neighbour != region) {
									if (!region.neighbours.Contains (neighbour)) {
										region.neighbours.Add (neighbour);
										neighbour.neighbours.Add (region);
									}
								}
							} else {
								frontiersCacheHit.Add (v, region);
								frontiersPoints.Add (p0);
								frontiersPoints.Add (p1);
							}
						}
						// Close the polygon
						frontiersPoints.Add (region.points [numPoints]);
						frontiersPoints.Add (region.points [0]);
					}
				}
			}

			int meshGroups = (frontiersPoints.Count / 65000) + 1;
			int meshIndex = -1;
			int[][] provincesIndices = new int[meshGroups][];
			Vector3[][] provincesBorders = new Vector3[meshGroups][];
			for (int k=0; k<frontiersPoints.Count; k+=65000) {
				int max = Mathf.Min (frontiersPoints.Count - k, 65000); 
				provincesBorders [++meshIndex] = new Vector3[max];
				provincesIndices [meshIndex] = new int[max];
				for (int j=k; j<k+max; j++) {
					provincesBorders [meshIndex] [j - k] = frontiersPoints [j];
					provincesIndices [meshIndex] [j - k] = j - k;
				}
			}

			// Create province layer if needed
			if (provincesObj != null)
				DestroyImmediate (provincesObj);

			provincesObj = new GameObject ("Provinces");
			provincesObj.hideFlags = HideFlags.DontSave;
			provincesObj.transform.SetParent (transform, false);
			provincesObj.transform.localPosition = Misc.Vector3back * 0.002f;
			provincesObj.layer = gameObject.layer;

			for (int k=0; k<provincesBorders.Length; k++) {
				GameObject flayer = new GameObject ("flayer");
				flayer.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				flayer.transform.SetParent (provincesObj.transform, false);
				flayer.transform.localPosition = Misc.Vector3zero;
				flayer.transform.localRotation = Quaternion.Euler (Misc.Vector3zero);
				flayer.layer = provincesObj.layer;

				Mesh mesh = new Mesh ();
				mesh.vertices = provincesBorders [k];
				mesh.SetIndices (provincesIndices [k], MeshTopology.Lines, 0);
				mesh.RecalculateBounds ();
				mesh.hideFlags = HideFlags.DontSave;
			
				MeshFilter mf = flayer.AddComponent<MeshFilter> ();
				mf.sharedMesh = mesh;
			
				MeshRenderer mr = flayer.AddComponent<MeshRenderer> ();
				mr.receiveShadows = false;
				mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
//				mr.useLightProbes = false;
				mr.sharedMaterial = provincesMat;
			}
			return true;
		}

	#endregion



	#region Province highlighting

		int GetCacheIndexForProvinceRegion (int provinceIndex, int regionIndex)
		{
			return 1000000 + provinceIndex * 1000 + regionIndex;
		}

		/// <summary>
		/// Highlights the province region specified. Returns the generated highlight surface gameObject.
		/// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a country region.
		/// </summary>
		/// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
		public GameObject HighlightProvinceRegion (int provinceIndex, int regionIndex, bool refreshGeometry)
		{
			if (!refreshGeometry && _provinceHighlightedIndex == provinceIndex && _provinceRegionHighlightedIndex == regionIndex)
				return provinceRegionHighlightedObj;
			if (provinceRegionHighlightedObj != null)
				HideProvinceRegionHighlight ();
			if (provinceIndex < 0 || provinceIndex >= provinces.Length || provinces [provinceIndex].regions == null || regionIndex < 0 || regionIndex >= provinces [provinceIndex].regions.Count)
				return null;

			if (OnProvinceHighlight != null) {
				bool allowHighlight = true;
				OnProvinceHighlight (provinceIndex, regionIndex, ref allowHighlight);
				if (!allowHighlight)
					return null;
			}

			int cacheIndex = GetCacheIndexForProvinceRegion (provinceIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (refreshGeometry && existsInCache) {
				GameObject obj = surfaces [cacheIndex];
				surfaces.Remove (cacheIndex);
				DestroyImmediate (obj);
				existsInCache = false;
			}

			if (_enableProvinceHighlight) {
				if (existsInCache) {
					provinceRegionHighlightedObj = surfaces [cacheIndex];
					if (provinceRegionHighlightedObj != null) {
						provinceRegionHighlightedObj.SetActive (true);
						provinceRegionHighlightedObj.GetComponent<Renderer> ().sharedMaterial = hudMatProvince;
					}
				} else {
					provinceRegionHighlightedObj = GenerateProvinceRegionSurface (provinceIndex, regionIndex, hudMatProvince);
				}
			}
			_provinceHighlighted = provinces [provinceIndex];
			_provinceHighlightedIndex = provinceIndex;
			_provinceRegionHighlighted = _provinceHighlighted.regions [regionIndex];
			_provinceRegionHighlightedIndex = regionIndex;
			return provinceRegionHighlightedObj;
		}

		void HideProvinceRegionHighlight ()
		{
			if (provinceCountryOutlineRef != null && _countryRegionHighlighted == null)
				provinceCountryOutlineRef.SetActive (false);
			if (provinceRegionHighlighted == null)
				return;
			if (provinceRegionHighlightedObj != null) {
				if (provinceRegionHighlighted.customMaterial != null) {
					ApplyMaterialToSurface (provinceRegionHighlightedObj, provinceRegionHighlighted.customMaterial);
				} else {
					provinceRegionHighlightedObj.SetActive (false);
				}
				provinceRegionHighlightedObj = null;
			}

			// Raise exit event
			if (OnProvinceExit != null)
				OnProvinceExit (_provinceHighlightedIndex, _provinceRegionHighlightedIndex);

			_provinceHighlighted = null;
			_provinceHighlightedIndex = -1;
			_provinceRegionHighlighted = null;
			_provinceRegionHighlightedIndex = -1;
		}
		
		GameObject GenerateProvinceRegionSurface (int provinceIndex, int regionIndex, Material material)
		{
			if (provinces [provinceIndex].regions == null)
				ReadProvincePackedString (provinces [provinceIndex]);
			if (provinceIndex < 0 || provinceIndex >= provinces.Length || provinces [provinceIndex].regions == null || regionIndex < 0 || regionIndex >= provinces [provinceIndex].regions.Count)
				return null;
			Region region = provinces [provinceIndex].regions [regionIndex];
			int[] surfIndices = Triangulator.GetPoints (region.points);
			int cacheIndex = GetCacheIndexForProvinceRegion (provinceIndex, regionIndex); 
			string cacheIndexSTR = cacheIndex.ToString ();
			// Deletes potential residual surface
			Transform t = surfacesLayer.transform.Find (cacheIndexSTR);
			if (t != null)
				DestroyImmediate (t.gameObject);
			GameObject surf = Drawing.CreateSurface (cacheIndexSTR, region.points, surfIndices, material);									
			surf.transform.SetParent (surfacesLayer.transform, false);
			surf.transform.localPosition = Misc.Vector3zero;
			surf.layer = gameObject.layer;
			if (surfaces.ContainsKey (cacheIndex))
				surfaces.Remove (cacheIndex);
			surfaces.Add (cacheIndex, surf);
			return surf;
		}

		#endregion


	}

}