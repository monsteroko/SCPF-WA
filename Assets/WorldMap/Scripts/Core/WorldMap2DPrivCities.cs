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
using System.Globalization;

namespace WPMF {

	public partial class WorldMap2D : MonoBehaviour {

		const float CITY_HIT_PRECISION = 0.00075f;

		// resources
		Material citiesNormalMat, citiesRegionCapitalMat, citiesCountryCapitalMat;

		// gameObjects
		GameObject citiesLayer, citySpot, citySpotCapitalRegion, citySpotCapitalCountry;

		#region IO stuff

		void ReadCitiesPackedString () {
			string cityCatalogFileName = "Geodata/zone_places";
			TextAsset ta = Resources.Load<TextAsset> (cityCatalogFileName);
			string s = ta.text;
			
			string[] cityList = s.Split (new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			int cityCount = cityList.Length;
			cities = new List<City> (cityCount);
			for (int k=0; k<cityCount; k++) {
				string[] cityInfo = cityList [k].Split (new char[] { '$' });
				string country = cityInfo [2];
				int countryIndex = GetCountryIndex(country);
				if (countryIndex>=0) {
					string name = cityInfo [0];
					string province = cityInfo [1];
					int population = int.Parse (cityInfo [3]);
					float x = float.Parse (cityInfo [4], CultureInfo.InvariantCulture);
					float y = float.Parse (cityInfo [5], CultureInfo.InvariantCulture);
					CITY_CLASS cityClass = (CITY_CLASS)int.Parse(cityInfo[6], CultureInfo.InvariantCulture);
					City city = new City (name, province, countryIndex, population, new Vector2 (x, y), cityClass);
					cities.Add (city);
				}
			}
		}


		#endregion


	#region Drawing stuff

		/// <summary>
		/// Redraws the cities. This is automatically called by Redraw(). Used internally by the Map Editor. You should not need to call this method directly.
		/// </summary>
		public void DrawCities () {

			if (!_showCities || !gameObject.activeInHierarchy) return;

			// Create cities layer
			Transform t = transform.Find ("Cities");
			if (t != null)
				DestroyImmediate (t.gameObject);
			citiesLayer = new GameObject ("Cities");
			citiesLayer.hideFlags = HideFlags.DontSave;
			citiesLayer.transform.SetParent (transform, false);
			citiesLayer.transform.localPosition = Misc.Vector3back * 0.001f;
			citiesLayer.layer = gameObject.layer;

			// Create cityclass parents
			GameObject countryCapitals = new GameObject("Country Capitals");
			countryCapitals.hideFlags = HideFlags.DontSave;
			countryCapitals.transform.SetParent(citiesLayer.transform, false);
			GameObject regionCapitals = new GameObject("Region Capitals");
			regionCapitals.hideFlags = HideFlags.DontSave;
			regionCapitals.transform.SetParent(citiesLayer.transform, false);
			GameObject normalCities = new GameObject("Normal Cities");
			normalCities.hideFlags = HideFlags.DontSave;
			normalCities.transform.SetParent(citiesLayer.transform, false);

			// Draw city marks
			numCitiesDrawn = 0;
			int minPopulation = _minPopulation;

			if (cities==null) return;
			for (int k=0; k<cities.Count; k++) {
				City city = cities [k];
				city.isVisible = false;
				switch(_cityClassFilter) {
				default:
					city.isVisible = (((int)city.cityClass & _cityClassAlwaysShow) != 0) && (minPopulation==0 || city.population >= minPopulation);
					break;
				case CITY_CLASS_FILTER.RegionCapitals:
					city.isVisible = (city.cityClass == CITY_CLASS.REGION_CAPITAL ) && (minPopulation==0 || city.population >= minPopulation);
					break;
				case CITY_CLASS_FILTER.CountryCapitals:
					city.isVisible = (city.cityClass == CITY_CLASS.COUNTRY_CAPITAL ) && (minPopulation==0 || city.population >= minPopulation);
					break;
				case CITY_CLASS_FILTER.RegionAndCountryCapitals:
					city.isVisible = (city.cityClass != CITY_CLASS.CITY ) && (minPopulation==0 || city.population >= minPopulation);
					break;
				}
				if (city.isVisible) {
					GameObject cityObj, cityParent;
					switch(city.cityClass) {
					case CITY_CLASS.COUNTRY_CAPITAL: 
						cityObj = Instantiate (citySpotCapitalCountry); 
						cityObj.GetComponent<Renderer> ().sharedMaterial = citiesCountryCapitalMat;
						cityParent = countryCapitals;
						break;
					case CITY_CLASS.REGION_CAPITAL: 
						cityObj = Instantiate (citySpotCapitalRegion); 
						cityObj.GetComponent<Renderer> ().sharedMaterial = citiesRegionCapitalMat;
						cityParent = regionCapitals;
						break;
					default:
						cityObj = Instantiate (citySpot); 
						cityObj.GetComponent<Renderer> ().sharedMaterial = citiesNormalMat;
						cityParent = normalCities;
						break;
					}
					cityObj.name = k.ToString();
					cityObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
					cityObj.layer = citiesLayer.layer;
					cityObj.transform.SetParent (cityParent.transform, false);
					cityObj.transform.localPosition = city.unity2DLocation;
					numCitiesDrawn++;
				}
			}
			// Toggle cities layer visibility according to settings
			citiesLayer.SetActive (_showCities);
			ScaleCities();
		}

		public string GetCityHierarchyName(int cityIndex) {
			if (cityIndex<0 || cityIndex>=cities.Count) return "";
			switch(cities[cityIndex].cityClass) {
			case CITY_CLASS.COUNTRY_CAPITAL: return "Cities/Country Capitals/" + cityIndex.ToString();
			case CITY_CLASS.REGION_CAPITAL: return "Cities/Region Capitals/" + cityIndex.ToString();
			default: return "Cities/Normal Cities/" + cityIndex.ToString();
			}
		}

		void ScaleCities() {
			if (!gameObject.activeInHierarchy || citiesLayer==null) return;
			CityScaler cityScaler = citiesLayer.GetComponent<CityScaler>() ?? citiesLayer.AddComponent<CityScaler>();
			cityScaler.map = this;
			if (_showCities) {
				cityScaler.ScaleCities();
			}
		}

		void HighlightCity(int cityIndex) {
			_cityHighlightedIndex = cityIndex;
			_cityHighlighted = cities[cityIndex];
			
			// Raise event
			if (OnCityEnter!=null) OnCityEnter(_cityHighlightedIndex);
		}
		
		void HideCityHighlight() {
			if (_cityHighlightedIndex<0) return;
			
			// Raise event
			if (OnCityExit!=null) OnCityExit(_cityHighlightedIndex);
			_cityHighlighted = null;
			_cityHighlightedIndex = -1;
		}


	#endregion

		#region Internal API

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		int GetCityNearPoint(Vector3 localPoint) {
			return GetCityNearPoint(localPoint, -1);
		}

		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		int GetCityNearPoint(Vector3 localPoint, int countryIndex) {
			float th = CITY_HIT_PRECISION * _cityHitTestRadius / mapWidth;
			float rl = localPoint.x - th;
			float rr = localPoint.x + th;
			float rt = localPoint.y + th;
			float rb = localPoint.y - th;
			for (int c=0;c<cities.Count;c++) {
				City city = cities[c];
				if (city.isVisible && (city.countryIndex == countryIndex || countryIndex == -1)) {
					Vector2 cityLoc = city.unity2DLocation;
					if (cityLoc.x>rl && cityLoc.x<rr && cityLoc.y>rb && cityLoc.y<rt) {
						return c;
					}
				}
			}
			return -1;
		}

		/// <summary>
		/// Returns the file name corresponding to the current city data file
		/// </summary>
		public string GetCityFileName() {
			return "cities.txt";
		}


		#endregion
		
		
	}
	
}