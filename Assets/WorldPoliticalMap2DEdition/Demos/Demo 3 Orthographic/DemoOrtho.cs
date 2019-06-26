using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using WPMF;

public class DemoOrtho : MonoBehaviour {

	WorldMap2D map;
	GUIStyle labelStyle, labelStyleShadow, buttonStyle, sliderStyle, sliderThumbStyle;
	ColorPicker colorPicker;
	bool changingFrontiersColor;
	bool minimizeState = false;
	bool viewportDemoStarted = false;
	GameObject viewport;
	float viewportDemoStartTime;
	float viewportAnimAngle;
	float zoomLevel = 1.0f;

	void Start () {
		// Get a reference to the World Map API:
		map = WorldMap2D.instance;

		// UI Setup - non-important, only for this demo
		labelStyle = new GUIStyle ();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.normal.textColor = Color.white;
		labelStyleShadow = new GUIStyle (labelStyle);
		labelStyleShadow.normal.textColor = Color.black;
		buttonStyle = new GUIStyle (labelStyle);
		buttonStyle.alignment = TextAnchor.MiddleLeft;
		buttonStyle.normal.background = Texture2D.whiteTexture;
		buttonStyle.normal.textColor = Color.white;
		colorPicker = gameObject.GetComponent<ColorPicker> ();
		sliderStyle = new GUIStyle ();
		sliderStyle.normal.background = Texture2D.whiteTexture;
		sliderStyle.fixedHeight = 4.0f;
		sliderThumbStyle = new GUIStyle ();
		sliderThumbStyle.normal.background = Resources.Load<Texture2D> ("thumb");
		sliderThumbStyle.overflow = new RectOffset (0, 0, 8, 0);
		sliderThumbStyle.fixedWidth = 20.0f;
		sliderThumbStyle.fixedHeight = 12.0f;

		// setup GUI resizer - only for the demo
		GUIResizer.Init (800, 500); 

		/* Register events: this is optionally but allows your scripts to be informed instantly as the mouse enters or exits a country, province or city */
		map.OnCityEnter += (int cityIndex) => Debug.Log ("Entered city " + map.cities [cityIndex].name);
		map.OnCityExit += (int cityIndex) => Debug.Log ("Exited city " + map.cities [cityIndex].name);
		map.OnCityClick += (int cityIndex) => Debug.Log ("Clicked city " + map.cities [cityIndex].name);
		map.OnCountryEnter += (int countryIndex, int regionIndex) => Debug.Log ("Entered country " + map.countries [countryIndex].name);
		map.OnCountryExit += (int countryIndex, int regionIndex) => Debug.Log ("Exited country " + map.countries [countryIndex].name);
		map.OnCountryClick += (int countryIndex, int regionIndex) => Debug.Log ("Clicked country " + map.countries [countryIndex].name);
		map.OnProvinceEnter += (int provinceIndex, int regionIndex) => Debug.Log ("Entered province " + map.provinces [provinceIndex].name);
		map.OnProvinceExit += (int provinceIndex, int regionIndex) => Debug.Log ("Exited province " + map.provinces [provinceIndex].name);
		map.OnProvinceClick += (int provinceIndex, int regionIndex) => Debug.Log ("Clicked province " + map.provinces [provinceIndex].name);

		map.CenterMap();
	}

	// Update is called once per frame
	void OnGUI () {

		// Do autoresizing of GUI layer
		GUIResizer.AutoResize ();

		// Check whether a country or city is selected, then show a label with the entity name and its neighbours (new in V4.1!)
		if (map.countryHighlighted != null || map.cityHighlighted != null || map.provinceHighlighted != null) {
			string text;
			if (map.cityHighlighted != null) {
				if (!map.cityHighlighted.name.Equals(map.cityHighlighted.province)) { // show city name + province & country name
					text = "City: " + map.cityHighlighted.name + " (" + map.cityHighlighted.province + ", " + map.countries[map.cityHighlighted.countryIndex].name + ")";
				} else {	// show city name + country name (city is a capital with same name as province)
					text = "City: " + map.cityHighlighted.name + " (" + map.countries[map.cityHighlighted.countryIndex].name + ")";
				}
			} else if (map.provinceHighlighted != null) {
				text = map.provinceHighlighted.name + ", " + map.countryHighlighted.name;
				List<Province> neighbours = map.ProvinceNeighboursOfCurrentRegion();
				if (neighbours.Count>0) text += "\n" + EntityListToString<Province>(neighbours);
			} else if (map.countryHighlighted != null) {
				text = map.countryHighlighted.name + " (" + map.countryHighlighted.continent + ")";
				List<Country> neighbours = map.CountryNeighboursOfCurrentRegion();
				if (neighbours.Count>0) text += "\n" + EntityListToString<Country>(neighbours);
			} else {
				text = "";
			}
			float x ,y;
			if (minimizeState) {
				x = Screen.width - 130;
				y = Screen.height - 140;
			} else {
				x = Screen.width / 2.0f;
				y = Screen.height - 40;
			}
			// shadow
			GUI.Label (new Rect (x - 1, y - 1, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 1, y + 2, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 2, y + 3, 0, 10), text, labelStyleShadow);
			GUI.Label (new Rect (x + 3, y + 4, 0, 10), text, labelStyleShadow);
			// texst face
			GUI.Label (new Rect (x, y, 0, 10), text, labelStyle);
		}

		// Assorted options to show/hide frontiers, cities, Earth and enable country highlighting
		GUI.Box (new Rect(0,0,150,200), "");
		map.showFrontiers = GUI.Toggle (new Rect (10, 20, 150, 30), map.showFrontiers, "Toggle Frontiers");
		map.showEarth = GUI.Toggle (new Rect (10, 50, 150, 30), map.showEarth, "Toggle Earth");
		map.showCities = GUI.Toggle (new Rect (10, 80, 150, 30), map.showCities, "Toggle Cities");
		map.showCountryNames = GUI.Toggle (new Rect (10, 110, 150, 30), map.showCountryNames, "Toggle Labels");
		map.enableCountryHighlight = GUI.Toggle (new Rect (10, 140, 170, 30), map.enableCountryHighlight, "Enable Highlight");

		// buttons background color
		GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.95f);

		// Viewport mode sample: Stop button
		if (viewportDemoStarted) {
			if (GUI.Button (new Rect (10, 170, 160, 30), "  Stop Viewport Demo", buttonStyle)) {
				ViewportModeStop ();
			} else {
				ViewportModeRun();
			}
			return;
		}

		// Add button to toggle Earth texture
		if (GUI.Button (new Rect (10, 170, 160, 30), "  Change Earth style", buttonStyle)) {
			map.earthStyle = (EARTH_STYLE)(((int)map.earthStyle + 1) % 4);
		}

		// Add buttons to show the color picker and change colors for the frontiers or fill
		if (GUI.Button (new Rect (10, 210, 160, 30), "  Change Frontiers Color", buttonStyle)) {
			colorPicker.showPicker = true;
			changingFrontiersColor = true;
		}
		if (GUI.Button (new Rect (10, 250, 160, 30), "  Change Fill Color", buttonStyle)) {
			colorPicker.showPicker = true;
			changingFrontiersColor = false;
		}
		if (colorPicker.showPicker) {
			if (changingFrontiersColor) {
				map.frontiersColor = colorPicker.setColor;
			} else {
				map.fillColor = colorPicker.setColor;
			}
		}

		// Add a button which demonstrates the navigateTo functionality -- pass the name of a country
		// For a list of countries and their names, check map.Countries collection.
		if (GUI.Button (new Rect (10, 290, 180, 30), "  Fly to Australia (Country)", buttonStyle)) {
			FlyToCountry ("Australia");
		}
		if (GUI.Button (new Rect (10, 325, 180, 30), "  Fly to Mexico (Country)", buttonStyle)) {
			FlyToCountry ("Mexico");
		}
		if (GUI.Button (new Rect (10, 360, 180, 30), "  Fly to San Francisco (City)", buttonStyle)) {
			FlyToCity ("San Francisco");
		}
		if (GUI.Button (new Rect (10, 395, 180, 30), "  Fly to Madrid (City)", buttonStyle)) {
			FlyToCity ("Madrid");
		}


		// Slider to show the new set zoom level API in V4.1
		GUI.Button (new Rect (10, 430, 85, 30), "  Zoom Level", buttonStyle);
		float prevZoomLevel = zoomLevel;
		GUI.backgroundColor = Color.white;
		zoomLevel = GUI.HorizontalSlider(new Rect (100, 445, 80, 85), zoomLevel, 0, 1, sliderStyle, sliderThumbStyle);
		GUI.backgroundColor = new Color (0.1f, 0.1f, 0.3f, 0.95f);
		if (zoomLevel!=prevZoomLevel) {
			prevZoomLevel = zoomLevel;
			map.SetZoomLevel(zoomLevel);
		}


		// Add a button to colorize countries
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 20, 180, 30), "  Colorize Europe", buttonStyle)) {
			for (int colorizeIndex =0; colorizeIndex < map.countries.Length; colorizeIndex++) {
				if (map.countries [colorizeIndex].continent.Equals ("Europe")) {
					Color color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
					map.ToggleCountrySurface (map.countries [colorizeIndex].name, true, color);
				}
			}
		}

		// Colorize random country and fly to it
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 60, 180, 30), "  Colorize Random", buttonStyle)) {
			int countryIndex = Random.Range (0, map.countries.Length);
			Color color = new Color (Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f), Random.Range (0.0f, 1.0f));
			map.ToggleCountrySurface (countryIndex, true, color);
			map.BlinkCountry(countryIndex, Color.green, Color.black, 0.8f, 0.2f);
		}
			
		// Button to clear colorized countries
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 100, 180, 30), "  Reset countries", buttonStyle)) {
			map.HideCountrySurfaces ();
		}

		// Tickers sample
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 140, 180, 30), "  Tickers Sample", buttonStyle)) {
			TickerSample ();
		}

		// Decorator sample
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 180, 180, 30), "  Texture Sample", buttonStyle)) {
			TextureSample ();
		}

		// Moving the Earth sample
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 220, 180, 30), "  Toggle Minimize", buttonStyle)) {
			ToggleMinimize ();
		}

		// Moving the Earth sample
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 260, 180, 30), "  Viewport Demo", buttonStyle)) {
			ViewportModeStart ();
		}

		// Add marker sample
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 300, 180, 30), "  Add Marker", buttonStyle)) {
			AddMarkerOnRandomCity();
		}
		
		if (GUI.Button (new Rect (GUIResizer.authoredScreenWidth - 190, 340, 180, 30), "  Add Trajectories", buttonStyle)) {
			AddTrajectories();
		}
	
	}


	// Utility functions called from OnGUI:

	void FlyToCountry(int countryIndex) {
		map.FlyToCountry(countryIndex, 2.0f);
		map.BlinkCountry(countryIndex, Color.green, Color.black, 3.0f, 0.2f);
	}

	void FlyToCountry(string countryName) {
		int countryIndex = map.GetCountryIndex(countryName);
		FlyToCountry(countryIndex);
	}

	void FlyToCity(string cityName) {
		map.FlyToCity(cityName, 2.0f);
	}

	// Sample code to show how tickers work
	void TickerSample () {
		map.ticker.ResetTickerBands ();

		// Configure 1st ticker band: a red band in the northern hemisphere
		TickerBand tickerBand = map.ticker.tickerBands [0];
		tickerBand.verticalOffset = -0.3f;
		tickerBand.backgroundColor = new Color (1, 0, 0, 0.9f);
		tickerBand.scrollSpeed = 0;	// static band
		tickerBand.visible = true;
		tickerBand.autoHide = true;

		// Prepare a static, blinking, text for the red band
		TickerText tickerText = new TickerText (0, "WARNING!!");
		tickerText.horizontalOffset = -0.35f;
		tickerText.textColor = Color.yellow;
		tickerText.blinkInterval = 0.2f;
		tickerText.duration = 10.0f;

		// Draw it!
		map.ticker.AddTickerText (tickerText);

		// Configure second ticker band (below the red band)
		tickerBand = map.ticker.tickerBands [1];
		tickerBand.verticalOffset = -0.4f;
		tickerBand.verticalSize = 0.05f;
		tickerBand.backgroundColor = new Color (0, 0, 1, 0.9f);
		tickerBand.visible = true;
		tickerBand.autoHide = true;
		tickerBand.scrollSpeed = -0.15f;

		// Prepare a ticker text
		tickerText = new TickerText (1, "INCOMING MISSILE DETECTED!!");
		tickerText.textColor = Color.white;

		// Draw it!
		map.ticker.AddTickerText (tickerText);
	}


	// Sample code to show how to use decorators to assign a texsture
	void TextureSample () {
		// 1st way (best): assign a flag texture to USA using direct API - this texture will get cleared when you call HideCountrySurfaces()
		Texture2D texture = Resources.Load<Texture2D> ("flagUSA");
		int countryIndex = map.GetCountryIndex("United States of America");
		map.ToggleCountrySurface(countryIndex, true, Color.white, texture);
		
		// 2nd way: assign a flag texture to Brazil using decorator - the texture will stay when you call HideCountrySurfaces()
		string countryName = "Brazil";
		CountryDecorator decorator = new CountryDecorator ();
		decorator.isColorized = true;
		decorator.texture = Resources.Load<Texture2D> ("flagBrazil");
		decorator.textureOffset = Misc.Vector2down * 2.4f;
		map.decorator.SetCountryDecorator (0, countryName, decorator);
		
		Debug.Log ("USA flag added with direct API.");
		Debug.Log ("Brazil flag added with decorator (persistent texture).");
	}
	



	// The globe can be moved and scaled at wish
	void ToggleMinimize() {
		minimizeState = !minimizeState;

		Camera.main.transform.rotation = Quaternion.Euler(Misc.Vector3zero);
		if (minimizeState) {
			map.earthStyle = EARTH_STYLE.Alternate2;
			map.earthColor = Color.black;
			map.longitudeStepping = 4;
			map.latitudeStepping = 40;
			map.showCities = false;
			map.showCountryNames = false;
			map.gridLinesColor = new Color (0.06f, 0.23f, 0.398f);
			map.fitWindowWidth = false;
			map.fitWindowHeight = false;
			map.frontiersThinLines = true;
			map.transform.localScale = new Vector3(0.2f, 0.1f, 1f);
			map.transform.position = Camera.main.transform.position + Vector3.forward + Vector3.right * 0.75f + Vector3.down * 0.4f; 
		} else {
			map.frontiersThinLines = false;
			map.earthStyle = EARTH_STYLE.Natural;
			map.longitudeStepping = 15;
			map.latitudeStepping = 15;
			map.showCities = true;
			map.showCountryNames = true;
			map.gridLinesColor = new Color (0.16f, 0.33f, 0.498f);
			map.transform.localScale = new Vector3(2f, 1f, 1f);
			map.transform.position = Vector3.forward;
			map.fitWindowWidth = true;
			map.fitWindowHeight = true;
			map.CenterMap();
		}

	}


	// Init viewport mode
	void ViewportModeStart() {
		// instantiate a viewport gameobject where the map will be rendered
		viewport = Instantiate (Resources.Load<GameObject> ("WPMF/Prefabs/Viewport"));
		viewport.transform.position = Misc.Vector3zero;

		// assign the viewport to the map. Note that the map itself will move away (far away...)
		map.renderViewport = viewport;

		// Move camera above
		Camera.main.transform.position += Misc.Vector3up * 50;

		// start animation
		viewportDemoStarted = true;
		viewportDemoStartTime = Time.time;
	}


	// Stop viewport mode
	void ViewportModeStop() {
		// Destroy viewport
		map.renderViewport = null;
		Destroy (viewport);

		// Reset camera
		Camera.main.transform.position = Misc.Vector3zero;
		Camera.main.transform.rotation = Quaternion.Euler(Misc.Vector3zero);

		// Returns main map gameobject to original position and fit the screen width
//		map.transform.position = MiscVector.Vector3zero;
		map.fitWindowWidth = true;
		map.CenterMap();

		// stop animation
		viewportDemoStarted = false;
	}

	// Animates viewport
	void ViewportModeRun() {
		int elapsed = (int)(Time.time - this.viewportDemoStartTime);
		if ((elapsed % 20) == 0 && !map.isFlying) {
			int countryIndex = Random.Range(0,map.countries.Length);
			FlyToCountry(countryIndex);
		}
		viewport.transform.localRotation = Quaternion.Euler(new Vector3(45,0,0));

		Vector3 v = Misc.Vector3back  + Misc.Vector3right * Mathf.Cos (Time.time/10.0f);
		Camera.main.transform.position = viewport.transform.position + v.normalized * 100 + Misc.Vector3up * 50;
		Camera.main.transform.LookAt(viewport.transform.position);
	}


	string EntityListToString<T>(List<T>entities) {
		StringBuilder sb = new StringBuilder("Neighbours: ");
		for (int k=0;k<entities.Count;k++) {
			if (k>0) {
				sb.Append(", ");
			}
			sb.Append( ((IAdminEntity)entities[k]).name);
		}
		return sb.ToString();
	}

	/// <summary>
	/// Illustrates how to add custom markers over the globe using the AddMarker API.
	/// In this example a building prefab is added to a random city (see comments for other options).
	/// </summary>
	void AddMarkerOnRandomCity() {
		
		// Every marker is put on a spherical-coordinate (assuming a radius = 0.5 and relative center at zero position)
		Vector2 planeLocation;
		
		// Add a marker on a random city
		City city = map.cities[Random.Range(0, map.cities.Count)];
		planeLocation = city.unity2DLocation;
		
		// or... choose a city by its name:
//		int cityIndex = map.GetCityIndex("Moscow");
//		planeLocation = map.cities[cityIndex].unity2DLocation;
		
		// or... use the centroid of a country
//		int countryIndex = map.GetCountryIndex("Greece");
//		planeLocation = map.countries[countryIndex].center;
		
		// or... use a custom location lat/lon. Example put the building over New York:
//		map.calc.fromLatDec = 40.71f;	// 40.71 decimal degrees north
//		map.calc.fromLonDec = -74.00f;	// 74.00 decimal degrees to the west
//		map.calc.fromUnit = UNIT_TYPE.DecimalDegrees;
//		map.calc.Convert();
//		planeLocation = map.calc.toPlaneLocation;
		
		// Send the prefab to the AddMarker API setting a scale of 0.1f (this depends on your marker scales)
		GameObject star = Instantiate(Resources.Load<GameObject>("StarSprite"));
		
		map.AddMarker(star, planeLocation, 0.02f);
		
		
		// Fly to the destination and see the building created
		map.FlyToLocation(planeLocation);
		
		// Optionally add a blinking effect to the marker
		MarkerBlinker.AddTo(star, 3, 0.2f);
	}
	
	
	/// <summary>
	/// Example of how to add custom lines to the map
	/// Similar to the AddMarker functionality, you need two spherical coordinates and then call AddLine
	/// </summary>
	void AddTrajectories() {
		
		// In this example we will add random lines from 5 cities to another cities (see AddMaker example above for other options to get locations)
		
		for(int line=0;line<5;line++) {
			// Get two random cities
			int city1 = Random.Range(0, map.cities.Count);
			int city2 = Random.Range(0, map.cities.Count);
			
			// Get their sphere-coordinates
			Vector2 start = map.cities[city1].unity2DLocation;
			Vector2 end = map.cities[city2].unity2DLocation;
			
			// Add lines with random color, speeds and elevation
			Color color = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
			float elevation = Random.Range(0, 0.5f); 	// elevation is % relative to the Earth radius
			float drawingDuration = 4.0f;
			float lineWidth = 1.0f;
			float fadeAfter = 2.0f; // line stays for 2 seconds, then fades out - set this to zero to avoid line removal
			map.AddLine(start, end, color, elevation, drawingDuration, lineWidth, fadeAfter);
		}
		
	}


}

