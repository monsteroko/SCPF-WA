using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

//using WPMF_Editor;

namespace WPMF {
				[CustomEditor (typeof(WorldMap2D))]
				public class WorldMap2DInspector : Editor {
								WorldMap2D _map;
								Texture2D _headerTexture, _blackTexture;
								string[] earthStyleOptions, frontiersDetailOptions;
								int[] earthStyleValues;
								GUIStyle blackBack, sectionHeaderNormalStyle;
								SerializedProperty isDirty;
								bool expandEarthSection, expandCitiesSection, expandCountriesSection, expandProvincesSection, expandInteractionSection;

								void OnEnable () {

												_map = (WorldMap2D)target;
												_headerTexture = Resources.Load<Texture2D> ("WPMF/EditorHeader");
												blackBack = new GUIStyle ();
												blackBack.normal.background = MakeTex (4, 4, Color.black);

												if (_map.countries == null) {
																_map.Init ();
												}
												earthStyleOptions = new string[] {
																"Natural", "Natural (High Resolution, 8K)", "Natural (High Resolution, 16K)", "Scenic", "Scenic (High Resolution, 16K)", "Alternate Style 1", "Alternate Style 2", "Alternate Style 3", "Solid Color"
												};
												earthStyleValues = new int[] {
																(int)EARTH_STYLE.Natural, (int)EARTH_STYLE.NaturalHighRes, (int)EARTH_STYLE.NaturalHighRes16K, (int)EARTH_STYLE.NaturalScenic, (int)EARTH_STYLE.NaturalScenic16K, (int)EARTH_STYLE.Alternate1, (int)EARTH_STYLE.Alternate2, (int)EARTH_STYLE.Alternate3, (int)EARTH_STYLE.SolidColor
												};

												frontiersDetailOptions = new string[] {
																"Low",
																"High"
												};
												isDirty = serializedObject.FindProperty ("isDirty");

												expandEarthSection = EditorPrefs.GetBool ("WPM2DGlobeEarthExpand", false);
												expandCitiesSection = EditorPrefs.GetBool ("WPM2DGlobeCitiesExpand", false);
												expandCountriesSection = EditorPrefs.GetBool ("WPM2DGlobeCountriesExpand", false);
												expandProvincesSection = EditorPrefs.GetBool ("WPM2DGlobeProvincesExpand", false);
												expandInteractionSection = EditorPrefs.GetBool ("WPM2DGlobeInteractionExpand", false);
								}

								void OnDestroy () {
												EditorPrefs.SetBool ("WPM2DGlobeEarthExpand", expandEarthSection);
												EditorPrefs.SetBool ("WPM2DGlobeCitiesExpand", expandCitiesSection);
												EditorPrefs.SetBool ("WPM2DGlobeCountriesExpand", expandCountriesSection);
												EditorPrefs.SetBool ("WPM2DGlobeProvincesExpand", expandProvincesSection);
												EditorPrefs.SetBool ("WPM2DGlobeInteractionExpand", expandInteractionSection);
								}

								public override void OnInspectorGUI () {

												if (sectionHeaderNormalStyle == null) {
																sectionHeaderNormalStyle = new GUIStyle (EditorStyles.foldout);
																sectionHeaderNormalStyle.SetFoldoutColor();
												}
			
												EditorGUILayout.Separator ();
												GUI.skin.label.alignment = TextAnchor.MiddleCenter;  
												GUILayout.BeginHorizontal (blackBack);
												GUILayout.Label (_headerTexture, GUILayout.ExpandWidth (true));
												GUI.skin.label.alignment = TextAnchor.MiddleLeft;  
												GUILayout.EndHorizontal ();

												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();

												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label (new GUIContent ("Camera", "By default the main camera is used. However, if you need to take into account another camera for any reason, just drag and drop or assign that other camera object."), GUILayout.Width (120));
												_map.mainCamera = (Camera)EditorGUILayout.ObjectField (_map.mainCamera, typeof(Camera), true);
												EditorGUILayout.EndHorizontal ();
												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label ("Fit Window Width", GUILayout.Width (120));
												_map.fitWindowWidth = EditorGUILayout.Toggle (_map.fitWindowWidth);
												GUILayout.Label ("Fit Window Height");
												_map.fitWindowHeight = EditorGUILayout.Toggle (_map.fitWindowHeight);
												if (GUILayout.Button ("Center Map")) {
																_map.CenterMap ();
												}
												EditorGUILayout.EndHorizontal ();

												EditorGUILayout.BeginHorizontal ();
												GUILayout.Label ("Render Viewport", GUILayout.Width (120));
												_map.renderViewport = (GameObject)EditorGUILayout.ObjectField (_map.renderViewport, typeof(GameObject), true);
												if (GUILayout.Button ("?", GUILayout.Width (24))) {
																EditorUtility.DisplayDialog ("Render Viewport Help", "Render Viewport allows to display the map onto a Viewport GameObject, cropping the map according to the size of the viewport.\n\nTo use this feature drag a Viewport prefab into the scene and assign the viewport gameobject created to this property.", "Ok");
												}
												EditorGUILayout.EndHorizontal ();

//			if (_map.renderViewport!=_map.gameObject) {
//				EditorGUILayout.BeginHorizontal ();
//				GUILayout.Label ("  Render Quality", GUILayout.Width (120));
//				_map.renderViewportQuality = (VIEWPORT_QUALITY)EditorGUILayout.Popup ((int)_map.renderViewportQuality, renderViewportQualityOptions);
//				EditorGUILayout.EndHorizontal ();
//			}

												if (_map.renderViewport != null && _map.renderViewport != _map.gameObject) {
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Filter Mode", GUILayout.Width (120));
																_map.renderViewportFilterMode = (FilterMode)EditorGUILayout.EnumPopup (_map.renderViewportFilterMode);
																EditorGUILayout.EndHorizontal ();
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label (new GUIContent ("Screen Overlay Mode", "Check this toggle to move the render viewport to front and align it with the camera."), GUILayout.Width (120));
																_map.renderViewPortScreenOverlay = EditorGUILayout.Toggle (_map.renderViewPortScreenOverlay);
																EditorGUILayout.EndHorizontal ();

																if (_map.renderViewPortScreenOverlay) {
																				EditorGUILayout.BeginHorizontal ();
																				float left, top, width, height;
																				EditorGUILayout.LabelField ("   Left", GUILayout.Width (45));
																				left = EditorGUILayout.FloatField (_map.renderViewportScreenRect.x, GUILayout.Width (40));
																				EditorGUILayout.LabelField ("Bottom", GUILayout.Width (45));
																				top = EditorGUILayout.FloatField (_map.renderViewportScreenRect.y, GUILayout.Width (40));
																				if (GUILayout.Button ("Full Screen", GUILayout.Width (80))) {
																								_map.renderViewportScreenRect = new Rect (0f, 0f, 1f, 1f);
																								_map.isDirty = true;
																								EditorGUIUtility.ExitGUI ();
																				}
																				if (GUILayout.Button ("?", GUILayout.Width (20))) {
																								EditorUtility.DisplayDialog ("Screen Rect", "The Rect which defines the viewport position in the screen (0=left/bottom, 1=top/right).", "Ok");
																								EditorGUIUtility.ExitGUI ();
																				}
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				EditorGUILayout.LabelField ("   Width", GUILayout.Width (45));
																				width = EditorGUILayout.FloatField (_map.renderViewportScreenRect.width, GUILayout.Width (40));
																				EditorGUILayout.LabelField ("Height", GUILayout.Width (45));
																				height = EditorGUILayout.FloatField (_map.renderViewportScreenRect.height, GUILayout.Width (40));
																				_map.renderViewportScreenRect = new Rect (left, top, width, height);
																				EditorGUILayout.EndHorizontal ();
																}
												}
			
												EditorGUILayout.EndVertical (); 
												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();

//												GUIStyle labelStyle = _map.showEarth ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
												EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
												expandEarthSection = EditorGUILayout.Foldout (expandEarthSection, "Earth Settings", sectionHeaderNormalStyle); 
												EditorGUILayout.EndHorizontal ();
												if (expandEarthSection) {

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Earth", GUILayout.Width (120));
																_map.showEarth = EditorGUILayout.Toggle (_map.showEarth);
																EditorGUILayout.EndHorizontal ();

																if (_map.showEarth) {
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Earth Style", GUILayout.Width (120));
																				_map.earthStyle = (EARTH_STYLE)EditorGUILayout.IntPopup ((int)_map.earthStyle, earthStyleOptions, earthStyleValues);

																				if (_map.earthStyle == EARTH_STYLE.SolidColor) {
																								GUILayout.Label ("Color");
																								_map.earthColor = EditorGUILayout.ColorField (_map.earthColor, GUILayout.Width (50));
																				}
																				EditorGUILayout.EndHorizontal ();
																}

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Latitude Lines", GUILayout.Width (120));
																_map.showLatitudeLines = EditorGUILayout.Toggle (_map.showLatitudeLines);
																GUILayout.Label ("Stepping");
																_map.latitudeStepping = EditorGUILayout.IntSlider (_map.latitudeStepping, 5, 45);
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Longitude Lines", GUILayout.Width (120));
																_map.showLongitudeLines = EditorGUILayout.Toggle (_map.showLongitudeLines);
																GUILayout.Label ("Stepping");
																_map.longitudeStepping = EditorGUILayout.IntSlider (_map.longitudeStepping, 5, 45);
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Grid Color", GUILayout.Width (120));
																_map.gridLinesColor = EditorGUILayout.ColorField (_map.gridLinesColor, GUILayout.Width (50));
																EditorGUILayout.EndHorizontal ();
												}

												EditorGUILayout.EndVertical (); 
												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();

//												labelStyle = _map.showCities ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
												EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
												expandCitiesSection = EditorGUILayout.Foldout (expandCitiesSection, "Cities Settings", sectionHeaderNormalStyle); 
												EditorGUILayout.EndHorizontal ();
												if (expandCitiesSection) {

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Cities", GUILayout.Width (120));
																_map.showCities = EditorGUILayout.Toggle (_map.showCities);
																EditorGUILayout.EndHorizontal ();

																if (_map.showCities) {
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Cities Color", GUILayout.Width (120));
																				_map.citiesColor = EditorGUILayout.ColorField (_map.citiesColor, GUILayout.Width (40));
																				GUILayout.Label ("Region Cap.");
																				_map.citiesRegionCapitalColor = EditorGUILayout.ColorField (_map.citiesRegionCapitalColor, GUILayout.Width (40));
																				GUILayout.Label ("Capital");
																				_map.citiesCountryCapitalColor = EditorGUILayout.ColorField (_map.citiesCountryCapitalColor, GUILayout.Width (40));
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   City Class", GUILayout.Width (120));
																				CITY_CLASS_FILTER prevFilter = _map.cityClassFilter;
																				_map.cityClassFilter = (CITY_CLASS_FILTER)EditorGUILayout.EnumPopup (_map.cityClassFilter);
																				EditorGUILayout.EndHorizontal ();
																				if (prevFilter != _map.cityClassFilter) {
																								GUIUtility.ExitGUI ();
																				}

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Min Population (K)", GUILayout.Width (120));
																				_map.minPopulation = EditorGUILayout.IntSlider (_map.minPopulation, 0, 3000);
																				GUILayout.Label (_map.numCitiesDrawn + "/" + _map.cities.Count);
																				EditorGUILayout.EndHorizontal ();

																				if (_map.cityClassFilter == CITY_CLASS_FILTER.Any) {
																								EditorGUILayout.BeginHorizontal ();
																								GUILayout.Label ("   Always Visible:", GUILayout.Width (120));
																								int cityClassFilter = 0;
																								bool cityBit;
																								cityBit = EditorGUILayout.Toggle ((_map.cityClassAlwaysShow & WorldMap2D.CITY_CLASS_FILTER_REGION_CAPITAL_CITY) != 0, GUILayout.Width (20));
																								GUILayout.Label ("Region Capitals");
																								if (cityBit)
																												cityClassFilter += WorldMap2D.CITY_CLASS_FILTER_REGION_CAPITAL_CITY;
																								cityBit = EditorGUILayout.Toggle ((_map.cityClassAlwaysShow & WorldMap2D.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY) != 0, GUILayout.Width (20));
																								GUILayout.Label ("Country Capitals");
																								if (cityBit)
																												cityClassFilter += WorldMap2D.CITY_CLASS_FILTER_COUNTRY_CAPITAL_CITY;
																								_map.cityClassAlwaysShow = cityClassFilter;
																								EditorGUILayout.EndHorizontal ();
																				}

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Icon Size", GUILayout.Width (120));
																				_map.cityIconSize = EditorGUILayout.Slider (_map.cityIconSize, 0.1f, 5.0f);
																				EditorGUILayout.EndHorizontal ();
				
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Hit Test Radius", GUILayout.Width (120));
																				_map.cityHitTestRadius = EditorGUILayout.Slider (_map.cityHitTestRadius, 1f, 400f);
																				EditorGUILayout.EndHorizontal ();

																}
												}

												EditorGUILayout.EndVertical (); 
												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();

			
//												labelStyle = _map.showCountryNames || _map.showFrontiers ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
												EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
												expandCountriesSection = EditorGUILayout.Foldout (expandCountriesSection, "Countries Settings", sectionHeaderNormalStyle); 
												EditorGUILayout.EndHorizontal ();
												if (expandCountriesSection) {

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Countries", GUILayout.Width (120));
																_map.showFrontiers = EditorGUILayout.Toggle (_map.showFrontiers);
																EditorGUILayout.EndHorizontal ();

																if (_map.showFrontiers) {
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Frontiers Detail", GUILayout.Width (120));
																				_map.frontiersDetail = (FRONTIERS_DETAIL)EditorGUILayout.Popup ((int)_map.frontiersDetail, frontiersDetailOptions);

																				GUILayout.Label (_map.countries.Length.ToString ());
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Frontiers Color", GUILayout.Width (120));
																				_map.frontiersColor = EditorGUILayout.ColorField (_map.frontiersColor); //, GUILayout.Width (50));
																				GUILayout.Label ("Outer Color", GUILayout.Width (120));
																				_map.frontiersColorOuter = EditorGUILayout.ColorField (_map.frontiersColorOuter, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label (new GUIContent ("   Thin Lines", "If set to true, country frontiers lines won't get thicker when you zoom in reducing vertex count (useful in VR or mobile)."), GUILayout.Width (120));
																				_map.frontiersThinLines = EditorGUILayout.Toggle (_map.frontiersThinLines);
																				EditorGUILayout.EndHorizontal ();


																}

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Country Highlight", GUILayout.Width (120));
																_map.enableCountryHighlight = EditorGUILayout.Toggle (_map.enableCountryHighlight);

																if (_map.enableCountryHighlight) {
																				GUILayout.Label ("Highlight Color", GUILayout.Width (120));
																				_map.fillColor = EditorGUILayout.ColorField (_map.fillColor, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Draw Outline", GUILayout.Width (120));
																				_map.showOutline = EditorGUILayout.Toggle (_map.showOutline);
																				if (_map.showOutline) {
																								GUILayout.Label ("Outline Color", GUILayout.Width (120));
																								_map.outlineColor = EditorGUILayout.ColorField (_map.outlineColor, GUILayout.Width (50));
																				}
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Include All Regions", GUILayout.Width (120));
																				_map.highlightAllCountryRegions = EditorGUILayout.Toggle (_map.highlightAllCountryRegions);
																}
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.EndVertical (); 
																EditorGUILayout.Separator ();
																EditorGUILayout.BeginVertical ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Country Names", GUILayout.Width (120));
																_map.showCountryNames = EditorGUILayout.Toggle (_map.showCountryNames);
																EditorGUILayout.EndHorizontal ();

																if (_map.showCountryNames) {
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("  Relative Size", GUILayout.Width (120));
																				_map.countryLabelsSize = EditorGUILayout.Slider (_map.countryLabelsSize, 0.01f, 0.9f);
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("  Minimum Size", GUILayout.Width (120));
																				_map.countryLabelsAbsoluteMinimumSize = EditorGUILayout.Slider (_map.countryLabelsAbsoluteMinimumSize, 0.001f, 2.5f);
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("  Font", GUILayout.Width (120));
																				_map.countryLabelsFont = (Font)EditorGUILayout.ObjectField (_map.countryLabelsFont, typeof(Font), false);
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("  Labels Color", GUILayout.Width (120));
																				_map.countryLabelsColor = EditorGUILayout.ColorField (_map.countryLabelsColor, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("  Draw Shadow", GUILayout.Width (120));
																				_map.showLabelsShadow = EditorGUILayout.Toggle (_map.showLabelsShadow);
																				if (_map.showLabelsShadow) {
																								GUILayout.Label ("Shadow Color", GUILayout.Width (120));
																								_map.countryLabelsShadowColor = EditorGUILayout.ColorField (_map.countryLabelsShadowColor, GUILayout.Width (50));
																				}
																				EditorGUILayout.EndHorizontal ();
																}
												}

												EditorGUILayout.EndVertical (); 
												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();

			
//												labelStyle = _map.showProvinces ? sectionHeaderBoldStyle : sectionHeaderNormalStyle;
												EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
												expandProvincesSection = EditorGUILayout.Foldout (expandProvincesSection, "Provinces Settings", sectionHeaderNormalStyle); 
												EditorGUILayout.EndHorizontal ();
												if (expandProvincesSection) {

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Provinces", GUILayout.Width (120));
																_map.showProvinces = EditorGUILayout.Toggle (_map.showProvinces);
																if (_map.showProvinces) {
																				GUILayout.Label ("Draw All Provinces", GUILayout.Width (120));
																				_map.drawAllProvinces = EditorGUILayout.Toggle (_map.drawAllProvinces, GUILayout.Width (50));
																}
																EditorGUILayout.EndHorizontal ();

																if (_map.showProvinces) {
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Borders Color", GUILayout.Width (120));
																				_map.provincesColor = EditorGUILayout.ColorField (_map.provincesColor, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();

																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("Province Highlight", GUILayout.Width (120));
																				_map.enableProvinceHighlight = EditorGUILayout.Toggle (_map.enableProvinceHighlight);
																				GUILayout.Label ("Highlight Color", GUILayout.Width (120));
																				_map.provincesFillColor = EditorGUILayout.ColorField (_map.provincesFillColor, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();
																}
												}
												EditorGUILayout.EndVertical (); 
												EditorGUILayout.Separator ();
												EditorGUILayout.BeginVertical ();
			
												EditorGUILayout.BeginHorizontal (GUILayout.Width (90));
												expandInteractionSection = EditorGUILayout.Foldout (expandInteractionSection, "Interaction Settings", sectionHeaderNormalStyle); 
												EditorGUILayout.EndHorizontal ();
												if (expandInteractionSection) {

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Show Cursor", GUILayout.Width (120));
																_map.showCursor = EditorGUILayout.Toggle (_map.showCursor);

																if (_map.showCursor) {
																				GUILayout.Label ("Cursor Color", GUILayout.Width (120));
																				_map.cursorColor = EditorGUILayout.ColorField (_map.cursorColor, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Follow Mouse", GUILayout.Width (120));
																				_map.cursorFollowMouse = EditorGUILayout.Toggle (_map.cursorFollowMouse);
																}
																EditorGUILayout.EndHorizontal ();
			
																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Allow User Drag", GUILayout.Width (120));
																_map.allowUserDrag = EditorGUILayout.Toggle (_map.allowUserDrag, GUILayout.Width (30));
																if (_map.allowUserDrag) {
																				GUILayout.Label ("Speed");
																				_map.mouseDragSensitivity = EditorGUILayout.Slider (_map.mouseDragSensitivity, 0.1f, 3);
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label (new GUIContent ("   Drag Threshold", "Enter a threshold value to avoid accidental map dragging when clicking on HiDpi screens. Values of 5, 10, 20 or more, depending on the sensitivity of the screen."), GUILayout.Width (120));
																				_map.mouseDragThreshold = EditorGUILayout.IntField (_map.mouseDragThreshold, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Right Click Centers", GUILayout.Width (120));
																				_map.centerOnRightClick = EditorGUILayout.Toggle (_map.centerOnRightClick, GUILayout.Width (30));
																				GUILayout.Label ("Constant Drag Speed", GUILayout.Width (130));
																				_map.dragConstantSpeed = EditorGUILayout.Toggle (_map.dragConstantSpeed, GUILayout.Width (50));
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Allow Keys (WASD)", GUILayout.Width (120));
																				_map.allowUserKeys = EditorGUILayout.Toggle (_map.allowUserKeys, GUILayout.Width (30));
																				if (_map.allowUserKeys) {
																								GUILayout.Label ("Flip Direction", GUILayout.Width (120));
																								_map.dragFlipDirection = EditorGUILayout.Toggle (_map.dragFlipDirection, GUILayout.Width (50));
																				}
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Screen Edge Scroll", GUILayout.Width (120));
																				_map.allowScrollOnScreenEdges = EditorGUILayout.Toggle (_map.allowScrollOnScreenEdges, GUILayout.Width (30));
																				if (_map.allowScrollOnScreenEdges) {
																								GUILayout.Label ("Edge Thickness");
																								_map.screenEdgeThickness = EditorGUILayout.IntSlider (_map.screenEdgeThickness, 1, 10);
																				}
																}
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Allow User Zoom", GUILayout.Width (120));
																_map.allowUserZoom = EditorGUILayout.Toggle (_map.allowUserZoom, GUILayout.Width (30));
																if (_map.allowUserZoom) {
																				GUILayout.Label ("Speed");
																				_map.mouseWheelSensitivity = EditorGUILayout.Slider (_map.mouseWheelSensitivity, 0.1f, 3);
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label ("   Invert Direction", GUILayout.Width (120));
																				_map.invertZoomDirection = EditorGUILayout.Toggle (_map.invertZoomDirection, GUILayout.Width (30));
																				GUILayout.Label ("Constant Zoom Speed", GUILayout.Width (130));
																				_map.zoomConstantSpeed = EditorGUILayout.Toggle (_map.zoomConstantSpeed, GUILayout.Width (30));
																				EditorGUILayout.EndHorizontal ();
																				EditorGUILayout.BeginHorizontal ();
																				GUILayout.Label (new GUIContent ("   Distance Min", "0 = default min distance"), GUILayout.Width (120));
																				_map.zoomMinDistance = EditorGUILayout.FloatField (_map.zoomMinDistance, GUILayout.Width (50));
																				GUILayout.Label (new GUIContent ("   Max", "10m = default max distance"), GUILayout.Width (40));
																				_map.zoomMaxDistance = EditorGUILayout.FloatField (_map.zoomMaxDistance, GUILayout.Width (50));
																}
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label (new GUIContent ("Static Camera", "If set to true, it'll be the map and not the camera what moves when user drag or zooms in/out."), GUILayout.Width (120));
																_map.staticCamera = EditorGUILayout.Toggle (_map.staticCamera, GUILayout.Width (30));
																EditorGUILayout.EndHorizontal ();

																EditorGUILayout.BeginHorizontal ();
																GUILayout.Label ("Navigation Time", GUILayout.Width (120));
																_map.navigationTime = EditorGUILayout.Slider (_map.navigationTime, 0, 10);
																EditorGUILayout.EndHorizontal ();
												}
												EditorGUILayout.EndVertical ();

												// Extra components opener
												EditorGUILayout.Separator ();
												float buttonWidth = Screen.width * 0.4f;
												if (_map.gameObject.activeInHierarchy) {
																EditorGUILayout.BeginHorizontal ();
																GUILayout.FlexibleSpace ();

																if (_map.gameObject.GetComponent<WorldMap2D_Calculator> () == null) {
																				if (GUILayout.Button ("Open Calculator", GUILayout.Width (buttonWidth))) {
																								_map.gameObject.AddComponent<WorldMap2D_Calculator> ();
																				}
																} else {
																				if (GUILayout.Button ("Hide Calculator", GUILayout.Width (buttonWidth))) {
																								DestroyImmediate (_map.gameObject.GetComponent<WorldMap2D_Calculator> ());
																								EditorGUIUtility.ExitGUI ();
																				}
																}

																if (_map.gameObject.GetComponent<WorldMap2D_Ticker> () == null) {
																				if (GUILayout.Button ("Open Ticker", GUILayout.Width (buttonWidth))) {
																								_map.gameObject.AddComponent<WorldMap2D_Ticker> ();
																				}
																} else {
																				if (GUILayout.Button ("Hide Ticker", GUILayout.Width (buttonWidth))) {
																								DestroyImmediate (_map.gameObject.GetComponent<WorldMap2D_Ticker> ());
																								EditorGUIUtility.ExitGUI ();
																				}
																}
																GUILayout.FlexibleSpace ();
																EditorGUILayout.EndHorizontal ();
																EditorGUILayout.BeginHorizontal ();
																GUILayout.FlexibleSpace ();
																if (_map.gameObject.GetComponent<WorldMap2D_Editor> () == null) {
																				if (GUILayout.Button ("Open Editor", GUILayout.Width (buttonWidth))) {
																								if (_map.mainCamera.orthographic) {
																												Debug.LogError ("Map Editor does not support orthographic camera!");
																								} else {
																												_map.gameObject.AddComponent<WorldMap2D_Editor> ();
																								}
																				}
																} else {
																				if (GUILayout.Button ("Hide Editor", GUILayout.Width (buttonWidth))) {
																								_map.HideProvinces ();
																								_map.HideCountrySurfaces ();
																								_map.HideProvinceSurfaces ();
																								_map.Redraw ();
																								DestroyImmediate (_map.gameObject.GetComponent<WorldMap2D_Editor> ());
																								EditorGUIUtility.ExitGUI ();
																				}
																}

																if (_map.gameObject.GetComponent<WorldMap2D_Decorator> () == null) {
																				if (GUILayout.Button ("Open Decorator", GUILayout.Width (buttonWidth))) {
																								_map.gameObject.AddComponent<WorldMap2D_Decorator> ();
																				}
																} else {
																				if (GUILayout.Button ("Hide Decorator", GUILayout.Width (buttonWidth))) {
																								DestroyImmediate (_map.gameObject.GetComponent<WorldMap2D_Decorator> ());
																								EditorGUIUtility.ExitGUI ();
																				}
																}

																GUILayout.FlexibleSpace ();
																EditorGUILayout.EndHorizontal ();
												}

												EditorGUILayout.BeginHorizontal ();
												GUILayout.FlexibleSpace ();
												if (GUILayout.Button ("About", GUILayout.Width (buttonWidth * 2.0f))) {
																WorldMap2DAbout.ShowAboutWindow ();
												}
												GUILayout.FlexibleSpace ();
												EditorGUILayout.EndHorizontal ();

 
												if (_map.isDirty) {
#if UNITY_5_6_OR_NEWER
				serializedObject.UpdateIfRequiredOrScript ();
#else
																serializedObject.UpdateIfDirtyOrScript ();
#endif
																isDirty.boolValue = false;
																serializedObject.ApplyModifiedProperties ();
																EditorUtility.SetDirty (target);
												}

								}

								Texture2D MakeTex (int width, int height, Color col) {
												Color[] pix = new Color[width * height];
			
												for (int i = 0; i < pix.Length; i++)
																pix [i] = col;
			
												TextureFormat tf = SystemInfo.SupportsTextureFormat (TextureFormat.RGBAFloat) ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
												Texture2D result = new Texture2D (width, height, tf, false);
												result.SetPixels (pix);
												result.Apply ();
			
												return result;
								}



				}

}