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

	public delegate void OnCountryEvent(int countryIndex, int regionIndex);
	public delegate void OnCountryHighlightEvent(int countryIndex, int regionIndex, ref bool allowHighlight);

	public partial class WorldMap2D : MonoBehaviour
	{

		#region Public properties

		/// <summary>
		/// Complete array of countries and the continent name they belong to.
		/// </summary>
		[NonSerialized]
		public Country[]
			countries;

		Country _countryHighlighted;
		/// <summary>
		/// Returns Country under mouse position or null if none.
		/// </summary>
		public Country countryHighlighted { get { return _countryHighlighted; } }

		int	_countryHighlightedIndex = -1;
		/// <summary>
		/// Returns currently highlighted country index in the countries list.
		/// </summary>
		public int countryHighlightedIndex { get { return _countryHighlightedIndex; } }

		Region _countryRegionHighlighted;
		/// <summary>
		/// Returns currently highlightd country's region.
		/// </summary>
		/// <value>The country region highlighted.</value>
		public Region countryRegionHighlighted { get { return _countryRegionHighlighted; } }

		int _countryRegionHighlightedIndex = -1;
		/// <summary>
		/// Returns currently highlighted region of the country.
		/// </summary>
		public int countryRegionHighlightedIndex { get { return _countryRegionHighlightedIndex; } }

		int _countryLastClicked = -1;
		/// <summary>
		/// Returns the last clicked country.
		/// </summary>
		public int countryLastClicked { get { return _countryLastClicked; } }

		int _countryRegionLastClicked = -1;
		/// <summary>
		/// Returns the last clicked country region index.
		/// </summary>
		public int countryRegionLastClicked { get { return _countryRegionLastClicked; } }

		public event OnCountryEvent OnCountryEnter;
		public event OnCountryEvent OnCountryExit;
		public event OnCountryEvent OnCountryClick;
		public event OnCountryHighlightEvent OnCountryHighlight;

		[SerializeField]
		bool
			_enableCountryHighlight = true;

		/// <summary>
		/// Enable/disable country highlight when mouse is over.
		/// </summary>
		public bool enableCountryHighlight {
			get {
				return _enableCountryHighlight;
			}
			set {
				if (_enableCountryHighlight != value) {
					_enableCountryHighlight = value;
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Set whether all regions of active country should be highlighted.
		/// </summary>
		[SerializeField]
		bool
			_highlightAllCountryRegions = false;
		
		public bool highlightAllCountryRegions {
			get {
				return _highlightAllCountryRegions;
			}
			set {
				if (_highlightAllCountryRegions != value) {
					_highlightAllCountryRegions = value;
					DestroySurfaces();
					isDirty = true;
				}
			}
		}



		[SerializeField]
		bool
			_showFrontiers = true;

		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showFrontiers { 
			get {
				return _showFrontiers; 
			}
			set {
				if (value != _showFrontiers) {
					_showFrontiers = value;
					isDirty = true;

					if (frontiersLayer != null) {
						frontiersLayer.SetActive (_showFrontiers);
					} else if (_showFrontiers) {
						DrawFrontiers ();
					}
				}
			}
		}

		[SerializeField]
		bool _frontiersThinLines = false;
		/// <summary>
		/// Set this property to true to disable thicker frontier lines when zooming in
		/// </summary>
		public bool frontiersThinLines { 
			get {
				return _frontiersThinLines; 
			}
			set {
				if (value != _frontiersThinLines) {
					_frontiersThinLines = value;
					transform.localPosition += Misc.Vector3right * 0.000001f; // triggers update
					isDirty = true;
				}
			}
		}



		/// <summary>
		/// Fill color to use when the mouse hovers a country's region.
		/// </summary>
		[SerializeField]
		Color
			_fillColor = new Color (1, 0, 0, 0.7f);

		public Color fillColor {
			get {
				if (hudMatCountry != null) {
					return hudMatCountry.color;
				} else {
					return _fillColor;
				}
			}
			set {
//				Color proposedNewColor = new Color (value.r, value.g, value.b, 0.7f);
				if (value != _fillColor) {
					_fillColor = value;
					isDirty = true;
					if (hudMatCountry != null && _fillColor != hudMatCountry.color) {
						hudMatCountry.color = _fillColor;
					}
				}
			}
		}

		/// <summary>
		/// Inner Color for country frontiers.
		/// </summary>
		[SerializeField]
		Color
			_frontiersColor = Color.green;

		public Color frontiersColor {
			get {
				if (frontiersMat != null) {
					return frontiersMat.color;
				} else {
					return _frontiersColor;
				}
			}
			set {
				if (value != _frontiersColor) {
					_frontiersColor = value;
					isDirty = true;

					if (frontiersMat != null && _frontiersColor != frontiersMat.color) {
						frontiersMat.color = _frontiersColor;
					}
				}
			}
		}


		/// <summary>
		/// Outer color for country frontiers.
		/// </summary>
		[SerializeField]
		Color
			_frontiersColorOuter = new Color(0,1,0,0.5f);
		
		public Color frontiersColorOuter {
			get {
				return _frontiersColorOuter;
			}
			set {
				if (value != _frontiersColorOuter) {
					_frontiersColorOuter = value;
					isDirty = true;
					
					if (frontiersMat != null) {
						frontiersMat.SetColor("_OuterColor", _frontiersColorOuter);
					}
				}
			}
		}


		[SerializeField]
		bool
			_showOutline = true;
		
		/// <summary>
		/// Toggle frontiers thicker outline visibility.
		/// </summary>
		public bool showOutline { 
			get {
				return _showOutline; 
			}
			set {
				if (value != _showOutline) {
					_showOutline = value;
					Redraw (); // recreate surfaces layer
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Color for country frontiers outline.
		/// </summary>
		[SerializeField]
		Color
			_outlineColor = Color.black;
		
		public Color outlineColor {
			get {
				if (outlineMat != null) {
					return outlineMat.color;
				} else {
					return _outlineColor;
				}
			}
			set {
				if (value != _outlineColor) {
					_outlineColor = value;
					isDirty = true;
					
					if (outlineMat != null && _outlineColor != outlineMat.color) {
						outlineMat.color = _outlineColor;
					}
				}
			}
		}

		[SerializeField]
		bool
			_showCountryNames = false;

		public bool showCountryNames {
			get {
				return _showCountryNames;
			}
			set {
				if (value != _showCountryNames) {
#if TRACE_CTL
					Debug.Log ("CTL " + DateTime.Now + ": showcountrynames!");
#endif
					_showCountryNames = value;
					isDirty = true;
					if (textRoot != null) {
						textRoot.SetActive (_showCountryNames);
					} else if (_showCountryNames) {
						DrawMapLabels ();
					}
//					if (gameObject.activeInHierarchy) {
//						if (!showCountryNames) {
//							DestroyMapLabels ();
//						} else {
//							DrawMapLabels ();
//						}
//					}
				}
			}
		}

		[SerializeField]
		float
			_countryLabelsAbsoluteMinimumSize = 0.5f;

		public float countryLabelsAbsoluteMinimumSize {
			get {
				return _countryLabelsAbsoluteMinimumSize;
			} 
			set {
				if (value != _countryLabelsAbsoluteMinimumSize) {
					_countryLabelsAbsoluteMinimumSize = value;
					isDirty = true;
					if (_showCountryNames)
						DrawMapLabels ();
				}
			}
		}

		[SerializeField]
		float
			_countryLabelsSize = 0.25f;
		
		public float countryLabelsSize {
			get {
				return _countryLabelsSize;
			} 
			set {
				if (value != _countryLabelsSize) {
					_countryLabelsSize = value;
					isDirty = true;
					if (_showCountryNames)
						DrawMapLabels ();
				}
			}
		}

		[SerializeField]
		bool
			_showLabelsShadow = true;

		/// <summary>
		/// Draws a shadow under map labels. Specify the color using labelsShadowColor.
		/// </summary>
		/// <value><c>true</c> if show labels shadow; otherwise, <c>false</c>.</value>
		public bool showLabelsShadow {
			get {
				return _showLabelsShadow;
			}
			set {
				if (value != _showLabelsShadow) {
					_showLabelsShadow = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						RedrawMapLabels ();
					}
				}
			}
		}

		[SerializeField]
		Color
			_countryLabelsColor = Color.white;
		
		/// <summary>
		/// Color for map labels.
		/// </summary>
		public Color countryLabelsColor {
			get {
				return _countryLabelsColor;
			}
			set {
				if (value != _countryLabelsColor) {
					_countryLabelsColor = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						labelsFont.material.color = _countryLabelsColor;
					}
				}
			}
		}

		[SerializeField]
		Color
			_countryLabelsShadowColor = Color.black;
		
		/// <summary>
		/// Color for map labels.
		/// </summary>
		public Color countryLabelsShadowColor {
			get {
				return _countryLabelsShadowColor;
			}
			set {
				if (value != _countryLabelsShadowColor) {
					_countryLabelsShadowColor = value;
					isDirty = true;
					if (gameObject.activeInHierarchy) {
						labelsShadowMaterial.color = _countryLabelsShadowColor;
					}
				}
			}
		}

		[SerializeField]
		Font _countryLabelsFont;
		/// <summary>
		/// Gets or sets the default font for country labels
		/// </summary>
		public Font countryLabelsFont {
			get {
				return _countryLabelsFont;
			}
			set {
				if (value != _countryLabelsFont) {
					_countryLabelsFont = value;
					isDirty = true;
					ReloadFont();
//					if (labelsFont == null) {
//						labelsFont = GameObject.Instantiate (Resources.Load <Font> ("Font/Lato"));
//						labelsFont.name = "Lato";
//						labelsFont.hideFlags = HideFlags.DontSave;
//					}
//					Material fontMaterial = Instantiate (labelsFont.material);
//					fontMaterial.hideFlags = HideFlags.DontSave;
//					labelsFont.material = fontMaterial;
//					labelsShadowMaterial = GameObject.Instantiate (fontMaterial);
//					labelsShadowMaterial.hideFlags = HideFlags.DontSave;
//					labelsShadowMaterial.renderQueue--;
					RedrawMapLabels ();
				}
			}
		}


	#endregion

	#region Public API area

		/// <summary>
		/// Returns the index of a country in the countries array by its name.
		/// </summary>
		public int GetCountryIndex (string countryName)
		{
			if (countryLookup.ContainsKey (countryName)) 
				return countryLookup [countryName];
			else
				return -1;
		}

		/// <summary>
		/// Returns the index of a country in the countries collection by its reference.
		/// </summary>
		public int GetCountryIndex (Country country)
		{
			if (countryLookup.ContainsKey (country.name)) 
				return countryLookup [country.name];
			else
				return -1;
		}

		/// <summary>
		/// Gets the index of the country that contains the provided map coordinates. This will ignore hidden countries.
		/// </summary>
		/// <returns>The country index.</returns>
		/// <param name="localPosition">Map coordinates in the range of (-0.5 .. 0.5)</param>
		public int GetCountryIndex(Vector2 localPosition) {
			// verify if hitPos is inside any country polygon
			int countryCount = _countriesOrderedBySize.Count;
			for (int oc=0; oc<countryCount; oc++) {
				int c = _countriesOrderedBySize[oc];
				Country country = countries[c];
				if (country.hidden) continue;
				if (!country.regionsRect2D.Contains(localPosition)) continue;
				for (int cr=0; cr<country.regions.Count; cr++) {
					if (country.regions [cr].Contains (localPosition)) {
						return c;
					}
				}
			}
			return -1;
		}


		
		/// <summary>
		/// Gets the country that contains a given map coordinate or the country whose center is nearest to that coordinate.
		/// </summary>
		public int GetCountryNearPoint(Vector2 localPosition) {
			int countryIndex = GetCountryIndex(localPosition);
			if (countryIndex>=0) return countryIndex;
			float minDist = float.MaxValue;
			int countryCount = _countriesOrderedBySize.Count;
			for (int k=0;k<countryCount;k++) {
				int oc = _countriesOrderedBySize[k];
				Country country = countries[oc];
				float dist = Vector2.SqrMagnitude(country.center - localPosition);
				if (dist<minDist) {
					minDist = dist;
					countryIndex = k;
				}
			}
			return countryIndex;
		}


		/// <summary>
		/// Returns the zoom level required to show the entire country region on screen
		/// </summary>
		/// <returns>The country zoom level of -1 if error.</returns>
		/// <param name="countryIndex">Country index.</param>
		public float GetCountryRegionZoomExtents(int countryIndex) {
			if (countryIndex<0 || countries==null || countryIndex>=countries.Length) return -1;
			return GetCountryRegionZoomExtents(countryIndex, countries[countryIndex].mainRegionIndex);
		}

		/// <summary>
		/// Returns the zoom level required to show the entire country region on screen
		/// </summary>
		/// <returns>The country zoom level of -1 if error.</returns>
		/// <param name="countryIndex">Country index.</param>
		/// <param name="regionIndex">Region index of the country.</param>
		public float GetCountryRegionZoomExtents(int countryIndex, int regionIndex) {
			if (countryIndex<0 || countries==null || countryIndex>=countries.Length) return -1;
			Country country = countries[countryIndex];
			if (regionIndex<0 || regionIndex>=country.regions.Count) return -1;
			Region region = country.regions[regionIndex];
			return GetFrustumZoomLevel(region.rect2D.width * mapWidth, region.rect2D.height * mapHeight);
		}

		/// <summary>
		/// Returns all neighbour countries
		/// </summary>
		public List<Country> CountryNeighbours (int countryIndex)
		{
			
			List<Country> countryNeighbours = new List<Country> ();
			
			// Get country object
			Country country = countries [countryIndex];
			
			// Iterate for all regions (a country can have several separated regions)
			for (int countryRegionIndex=0; countryRegionIndex<country.regions.Count; countryRegionIndex++) {
				Region countryRegion = country.regions [countryRegionIndex];
				
				// Get the neighbours for this region
				for (int neighbourIndex=0; neighbourIndex<countryRegion.neighbours.Count; neighbourIndex++) {
					Region neighbour = countryRegion.neighbours [neighbourIndex];
					Country neighbourCountry = (Country)neighbour.entity;	
					if (!countryNeighbours.Contains (neighbourCountry)) {
						countryNeighbours.Add (neighbourCountry);
					}
				}
			}
			
			return countryNeighbours;
		}
		
		
		/// <summary>
		/// Get neighbours of the main region of a country
		/// </summary>
		public List<Country> CountryNeighboursOfMainRegion (int countryIndex)
		{
			
			List<Country> countryNeighbours = new List<Country> ();
			
			// Get main region
			Country country = countries [countryIndex];
			Region countryRegion = country.regions [country.mainRegionIndex];
			
			// Get the neighbours for this region
			for (int neighbourIndex=0; neighbourIndex<countryRegion.neighbours.Count; neighbourIndex++) {
				Region neighbour = countryRegion.neighbours [neighbourIndex];
				Country neighbourCountry = (Country)neighbour.entity;	
				if (!countryNeighbours.Contains (neighbourCountry)) {
					countryNeighbours.Add (neighbourCountry);
				}
			}
			return countryNeighbours;
		}
		
		
		/// <summary>
		/// Get neighbours of the currently selected region
		/// </summary>
		public List<Country> CountryNeighboursOfCurrentRegion ()
		{
			
			List<Country> countryNeighbours = new List<Country> ();
			
			// Get main region
			Region selectedRegion = countryRegionHighlighted;
			if (selectedRegion == null)
				return countryNeighbours;

			// Get the neighbours for this region
			for (int neighbourIndex=0; neighbourIndex<selectedRegion.neighbours.Count; neighbourIndex++) {
				Region neighbour = selectedRegion.neighbours [neighbourIndex];
				Country neighbourCountry = (Country)neighbour.entity;	
				if (!countryNeighbours.Contains (neighbourCountry)) {
					countryNeighbours.Add (neighbourCountry);
				}
			}
			return countryNeighbours;
		}

		
		/// <summary>
		/// Renames the country. Name must be unique, different from current and one letter minimum.
		/// </summary>
		/// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
		public bool CountryRename (string oldName, string newName)
		{
			if (newName == null || newName.Length == 0)
				return false;
			int countryIndex = GetCountryIndex (oldName);
			int newCountryIndex = GetCountryIndex (newName);
			if (countryIndex < 0 || newCountryIndex >= 0)
				return false;
			countries [countryIndex].name = newName;
			lastCountryLookupCount = -1;
			return true;
				
		}


		/// <summary>
		/// Deletes the country. Optionally also delete its dependencies (provinces, cities, mountpoints).
		/// </summary>
		/// <returns><c>true</c> if country was deleted, <c>false</c> otherwise.</returns>
		public bool CountryDelete (int countryIndex, bool deleteDependencies)
		{
			if (countryIndex < 0 || countryIndex >= countries.Length)
				return false;
			
			// Update dependencies
			if (deleteDependencies) {
				HideProvinceRegionHighlights(true);
				List<Province> newProvinces = new List<Province> (provinces.Length);
				int k;
				for (k=0; k<provinces.Length; k++) {
					if (provinces [k].countryIndex != countryIndex)
						newProvinces.Add (provinces [k]);
				}
				provinces = newProvinces.ToArray ();
				lastProvinceLookupCount = -1;
				
				HideCityHighlights();
				k=-1;
				while(++k<cities.Count) {
					if (cities[k].countryIndex == countryIndex) {
						cities.RemoveAt(k);
						k--;
					}
				}
				
				HideMountPointHighlights();
				k=-1;
				while(++k<mountPoints.Count) {
					if (mountPoints[k].countryIndex == countryIndex) {
						mountPoints.RemoveAt(k);
						k--;
					}
				}
			}
			
			// Updates provinces reference to country
			for (int k=0; k<provinces.Length; k++) {
				if (provinces [k].countryIndex > countryIndex)
					provinces [k].countryIndex--;
			}
			
			// Updates country index in cities
			for (int k=0;k<cities.Count;k++) {
				if (cities[k].countryIndex >countryIndex) {
					cities[k].countryIndex--;
				}
			}
			// Updates country index in mount points
			if (mountPoints!=null) {
				for (int k=0;k<mountPoints.Count;k++) {
					if (mountPoints[k].countryIndex >countryIndex) {
						mountPoints[k].countryIndex--;
					}
				}
			}
			
			// Excludes country from new array
			List<Country> newCountries = new List<Country> (countries.Length);
			for (int k=0; k<countries.Length; k++) {
				if (k != countryIndex)
					newCountries.Add (countries [k]);
			}
			countries = newCountries.ToArray ();
			
			// Update lookup dictionaries
			lastCountryLookupCount = -1;
			
			return true;
		}


		
		/// <summary>
		/// Deletes all provinces from a country.
		/// </summary>
		/// <returns><c>true</c>, if provinces where deleted, <c>false</c> otherwise.</returns>
		public bool CountryDeleteProvinces(int countryIndex) {
			int numProvinces = provinces.Length;
			List<Province> newProvinces = new List<Province>(numProvinces);
			for (int k=0;k<numProvinces;k++) {
				if (provinces[k]!=null && provinces[k].countryIndex!= countryIndex) {
					newProvinces.Add (provinces[k]);
				}
			}
			provinces = newProvinces.ToArray();
			lastProvinceLookupCount = -1;
			return true;
		}


		public void CountriesDeleteFromContinent(string continentName) {
			
			HideCountryRegionHighlights(true);
			
			ProvincesDeleteOfSameContinent(continentName);
			CitiesDeleteFromContinent(continentName);
			MountPointsDeleteFromSameContinent(continentName);
			
			List<Country> newAdmins = new List<Country>(countries.Length-1);
			for (int k=0;k<countries.Length;k++) {
				if (!countries[k].continent.Equals (continentName)) {
					newAdmins.Add (countries[k]);
				} else {
					int lastIndex = newAdmins.Count-1;
					// Updates country index in provinces
					if (provinces!=null) {
						for (int p=0;p<_provinces.Length;p++) {
							if (_provinces[p].countryIndex>lastIndex) {
								_provinces[p].countryIndex--;
							}
						}
					}
					// Updates country index in cities
					if (cities!=null) {
						for (int c=0;c<cities.Count;c++) {
							if (cities[c].countryIndex>lastIndex) {
								cities[c].countryIndex--;
							}
						}
					}
					// Updates country index in mount points
					if (mountPoints!=null) {
						for (int c=0;c<mountPoints.Count;c++) {
							if (mountPoints[c].countryIndex>lastIndex) {
								mountPoints[c].countryIndex--;
							}
						}
					}
				}
			}
			
			countries = newAdmins.ToArray();
			lastCountryLookupCount = -1;
			
		}



		/// <summary>
		/// Adds a new country which has been properly initialized. Used by the Map Editor. Name must be unique.
		/// </summary>
		/// <returns>The index of the new country</returns>
		public int CountryAdd (Country country)
		{
			int countryIndex = GetCountryIndex (country.name);
			if (countryIndex >= 0)
				return -1;
			Country[] newCountries = new Country[countries.Length + 1];
			for (int k=0; k<countries.Length; k++) {
				newCountries [k] = countries [k];
			}
			countryIndex = newCountries.Length - 1;
			newCountries [countryIndex] = country;
			countries = newCountries;
			lastCountryLookupCount = -1;
			return countryIndex;
		}


		/// <summary>
		/// Returns the country index by screen position.
		/// </summary>
		public bool GetCountryIndex (Ray ray, out int countryIndex, out int regionIndex)
		{
			RaycastHit[] hits = Physics.RaycastAll (ray, 500, layerMask);
			if (hits.Length > 0) {
				for (int k=0; k<hits.Length; k++) {
					if (hits [k].collider.gameObject == gameObject) {
						Vector2 localHit = transform.InverseTransformPoint (hits [k].point);
						for (int c=0; c<countries.Length; c++) {
							for (int cr=0; cr<countries[c].regions.Count; cr++) {
								Region region = countries [c].regions [cr];
								if (region.Contains (localHit)) {
									countryIndex = c;
									regionIndex = cr;
									return true;
								}	
							}
						}
					}
				}
			}
			countryIndex = -1;
			regionIndex = -1;
			return false;
		}


		/// <summary>
		/// Starts navigation to target country. Returns false if country is not found.
		/// </summary>
		public bool FlyToCountry (Country country)
		{
			int countryIndex = GetCountryIndex (country);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex);
				return true;
			}
			return false;
		}


		/// <summary>
		/// Starts navigation to target country. Returns false if country is not found.
		/// </summary>
		public bool FlyToCountry (string name)
		{
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Starts navigation to target country. with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// Returns false if country is not found. 
		/// </summary>
		public bool FlyToCountry (string name, float duration)
		{
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex, duration);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Starts navigation to target country. with specified duration and zoom level, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// Returns false if country is not found. 
		/// </summary>
		public bool FlyToCountry (string name, float duration, float zoomLevel) {
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				FlyToCountry (countryIndex, duration, zoomLevel);
				return true;
			}
			return false;
		}

		
		/// <summary>
		/// Starts navigation to target country by index in the countries collection. Returns false if country is not found.
		/// </summary>
		public void FlyToCountry (int countryIndex)
		{
			FlyToCountry (countryIndex, _navigationTime);
		}
		
		/// <summary>
		/// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// </summary>
		public void FlyToCountry (int countryIndex, float duration)
		{
			SetDestination (countries [countryIndex].center, duration);
		}
	
		
		/// <summary>
		/// Starts navigating to target country by index in the countries collection with specified duration, ignoring NavigationTime property.
		/// Set duration to zero to go instantly.
		/// </summary>
		public void FlyToCountry (int countryIndex, float duration, float zoomLevel) {
			SetDestination (countries [countryIndex].center, duration, zoomLevel);
		}


		/// <summary>
		/// Colorize all regions of specified country by name. Returns false if not found.
		/// </summary>
		public bool ToggleCountrySurface (string name, bool visible, Color color)
		{
			int countryIndex = GetCountryIndex (name);
			if (countryIndex >= 0) {
				ToggleCountrySurface (countryIndex, visible, color);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Iterates for the countries list and colorizes those belonging to specified continent name.
		/// </summary>
		public void ToggleContinentSurface (string continentName, bool visible, Color color)
		{
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					ToggleCountrySurface (countries [colorizeIndex].name, visible, color);
				}
			}
		}

		/// <summary>
		/// Uncolorize/hide specified countries beloning to a continent.
		/// </summary>
		public void HideContinentSurface (string continentName)
		{
			for (int colorizeIndex =0; colorizeIndex < countries.Length; colorizeIndex++) {
				if (countries [colorizeIndex].continent.Equals (continentName)) {
					HideCountrySurface (colorizeIndex);
				}
			}
		}

		/// <summary>
		/// Colorize all regions of specified country by index in the countries collection.
		/// </summary>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color)
		{
			ToggleCountrySurface (countryIndex, visible, color, null);
		}

		/// <summary>
		/// Colorize all regions of specified country and assings a texture.
		/// </summary>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color, Texture2D texture, bool applyTextureToAllRegions = true)
		{
			ToggleCountrySurface (countryIndex, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, applyTextureToAllRegions);
		}


		/// <summary>
		/// Colorize all regions of specified country and assings a texture.
		/// </summary>
		/// <param name="countryIndex">Country index.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		/// <param name="color">Color.</param>
		/// <param name="texture">Texture.</param>
		/// <param name="textureScale">Texture scale.</param>
		/// <param name="textureOffset">Texture offset.</param>
		/// <param name="textureRotation">Texture rotation.</param>
		/// <param name="applyTextureToAllRegions">If set to <c>true</c> the texture will be applied to all regions, otherwise only the main region will get the texture and the remaining regions will get the color.</param>
		public void ToggleCountrySurface (int countryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool applyTextureToAllRegions)
		{
			if (!visible) {
				HideCountrySurface (countryIndex);
				return;
			}
			Country country = countries[countryIndex];
			ToggleCountryRegionSurface (countryIndex, country.mainRegionIndex, visible, color, texture, textureScale, textureOffset, textureRotation, applyTextureToAllRegions);
		}

		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountrySurface (int countryIndex)
		{
			for (int r=0; r<countries[countryIndex].regions.Count; r++) {
				HideCountryRegionSurface (countryIndex, r);
			}
		}

		/// <summary>
		/// Highlights the country region specified.
		/// Internally used by the Editor component, but you can use it as well to temporarily mark a country region.
		/// </summary>
		/// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
		public GameObject ToggleCountryRegionSurfaceHighlight (int countryIndex, int regionIndex, Color color, bool drawOutline)
		{
			GameObject surf;
			Material mat = Instantiate (hudMatCountry);
			mat.hideFlags = HideFlags.DontSave;
			mat.color = color;
			mat.renderQueue--;
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex); 
			bool existsInCache = surfaces.ContainsKey (cacheIndex);
			if (existsInCache) {
				surf = surfaces [cacheIndex];
				if (surf == null) {
					surfaces.Remove (cacheIndex);
				} else {
					surf.SetActive (true);
					surf.GetComponent<Renderer> ().sharedMaterial = mat;
				}
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, mat, Misc.Vector2one, Misc.Vector2zero, 0, drawOutline);
			}
			return surf;
		}

		/// <summary>
		/// Colorize main region of a country by index in the countries collection.
		/// </summary>
		public void ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color)
		{
			ToggleCountryMainRegionSurface(countryIndex, visible, color, null, Misc.Vector2zero, Misc.Vector2zero, 0);
		}

		/// <summary>
		/// Add texture to main region of a country by index in the countries collection.
		/// </summary>
		/// <param name="texture">Optional texture or null to colorize with single color</param>
		public void ToggleCountryMainRegionSurface (int countryIndex, bool visible, Texture2D texture)
		{
			ToggleCountryRegionSurface (countryIndex, countries [countryIndex].mainRegionIndex, visible, Color.white, texture, Misc.Vector2one, Misc.Vector2zero, 0);
		}

		/// <summary>
		/// Add color and texture to main region of a country by index in the countries collection.
		/// </summary>
		/// <param name="texture">Optional texture or null to colorize with single color</param>
		public void ToggleCountryMainRegionSurface (int countryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation)
		{
			ToggleCountryRegionSurface (countryIndex, countries [countryIndex].mainRegionIndex, visible, color, texture, textureScale, textureOffset, textureRotation);
		}

		public void ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color)
		{
			ToggleCountryRegionSurface (countryIndex, regionIndex, visible, color, null, Misc.Vector2one, Misc.Vector2zero, 0);
		}

		/// <summary>
		/// Colorize specified region of a country by indexes.
		/// </summary>
		public void ToggleCountryRegionSurface (int countryIndex, int regionIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool applyTextureToAllRegions = true)
		{
			if (!visible) {
				HideCountryRegionSurface (countryIndex, regionIndex);
				return;
			}
			GameObject surf = null;
			Region region = countries [countryIndex].regions [regionIndex];
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			// Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
			if (surfaces.ContainsKey (cacheIndex) && surfaces [cacheIndex] != null) 
				surf = surfaces [cacheIndex];

			// Should the surface be recreated?
			Material surfMaterial;
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset || 
					textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals (texturizedMat.name))) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = countryHighlightedIndex == countryIndex && (countryRegionHighlightedIndex == regionIndex || _highlightAllCountryRegions) && _enableCountryHighlight;
			if (surf != null) {
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if ((texture == null && !surfMaterial.name.Equals (coloredMat.name)) || (texture != null && !surfMaterial.name.Equals (texturizedMat.name)) 
					|| (surfMaterial.color != color && !isHighlighted) || (texture != null && region.customMaterial.mainTexture != texture)) {
                    Material goodMaterial = GetColoredTexturedMaterial (color, texture);
                    region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (surf, goodMaterial);
				}
			} else {
                surfMaterial = GetColoredTexturedMaterial (color, texture);
                surf = GenerateCountryRegionSurface (countryIndex, regionIndex, surfMaterial, textureScale, textureOffset, textureRotation, _showOutline, applyTextureToAllRegions);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureRotation = textureRotation;
				region.customTextureScale = textureScale;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted && region.customMaterial.color != hudMatCountry.color) {
				Material clonedMat = Instantiate (region.customMaterial);
				clonedMat.hideFlags = HideFlags.DontSave;
				clonedMat.name = region.customMaterial.name;
				clonedMat.color = hudMatCountry.color;
				ApplyMaterialToSurface(surf, clonedMat);
				countryRegionHighlightedObj = surf;
			}
		}

		
		/// <summary>
		/// Uncolorize/hide specified country by index in the countries collection.
		/// </summary>
		public void HideCountryRegionSurface (int countryIndex, int regionIndex)
		{
			if (_countryHighlightedIndex != countryIndex || _countryRegionHighlightedIndex != regionIndex) {
				int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
				if (surfaces.ContainsKey (cacheIndex)) {
					if (surfaces [cacheIndex] == null) {
						surfaces.Remove (cacheIndex);
					} else {
						surfaces [cacheIndex].SetActive (false);
					}
				}
			}
			countries [countryIndex].regions [regionIndex].customMaterial = null;
		}

		/// <summary>
		/// Hides all colorized regions of all countries.
		/// </summary>
		public void HideCountrySurfaces ()
		{
			for (int c=0; c<countries.Length; c++) {
				HideCountrySurface (c);
			}
		}

		/// <summary>
		/// Flashes specified country by index in the countries collection.
		/// </summary>
		public void BlinkCountry (int countryIndex, Color color1, Color color2, float duration, float blinkingSpeed)
		{
			int mainRegionIndex = countries [countryIndex].mainRegionIndex;
			BlinkCountry (countryIndex, mainRegionIndex, color1, color2, duration, blinkingSpeed);
		}

		/// <summary>
		/// Flashes specified country's region.
		/// </summary>
		public void BlinkCountry (int countryIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed)
		{
			int cacheIndex = GetCacheIndexForCountryRegion (countryIndex, regionIndex);
			GameObject surf;
			bool disableAtEnd;
			if (surfaces.ContainsKey (cacheIndex)) {
				surf = surfaces [cacheIndex];
				disableAtEnd = !surf.activeSelf;
			} else {
				surf = GenerateCountryRegionSurface (countryIndex, regionIndex, hudMatCountry, _showOutline);
				disableAtEnd = true;
			}
			SurfaceBlinker sb = surf.AddComponent<SurfaceBlinker> ();
			sb.blinkMaterial = hudMatCountry;
			sb.color1 = color1;
			sb.color2 = color2;
			sb.duration = duration;
			sb.speed = blinkingSpeed;
			sb.disableAtEnd = disableAtEnd;
			sb.customizableSurface = countries [countryIndex].regions [regionIndex];
			surf.SetActive (true);
		}

		/// <summary>
		/// Returns an array of country names. The returning list can be grouped by continent.
		/// </summary>
		public string[] GetCountryNames (bool groupByContinent)
		{
			List<string> c = new List<string> ();
			if (countries == null)
				return c.ToArray ();
			Dictionary<string, bool> continentsAdded = new Dictionary<string, bool> ();
			for (int k=0; k<countries.Length; k++) {
				Country country = countries [k];
				if (groupByContinent) {
					if (!continentsAdded.ContainsKey (country.continent)) {
						continentsAdded.Add (country.continent, true);
						c.Add (country.continent);
					}
					c.Add (country.continent + "|" + country.name + " (" + k + ")");
				} else {
					c.Add (country.name + " (" + k + ")");
				}
			}
			c.Sort ();

			if (groupByContinent) {
				int k = -1;
				while (++k<c.Count) {
					int i = c [k].IndexOf ('|');
					if (i > 0) {
						c [k] = "  " + c [k].Substring (i + 1);
					}
				}
			}
			return c.ToArray ();
		}

		#endregion

	}

}