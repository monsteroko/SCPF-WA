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
	public delegate void OnCityEnter (int cityIndex);

	public delegate void OnCityExit (int cityIndex);

	public delegate void OnCityClick (int cityIndex);

	public enum CITY_CLASS_FILTER
	{
		Any = 0,
		RegionCapitals = 1,
		CountryCapitals = 2,
		RegionAndCountryCapitals = 3
	}

	public partial class WorldMap2D : MonoBehaviour
	{

		#region Public properties

		/// <summary>
		/// Complete list of cities with their names and country names.
		/// </summary>
		[NonSerialized]
		public List<City>
			cities;
		public const int CITY_CLASS_FILTER_REGION_CAPITAL_CITY = 2;
		public const int CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY = 4;

		/// <summary>
		/// Returns City under mouse position or null if none.
		/// </summary>
		City _cityHighlighted;
		/// <summary>
		/// Returns City under mouse position or null if none.
		/// </summary>
		public City cityHighlighted { get { return _cityHighlighted; } }
		
		int _cityHighlightedIndex = -1;
		/// <summary>
		/// Returns City index mouse position or null if none.
		/// </summary>
		public int cityHighlightedIndex { get { return _cityHighlightedIndex; } }
		
		int _cityLastClicked = -1;
		/// <summary>
		/// Returns the last clicked city index.
		/// </summary>
		public int cityLastClicked { get { return _cityLastClicked; } }
	
		public event OnCityEnter OnCityEnter;
		public event OnCityEnter OnCityExit;
		public event OnCityClick OnCityClick;

		[SerializeField]
		bool
			_showCities = true;

		/// <summary>
		/// Toggle cities visibility.
		/// </summary>
		public bool showCities { 
			get {
				return _showCities; 
			}
			set {
				if (_showCities != value) {
					_showCities = value;
					isDirty = true;
					if (citiesLayer != null) {
						citiesLayer.SetActive (_showCities);
					} else if (_showCities) {
						DrawCities ();
					}
				}
			}
		}

		[SerializeField]
		CITY_CLASS_FILTER
			_cityClassFilter = CITY_CLASS_FILTER.Any;
		
		/// <summary>
		/// Applies a city class filter.
		/// </summary>
		public CITY_CLASS_FILTER cityClassFilter { 
			get {
				return _cityClassFilter; 
			}
			set {
				if (_cityClassFilter != value) {
					_cityClassFilter = value;
					isDirty = true;
					DrawCities ();
				}
			}
		}

		[SerializeField]
		Color
			_citiesColor = Color.white;
		
		/// <summary>
		/// Global color for cities.
		/// </summary>
		public Color citiesColor {
			get {
				if (citiesNormalMat != null) {
					return citiesNormalMat.color;
				} else {
					return _citiesColor;
				}
			}
			set {
				if (value != _citiesColor) {
					_citiesColor = value;
					isDirty = true;
					
					if (citiesNormalMat != null && _citiesColor != citiesNormalMat.color) {
						citiesNormalMat.color = _citiesColor;
					}
				}
			}
		}
		
		[SerializeField]
		Color
			_citiesRegionCapitalColor = Color.cyan;
		
		/// <summary>
		/// Global color for region capitals.
		/// </summary>
		public Color citiesRegionCapitalColor {
			get {
				if (citiesRegionCapitalMat != null) {
					return citiesRegionCapitalMat.color;
				} else {
					return _citiesRegionCapitalColor;
				}
			}
			set {
				if (value != _citiesRegionCapitalColor) {
					_citiesRegionCapitalColor = value;
					isDirty = true;
					
					if (citiesRegionCapitalMat != null && _citiesRegionCapitalColor != citiesRegionCapitalMat.color) {
						citiesRegionCapitalMat.color = _citiesRegionCapitalColor;
					}
				}
			}
		}
		
		[SerializeField]
		Color
			_citiesCountryCapitalColor = Color.yellow;
		
		/// <summary>
		/// Global color for country capitals.
		/// </summary>
		public Color citiesCountryCapitalColor {
			get {
				if (citiesCountryCapitalMat != null) {
					return citiesCountryCapitalMat.color;
				} else {
					return _citiesCountryCapitalColor;
				}
			}
			set {
				if (value != _citiesCountryCapitalColor) {
					_citiesCountryCapitalColor = value;
					isDirty = true;
					
					if (citiesCountryCapitalMat != null && _citiesCountryCapitalColor != citiesCountryCapitalMat.color) {
						citiesCountryCapitalMat.color = _citiesCountryCapitalColor;
					}
				}
			}
		}

		[SerializeField]
		float
			_cityIconSize = 1.0f;

		/// <summary>
		/// The size of the cities icon (dot).
		/// </summary>
		public float cityIconSize { 
			get {
				return _cityIconSize; 
			}
			set {
				if (value != _cityIconSize) {
					_cityIconSize = value;
					ScaleCities (); 
					ScaleMountPoints ();	// for the Editor's icon: mount points are invisible at runtime
					isDirty = true;
				}
			}
		}

		[SerializeField]
		float
			_cityHitTestRadius = 200f;
		
		/// <summary>
		/// A multiplier to the hit test. If you change scale of map you may want to adjust this value so hit testing match icon size.
		/// </summary>
		public float cityHitTestRadius { 
			get {
				return _cityHitTestRadius; 
			}
			set {
				if (value != _cityHitTestRadius) {
					_cityHitTestRadius = value;
					isDirty = true;
				}
			}
		}

		[Range(0, 17000)]
		[SerializeField]
		int
			_minPopulation = 0;

		public int minPopulation {
			get {
				return _minPopulation;
			}
			set {
				if (value != _minPopulation) {
					_minPopulation = value;
					isDirty = true;
					DrawCities ();
				}
			}
		}
		
		[SerializeField]
		int
			_cityClassAlwaysShow;

		/// <summary>
		/// Flags for specifying the class of cities to always show irrespective of other filters like minimum population. Can assign a combination of bit flags defined by CITY_CLASS_FILTER* 
		/// This flags only applies when cityClassFilter is set to ANY.
		/// </summary>
		public int cityClassAlwaysShow {
			get { return _cityClassAlwaysShow; }
			set {
				if (_cityClassAlwaysShow != value) {
					_cityClassAlwaysShow = value;
					isDirty = true;
					DrawCities ();
				}
			}
		}

		[NonSerialized]
		public int
			numCitiesDrawn = 0;

	#endregion

	#region Public API area

		/// <summary>
		/// Deletes all cities of current selected country's continent
		/// </summary>
		public void CitiesDeleteFromContinent (string continentName)
		{
			HideCityHighlights ();
			int k = -1;
			while (++k<cities.Count) {
				int cindex = cities [k].countryIndex;
				if (cindex >= 0) {
					string cityContinent = countries [cindex].continent;
					if (cityContinent.Equals (continentName)) {
						cities.RemoveAt (k);
						k--;
					}
				}
			}
		}

		/// <summary>
		/// Returns the index of a city in the global countries collection. Note that country index needs to be supplied due to repeated city names.
		/// </summary>
		public int GetCityIndex (int countryIndex, string cityName)
		{
			if (countryIndex >= 0 && countryIndex < countries.Length) {
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].countryIndex == countryIndex && cities [k].name.Equals (cityName))
						return k;
				}
			} else {
				// Try to select city by its name alone
				for (int k=0; k<cities.Count; k++) {
					if (cities [k].name.Equals (cityName))
						return k;
				}
			}
			return -1;
		}

		/// <summary>
		/// Flashes specified city by index in the global city collection.
		/// </summary>
		public void BlinkCity (int cityIndex, Color color1, Color color2, float duration, float blinkingSpeed)
		{
			if (citiesLayer == null)
				return;

			string cobj = GetCityHierarchyName (cityIndex);
			Transform t = transform.Find (cobj);
			if (t == null)
				return;
			CityBlinker sb = t.gameObject.AddComponent<CityBlinker> ();
			sb.blinkMaterial = t.GetComponent<Renderer> ().sharedMaterial;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
		}


		/// <summary>
		/// Starts navigation to target city. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name)
		{
			return FlyToCity (name, _navigationTime);
		}

		/// <summary>
		/// Starts navigation to target city with custom duration. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name, float duration)
		{
			int cityCount = cities.Count;
			for (int k=0; k<cityCount; k++) {
				if (name.Equals (cities [k].name)) {
					FlyToCity (k, duration);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target city with custom duration and zoom level. Returns false if not found.
		/// </summary>
		public bool FlyToCity (string name, float duration, float zoomLevel)
		{
			int cityCount = cities.Count;
			for (int k=0; k<cityCount; k++) {
				if (name.Equals (cities [k].name)) {
					FlyToCity (k, duration, zoomLevel);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target city by index in the cities collection.
		/// </summary>
		public void FlyToCity (int cityIndex)
		{
			FlyToCity (cityIndex, _navigationTime);
		}

		
		/// <summary>
		/// Starts navigation to target city by index with custom duration
		/// </summary>
		public void FlyToCity (int cityIndex, float duration)
		{
			SetDestination (cities [cityIndex].unity2DLocation, duration);
		}

		
		
		/// <summary>
		/// Starts navigation to target city by index with custom duration and zoom level
		/// </summary>
		public void FlyToCity (int cityIndex, float duration, float zoomLevel)
		{
			SetDestination (cities [cityIndex].unity2DLocation, duration, zoomLevel);
		}

		/// <summary>
		/// Returns the index of the city by its name in the cities collection.
		/// </summary>
		public int GetCityIndex (string cityName)
		{
			int cityCount = cities.Count;
			for (int k=0; k<cityCount; k++) {
				if (cityName.Equals (cities [k].name)) {
					return k;
				}
			}
			return -1;
		}


		/// <summary>
		/// Returns the city index by screen position.
		/// </summary>
		public bool GetCityIndex (Ray ray, out int cityIndex)
		{
			RaycastHit[] hits = Physics.RaycastAll (ray, 500, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector3 localHit = transform.InverseTransformPoint (hits [k].point);
						int c = GetCityNearPoint (localHit);
						if (c >= 0) {
							cityIndex = c;
							return true;
						}
					}
				}
			}
			cityIndex = -1;
			return false;
		}


		/// <summary>
		/// Clears any city highlighted (color changed) and resets them to default city color
		/// </summary>
		public void HideCityHighlights ()
		{
			DrawCities ();
		}


		/// <summary>
		/// Toggles the city highlight.
		/// </summary>
		/// <param name="cityIndex">City index.</param>
		/// <param name="color">Color.</param>
		/// <param name="highlighted">If set to <c>true</c> the color of the city will be changed. If set to <c>false</c> the color of the city will be reseted to default color</param>
		public void ToggleCityHighlight (int cityIndex, Color color, bool highlighted)
		{
			if (citiesLayer == null)
				return;
			string cobj = GetCityHierarchyName (cityIndex);
			Transform t = transform.Find (cobj);
			if (t == null)
				return;
			Renderer rr = t.gameObject.GetComponent<Renderer> ();
			if (rr == null)
				return;
			Material mat;
			if (highlighted) {
				mat = Instantiate (rr.sharedMaterial);
				mat.hideFlags = HideFlags.DontSave;
				mat.color = color;
				rr.sharedMaterial = mat;
			} else {
				switch (cities [cityIndex].cityClass) {
				case CITY_CLASS.COUNTRY_CAPITAL:
					mat = citiesCountryCapitalMat;
					break;
				case CITY_CLASS.REGION_CAPITAL:
					mat = citiesRegionCapitalMat;
					break;
				default:
					mat = citiesNormalMat;
					break;
				}
				rr.sharedMaterial = mat;
			}
		}

		/// <summary>
		/// Returns an array with the city names.
		/// </summary>
		public string[] GetCityNames ()
		{
			List<string> c = new List<string> (cities.Count);
			for (int k=0; k<cities.Count; k++) {
				c.Add (cities [k].name + " (" + k + ")");
			}
			c.Sort ();
			return c.ToArray ();
		}

		/// <summary>
		/// Returns an array with the city names.
		/// </summary>
		public string[] GetCityNames (int countryIndex)
		{
			List<string> c = new List<string> (cities.Count);
			for (int k=0; k<cities.Count; k++) {
				if (cities [k].countryIndex == countryIndex) {
					c.Add (cities [k].name + " (" + k + ")");
				}
			}
			c.Sort ();
			return c.ToArray ();
		}

		
		
		/// <summary>
		/// Returns a list of cities contained in a given region
		/// </summary>
		public List<City> GetCities (Region region) {
			int citiesCount = cities.Count;
			List<City> cc = new List<City> ();
			for (int k = 0; k < citiesCount; k++) {
				if (region.Contains (cities [k].unity2DLocation))
					cc.Add (cities [k]);
			}
			return cc;
		}

		#endregion

	}

}