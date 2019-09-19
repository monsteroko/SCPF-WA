using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	[RequireComponent(typeof(WorldMap2D))]
	[Serializable]
	public class WorldMap2D_Decorator : MonoBehaviour {

		public const int NUM_GROUPS = 32;

		GameObject _decoratorLayer;
		public GameObject decoratorLayer {
			get {
				if (_decoratorLayer==null) {
					Transform t = map.transform.Find("DecoratorLayer");
					if (t== null) {
						_decoratorLayer = new GameObject("DecoratorLayer");
						_decoratorLayer.transform.SetParent(map.transform, false);
					} else {
						_decoratorLayer = t.gameObject;
					}
				}
				return _decoratorLayer;
			}
		}

		public bool groupByContinent;
		public int GUIGroupIndex;
		public int GUICountryIndex = -1;
		public string GUICountryName = "";

		[NonSerialized]
		public string[] countryNames;

		[NonSerialized]
		public int lastCountryCount;

		/// <summary>
		/// Accesor to the World Map Globe core API
		/// </summary>
		public WorldMap2D map { get { return GetComponent<WorldMap2D> (); } }

		public CountryDecoratorGroupInfo GetDecoratorGroup(int groupIndex, bool createIfNotExists) {
			if (decoratorLayer==null) return null;
			
			// Find the decorator container and returns the group info
			CountryDecoratorGroupInfo[] dgis = decoratorLayer.GetComponentsInChildren<CountryDecoratorGroupInfo>(true);
			for (int k=0;k<dgis.Length;k++) {
				if (dgis[k].groupIndex==groupIndex) return dgis[k];
			}
			
			// If it doesn't exist, create a container in the scene
			if (!createIfNotExists) return null;
			GameObject o = new GameObject("DecoratorGroup" + groupIndex);
			o.transform.SetParent(decoratorLayer.transform, false);
			CountryDecoratorGroupInfo dgi = o.AddComponent<CountryDecoratorGroupInfo>();
			dgi.groupIndex = groupIndex;
			dgi.active = true;
			return dgi;
		}

		public List<string>GetDecoratedCountries (int groupIndex, bool addCountryIndexSuffix) {
			List<string> decoratedCountries = new List<string> ();
			CountryDecoratorGroupInfo dg = GetDecoratorGroup (groupIndex, false);
			if (dg != null) {
				decoratedCountries.AddRange (dg.GetDecoratedCountries (addCountryIndexSuffix));
			}
			return decoratedCountries;
		}
		
		public CountryDecorator GetCountryDecorator(int groupIndex, string countryName) {
			CountryDecoratorGroupInfo di = GetDecoratorGroup(groupIndex, true);
			if (di!=null) 
				return di.GetDecorator(countryName);
			else
				return null;
		}
		
		
		public void SetCountryDecorator(int groupIndex, string countryName, CountryDecorator decorator) {
			// Get the group decorator container
			CountryDecoratorGroupInfo di = GetDecoratorGroup(groupIndex, true);
			if (decorator.countryName == null || !decorator.countryName.Equals(countryName))
				decorator.countryName = countryName;
			di.SetDecorator(decorator);
		}
		
		public void RemoveCountryDecorator(int groupIndex, string countryName) {
			// Get the group decorator container
			CountryDecoratorGroupInfo di = GetDecoratorGroup(groupIndex, false);
			if (di!=null) {
				di.RemoveDecorator(countryName);
			}
		}

		public int GetCountryDecoratorCount (int groupIndex) {
			CountryDecoratorGroupInfo dg = GetDecoratorGroup (groupIndex, false);
			if (dg != null) 
				return dg.decorators != null ? dg.decorators.Count : 0;
			else
				return 0;
		}

		public void ClearDecoratorGroup(int groupIndex) {
			CountryDecoratorGroupInfo di = GetDecoratorGroup(groupIndex, false);
			if (di!=null)
				di.RemoveAllDecorators();
		}

		public void ForceUpdateDecorators() {
			for (int k=0;k<NUM_GROUPS;k++) {
				CountryDecoratorGroupInfo dgi = GetDecoratorGroup(k, false);
				if (dgi!=null) dgi.UpdateDecorators(true);
			}
		}


		public void ReloadCountryNames () {
			if (map == null || map.countries == null)
				lastCountryCount = -1;
			else
				lastCountryCount = map.countries.Length;
			GUICountryIndex = -1;
			List<string> all = new List<string> ();
			all.AddRange (GetDecoratedCountries (GUIGroupIndex, true));
			// recover GUI country index selection
			if (GUICountryName.Length > 0) {
				for (int k=0; k<all.Count; k++) {
					if (all [k].StartsWith (GUICountryName)) {
						GUICountryIndex = k;
						break; 
					}
				}
			}
			if (all.Count > 0) 
				all.Add ("---");
			all.AddRange (map.GetCountryNames (groupByContinent));
			// recover GUI country index selection in case it's still undecorated
			if (GUICountryIndex == -1 && GUICountryName.Length > 0) {
				string countryNameToSearch = groupByContinent ? "  " + GUICountryName : GUICountryName;
				for (int k=0; k<all.Count; k++) {
					if (all [k].StartsWith (countryNameToSearch)) {
						GUICountryIndex = k;
						break; 
					}
				}
			}
			countryNames = all.ToArray ();
		}


	}
}
