using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WPMF;

namespace WPMF
{
	[CustomEditor(typeof(WorldMap2D_Editor))]
	public class WorldMap2D_EditorInspector : Editor
	{

		static Vector3 pointSnap = Misc.Vector3one * 0.1f;
		const float HANDLE_SIZE = 0.05f;
		const float HIT_PRECISION = 0.00075f;
		const string EDITORPREF_SCALE_WARNED = "ScaleWarned";
		WorldMap2D_Editor _editor;
		GUIStyle labelsStyle, attribHeaderStyle, warningLabelStyle, editorCaptionLabelStyle;
		GUIContent[] mainToolbarIcons;
		GUIContent[] reshapeRegionToolbarIcons, reshapeCityToolbarIcons, reshapeMountPointToolbarIcons, createToolbarIcons;
		int[] controlIds;
		bool startedReshapeRegion, startedReshapeCity, startedReshapeMountPoint, undoPushStarted;
		long tickStart;
		string[] reshapeRegionModeExplanation, reshapeCityModeExplanation, reshapeMountPointModeExplanation, editingModeOptions, editingCountryFileOptions, createModeExplanation, cityClassOptions;
		int[] cityClassValues;
		string[] emptyStringArray;

		WorldMap2D _map { get { return _editor.map; } }

		const string INFO_MSG_CHANGES_SAVED = "Changes saved. Original geodata files in /Backup folder.";
		const string INFO_MSG_REGION_DELETED = "Region deleted!";
		const string INFO_MSG_BACKUP_NOT_FOUND = "Backup folder not found!";
		const string INFO_MSG_BACKUP_RESTORED = "Backup restored.";
		const string INFO_MSG_GEODATA_LOW_QUALITY_CREATED = "Low quality geodata file created.";
		const string INFO_MSG_CITY_DELETED = "City deleted!";
		const string INFO_MSG_NO_CHANGES_TO_SAVE = "Nothing changed to save!";
		const string INFO_MSG_CHOOSE_COUNTRY = "Choose a country first.";
		const string INFO_MSG_CHOOSE_PROVINCE = "Choose a province first.";
		const string INFO_MSG_CONTINENT_DELETED = "Continent deleted!";
		const string INFO_MSG_COUNTRY_DELETED = "Country deleted!";
		const string INFO_MSG_PROVINCE_DELETED = "Province deleted!";
		const string INFO_MSG_PROVINCE_SEPARATED = "Province separated!";
		const string INFO_MSG_MOUNT_POINT_DELETED = "Mount point deleted!";

		#region Inspector lifecycle

		void OnEnable()
		{

			// Setup basic inspector stuff
			_editor = (WorldMap2D_Editor)target;
			if (_map.countries == null)
			{
				_map.Init();
			}

			// Load UI icons
			Texture2D[] icons = new Texture2D[20];
			icons[0] = Resources.Load<Texture2D>("IconSelect");
			icons[1] = Resources.Load<Texture2D>("IconPolygon");
			icons[2] = Resources.Load<Texture2D>("IconUndo");
			icons[3] = Resources.Load<Texture2D>("IconConfirm");
			icons[4] = Resources.Load<Texture2D>("IconPoint");
			icons[5] = Resources.Load<Texture2D>("IconCircle");
			icons[6] = Resources.Load<Texture2D>("IconMagnet");
			icons[7] = Resources.Load<Texture2D>("IconSplitVert");
			icons[8] = Resources.Load<Texture2D>("IconSplitHoriz");
			icons[9] = Resources.Load<Texture2D>("IconDelete");
			icons[10] = Resources.Load<Texture2D>("IconEraser");
			icons[11] = Resources.Load<Texture2D>("IconMorePoints");
			icons[12] = Resources.Load<Texture2D>("IconCreate");
			icons[13] = Resources.Load<Texture2D>("IconPenCountry");
			icons[14] = Resources.Load<Texture2D>("IconTarget");
			icons[15] = Resources.Load<Texture2D>("IconPenCountryRegion");
			icons[16] = Resources.Load<Texture2D>("IconPenProvince");
			icons[17] = Resources.Load<Texture2D>("IconPenProvinceRegion");
			icons[18] = Resources.Load<Texture2D>("IconMove");
			icons[19] = Resources.Load<Texture2D>("IconMountPoint");

			// Setup main toolbar
			mainToolbarIcons = new GUIContent[5];
			mainToolbarIcons[0] = new GUIContent("Select", icons[0], "Selection mode");
			mainToolbarIcons[1] = new GUIContent("Reshape", icons[1], "Change the shape of this entity");
			mainToolbarIcons[2] = new GUIContent("Create", icons[12], "Add a new entity to this layer");
			mainToolbarIcons[3] = new GUIContent("Revert", icons[2], "Restore shape information");
			mainToolbarIcons[4] = new GUIContent("Save", icons[3], "Confirm changes and save to file");

			// Setup reshape region command toolbar
			int RESHAPE_REGION_TOOLS_COUNT = 8;
			reshapeRegionToolbarIcons = new GUIContent[RESHAPE_REGION_TOOLS_COUNT];
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.POINT] = new GUIContent("Point", icons[4], "Single Point Tool");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.CIRCLE] = new GUIContent("Circle", icons[5], "Group Move Tool");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SPLITV] = new GUIContent("SplitV", icons[7], "Split Vertically");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SPLITH] = new GUIContent("SplitH", icons[8], "Split Horizontally");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.MAGNET] = new GUIContent("Magnet", icons[6], "Join frontiers between different regions");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.SMOOTH] = new GUIContent("Smooth", icons[11], "Add Point Tool");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.ERASER] = new GUIContent("Erase", icons[10], "Removes Point Tool");
			reshapeRegionToolbarIcons[(int)RESHAPE_REGION_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete Region or Country");
			reshapeRegionModeExplanation = new string[RESHAPE_REGION_TOOLS_COUNT];
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.POINT] = "Drag a SINGLE point of currently selected region (and its neighbour)";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.CIRCLE] = "Drag a GROUP of points of currently selected region (and from its neighbour region if present)";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SPLITV] = "Splits VERTICALLY currently selected region. One of the two splitted parts will form a new country.";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SPLITH] = "Splits HORIZONTALLY currently selected region. One of the two splitted parts will form a new country.";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.MAGNET] = "Click several times on a group of points next to a neighbour frontier to makes them JOIN. You may need to add additional points on both sides using the smooth tool.";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.SMOOTH] = "Click around currently selected region to ADD new points.";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.ERASER] = "Click on target points of currently selected region to ERASE them.";
			reshapeRegionModeExplanation[(int)RESHAPE_REGION_TOOL.DELETE] = "DELETES currently selected region. If this is the last region of the country or province, then the country or province will be deleted completely.";

			// Setup create command toolbar
			int CREATE_TOOLS_COUNT = 6;
			createToolbarIcons = new GUIContent[CREATE_TOOLS_COUNT];
			createToolbarIcons[(int)CREATE_TOOL.CITY] = new GUIContent("City", icons[14], "Create a new city");
			createToolbarIcons[(int)CREATE_TOOL.COUNTRY] = new GUIContent("Country", icons[13], "Draw a new country");
			createToolbarIcons[(int)CREATE_TOOL.COUNTRY_REGION] = new GUIContent("Co. Region", icons[15], "Draw a new region for current selected country");
			createToolbarIcons[(int)CREATE_TOOL.PROVINCE] = new GUIContent("Province", icons[16], "Draw a new province for current selected country");
			createToolbarIcons[(int)CREATE_TOOL.PROVINCE_REGION] = new GUIContent("Prov. Region", icons[17], "Draw a new region for current selected province");
			createToolbarIcons[(int)CREATE_TOOL.MOUNT_POINT] = new GUIContent("Mount Point", icons[19], "Create a new mount point");

			createModeExplanation = new string[CREATE_TOOLS_COUNT];
			createModeExplanation[(int)CREATE_TOOL.CITY] = "Click over the map to create a NEW CITY for currrent COUNTRY";
			createModeExplanation[(int)CREATE_TOOL.COUNTRY] = "Click over the map to create a polygon and add points for a NEW COUNTRY";
			createModeExplanation[(int)CREATE_TOOL.COUNTRY_REGION] = "Click over the map to create a polygon and add points for a NEW REGION of currently selected COUNTRY";
			createModeExplanation[(int)CREATE_TOOL.PROVINCE] = "Click over the map to create a polygon and add points for a NEW PROVINCE of currently selected country";
			createModeExplanation[(int)CREATE_TOOL.PROVINCE_REGION] = "Click over the map to create a polygon and add points for a NEW REGION of currently selected PROVINCE";
			createModeExplanation[(int)CREATE_TOOL.MOUNT_POINT] = "Click over the map to create a NEW MOUNT POINT for current COUNTRY and optional PROVINCE";

			// Setup reshape city tools
			int RESHAPE_CITY_TOOLS_COUNT = 2;
			reshapeCityToolbarIcons = new GUIContent[RESHAPE_CITY_TOOLS_COUNT];
			reshapeCityToolbarIcons[(int)RESHAPE_CITY_TOOL.MOVE] = new GUIContent("Move", icons[18], "Move city");
			reshapeCityToolbarIcons[(int)RESHAPE_CITY_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete city");
			reshapeCityModeExplanation = new string[RESHAPE_CITY_TOOLS_COUNT];
			reshapeCityModeExplanation[(int)RESHAPE_CITY_TOOL.MOVE] = "Click and drag currently selected CITY to change its POSITION";
			reshapeCityModeExplanation[(int)RESHAPE_CITY_TOOL.DELETE] = "DELETES currently selected CITY.";

			// Setup reshape mount point tools
			int RESHAPE_MOUNT_POINT_TOOLS_COUNT = 2;
			reshapeMountPointToolbarIcons = new GUIContent[RESHAPE_MOUNT_POINT_TOOLS_COUNT];
			reshapeMountPointToolbarIcons[(int)RESHAPE_MOUNT_POINT_TOOL.MOVE] = new GUIContent("Move", icons[18], "Move mount point");
			reshapeMountPointToolbarIcons[(int)RESHAPE_MOUNT_POINT_TOOL.DELETE] = new GUIContent("Delete", icons[9], "Delete mount point");
			reshapeMountPointModeExplanation = new string[RESHAPE_MOUNT_POINT_TOOLS_COUNT];
			reshapeMountPointModeExplanation[(int)RESHAPE_MOUNT_POINT_TOOL.MOVE] = "Click and drag currently selected MOUNT POINT to change its POSITION";
			reshapeMountPointModeExplanation[(int)RESHAPE_MOUNT_POINT_TOOL.DELETE] = "DELETES currently selected MOUNT POINT.";


			editingModeOptions = new string[] {
																"Only Countries",
																"Countries + Provinces"
												};

			editingCountryFileOptions = new string[] {
																"High Definition Geodata File",
																"Low Definition Geodata File"
												};
			cityClassOptions = new string[] {
																"City",
																"Country Capital",
																"Region Capital"
												};
			cityClassValues = new int[] {
																(int)CITY_CLASS.CITY,
																(int)CITY_CLASS.COUNTRY_CAPITAL,
																(int)CITY_CLASS.REGION_CAPITAL
												};


			emptyStringArray = new string[0];
			_map.showCities = true;

			AdjustCityIconsScale();
			AdjustMountPointIconsScale();

			CheckScale();
		}

		public override void OnInspectorGUI()
		{
			if (_editor == null)
				return;

			if (_map.showProvinces)
			{
				_editor.editingMode = EDITING_MODE.PROVINCES;
			}
			else
			{
				_editor.editingMode = EDITING_MODE.COUNTRIES;
			}

			CheckEditorStyles();

			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			if (Application.isPlaying)
			{
				EditorGUILayout.BeginHorizontal();
				DrawWarningLabel("Map Editor not available at runtime");
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
				return;
			}
			else if (_map.renderViewport != _map.gameObject)
			{
				EditorGUILayout.BeginHorizontal();
				DrawWarningLabel("Note: Map Editor doesn't work on the viewport");
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Show Layers", GUILayout.Width(90));
			EDITING_MODE prevEditingMode = _editor.editingMode;
			_editor.editingMode = (EDITING_MODE)EditorGUILayout.Popup((int)_editor.editingMode, editingModeOptions);
			if (_editor.editingMode != prevEditingMode)
			{
				ChangeEditingMode(_editor.editingMode);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();

			ShowEntitySelectors();

			EditorGUILayout.BeginVertical();

			// main toolbar
			GUIStyle toolbarStyle = new GUIStyle(GUI.skin.button);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			OPERATION_MODE prevOp = _editor.operationMode;
			_editor.operationMode = (OPERATION_MODE)GUILayout.Toolbar((int)_editor.operationMode, mainToolbarIcons, toolbarStyle, GUILayout.Height(24), GUILayout.MaxWidth(350));
			if (prevOp != _editor.operationMode)
			{
				NewShapeInit();
				ProcessOperationMode();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			if (_editor.infoMsg.Length > 0)
			{
				if (Event.current.type == EventType.Layout && (DateTime.Now - _editor.infoMsgStartTime).TotalSeconds > 5)
				{
					_editor.infoMsg = "";
				}
				else
				{
					EditorGUILayout.HelpBox(_editor.infoMsg, MessageType.Info);
				}
			}
			EditorGUILayout.Separator();
			switch (_editor.operationMode)
			{
				case OPERATION_MODE.UNDO:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					DrawWarningLabel("Discard current changes?");
					if (GUILayout.Button("Discard", GUILayout.Width(80)))
					{
						_editor.DiscardChanges();
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
					if (GUILayout.Button("Cancel", GUILayout.Width(80)))
					{
						_editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Separator();
					EditorGUILayout.EndVertical();
					EditorGUILayout.Separator();
					EditorGUILayout.BeginVertical();
					break;
				case OPERATION_MODE.CONFIRM:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					DrawWarningLabel("Save changes?");
					if (GUILayout.Button("Save", GUILayout.Width(80)))
					{
						if (SaveChanges())
						{
							_editor.SetInfoMsg(INFO_MSG_CHANGES_SAVED);
						}
						else
						{
							_editor.SetInfoMsg(INFO_MSG_NO_CHANGES_TO_SAVE);
						}
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
					if (GUILayout.Button("Cancel", GUILayout.Width(80)))
					{
						_editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Separator();
					EditorGUILayout.EndVertical();
					EditorGUILayout.Separator();
					EditorGUILayout.BeginVertical();
					break;
				case OPERATION_MODE.RESHAPE:
					if (_editor.countryIndex < 0 && _editor.cityIndex < 0)
					{
						DrawWarningLabel("No country, province nor city selected.");
						break;
					}

					if (_editor.countryIndex >= 0)
					{
						ShowReshapingRegionTools();
					}
					if (_editor.cityIndex >= 0)
					{
						ShowReshapingCityTools();
					}
					if (_editor.mountPointIndex >= 0)
					{
						ShowReshapingMountPointTools();
					}
					break;
				case OPERATION_MODE.CREATE:
					ShowCreateTools();
					break;
			}
			EditorGUILayout.EndVertical();


			CheckHideEditorMesh();
		}

		void OnSceneGUI()
		{
			ProcessOperationMode();
		}

		void CheckEditorStyles()
		{

			// Setup styles
			if (labelsStyle == null)
			{
				labelsStyle = new GUIStyle();
				labelsStyle.normal.textColor = Color.green;
				labelsStyle.alignment = TextAnchor.MiddleCenter;
			}

			if (attribHeaderStyle == null)
			{
				attribHeaderStyle = new GUIStyle(EditorStyles.foldout);
				attribHeaderStyle.SetFoldoutColor();
				attribHeaderStyle.margin = new RectOffset(24, 0, 0, 0);
			}
		}

		void ChangeEditingMode(EDITING_MODE newMode)
		{
			_editor.editingMode = newMode;
			// Ensure file is loaded by the map
			switch (_editor.editingMode)
			{
				case EDITING_MODE.COUNTRIES:
					_map.showFrontiers = true;
					_map.showProvinces = false;
					_map.HideProvinces();
					break;
				case EDITING_MODE.PROVINCES:
					_map.showProvinces = true;
					break;
			}
		}

		void ShowEntitySelectors()
		{

			// preprocesssing logic first to not interfere with layout and repaint events
			string[] provinceNames, countryNames = _editor.countryNames, cityNames = _editor.cityNames, mountPointNames = _editor.mountPointNames;
			if (_editor.editingMode != EDITING_MODE.PROVINCES)
			{
				provinceNames = emptyStringArray;
			}
			else
			{
				provinceNames = _editor.provinceNames;
				if (provinceNames == null)
					provinceNames = emptyStringArray;
			}
			if (mountPointNames == null)
				mountPointNames = emptyStringArray;

			EditorGUILayout.BeginVertical();
			// country selector
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Country", GUILayout.Width(90));
			int selection = EditorGUILayout.Popup(_editor.GUICountryIndex, countryNames);
			if (selection != _editor.GUICountryIndex)
			{
				_editor.CountrySelectByCombo(selection);
			}
			bool prevc = _editor.groupByParentAdmin;
			GUILayout.Label("Grouped");
			_editor.groupByParentAdmin = EditorGUILayout.Toggle(_editor.groupByParentAdmin, GUILayout.Width(20));
			if (_editor.groupByParentAdmin != prevc)
			{
				_editor.ReloadCountryNames();
			}
			EditorGUILayout.EndHorizontal();
			if (_editor.countryIndex >= 0)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Name", GUILayout.Width(90));
				_editor.GUICountryNewName = EditorGUILayout.TextField(_editor.GUICountryNewName);
				if (GUILayout.Button("Change"))
				{
					_editor.CountryRename();
				}
				if (GUILayout.Button("Delete"))
				{
					if (EditorUtility.DisplayDialog("Delete Country", "This option will completely delete current country and all its dependencies (cities, provinces, mount points, ...)\n\nContinue?", "Yes", "No"))
					{
						_editor.CountryDelete();
						_editor.SetInfoMsg(INFO_MSG_COUNTRY_DELETED);
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Hidden", GUILayout.Width(90));
				_editor.GUICountryHidden = EditorGUILayout.Toggle(_editor.GUICountryHidden);
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Sovereign", GUILayout.Width(90));
				_editor.GUICountryTransferToCountryIndex = EditorGUILayout.Popup(_editor.GUICountryTransferToCountryIndex, countryNames);
				if (GUILayout.Button("Transfer Region"))
				{
					if (_editor.GUICountryIndex >= 0 && _editor.GUICountryTransferToCountryIndex >= 0)
					{
						string sourceCountry = countryNames[_editor.GUICountryIndex].Trim();
						string targetCountry = countryNames[_editor.GUICountryTransferToCountryIndex].Trim();
						if (EditorUtility.DisplayDialog("Change Region's Sovereignty", "Current region of " + sourceCountry + " will join target country " + targetCountry + ".\n\nAre you sure?", "Ok", "Cancel"))
						{
							_editor.CountryRegionTransferTo();
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
					}
				}
				if (GUILayout.Button("Transfer Country"))
				{
					if (_editor.GUICountryIndex >= 0 && _editor.GUICountryTransferToCountryIndex >= 0)
					{
						string sourceCountry = countryNames[_editor.GUICountryIndex].Trim();
						string targetCountry = countryNames[_editor.GUICountryTransferToCountryIndex].Trim();
						if (EditorUtility.DisplayDialog("Change Country's Sovereignty", "Current country " + sourceCountry + " will join target country " + targetCountry + ".\n\nAre you sure?", "Ok", "Cancel"))
						{
							_editor.CountryTransferTo();
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Continent", GUILayout.Width(90));
				_editor.GUICountryNewContinent = EditorGUILayout.TextField(_editor.GUICountryNewContinent);
				GUI.enabled = _editor.countryIndex >= 0;
				if (GUILayout.Button("Rename"))
				{
					if (EditorUtility.DisplayDialog("Continent Renaming", "This option will rename the continent affecting to all countries in same continent. Continue?", "Yes", "No"))
					{
						_editor.ContinentRename();
					}
				}
				if (GUILayout.Button("Delete"))
				{
					if (EditorUtility.DisplayDialog("Delete all countries (in same continent)", "You're going to delete all countries and provinces in continent " + _map.countries[_editor.countryIndex].continent + ".\n\nAre you sure?", "Yes", "No"))
					{
						_editor.CountryDeleteSameContinent();
						_editor.SetInfoMsg(INFO_MSG_CONTINENT_DELETED);
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();

				// Country Regions
				if (_editor.countryIndex >= 0 && _editor.countryIndex < _map.countries.Length)
				{
					Country country = _map.countries[_editor.countryIndex];
					if (ShowRegionsGroup(country, _editor.countryRegionIndex))
					{
						GUIUtility.ExitGUI();
						return;
					}
				}
			}

			if (_editor.editingMode == EDITING_MODE.PROVINCES)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Province/State", GUILayout.Width(90));
				int provSelection = EditorGUILayout.Popup(_editor.GUIProvinceIndex, provinceNames);
				if (provSelection != _editor.GUIProvinceIndex)
				{
					_editor.ProvinceSelectByCombo(provSelection);
				}
				EditorGUILayout.EndHorizontal();
				if (_editor.provinceIndex >= 0)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   Name", GUILayout.Width(90));
					_editor.GUIProvinceNewName = EditorGUILayout.TextField(_editor.GUIProvinceNewName);
					if (GUILayout.Button("Change"))
					{
						_editor.ProvinceRename();
					}
					if (GUILayout.Button("Delete"))
					{
						if (EditorUtility.DisplayDialog("Delete Province", "This option will completely delete current province.\n\nContinue?", "Yes", "No"))
						{
							_editor.ProvinceDelete();
							_editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   Sovereign", GUILayout.Width(90));
					_editor.GUIProvinceTransferToCountryIndex = EditorGUILayout.Popup(_editor.GUIProvinceTransferToCountryIndex, countryNames);
					if (GUILayout.Button("Transfer"))
					{
						string sourceProvince = provinceNames[_editor.GUIProvinceIndex].Trim();
						string targetCountry = countryNames[_editor.GUIProvinceTransferToCountryIndex].Trim();
						EditorUtility.DisplayDialog("Change Province's Sovereignty", "Not available, need high definition country file. Or just fix this shitty code.", "Ok");
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   New Country", GUILayout.Width(90));
					_editor.GUIProvinceNewCountryName = EditorGUILayout.TextField(_editor.GUIProvinceNewCountryName);
					if (GUILayout.Button("Separate"))
					{
						string sourceCountryName = countryNames[_editor.GUICountryIndex].Trim();
						if (EditorUtility.DisplayDialog("Separate Province", "This option will separate selected province from its current country '" + sourceCountryName + "' to a new country named '" + _editor.GUIProvinceNewCountryName + "'.\n\nContinue?", "Yes", "No"))
						{
							_editor.ProvinceSeparate(_editor.GUIProvinceNewCountryName);
							_editor.SetInfoMsg(INFO_MSG_PROVINCE_SEPARATED);
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
					}
					EditorGUILayout.EndHorizontal();

					// Province Regions
					Province province = _map.provinces[_editor.provinceIndex];
					if (ShowRegionsGroup(province, _editor.provinceRegionIndex))
					{
						GUIUtility.ExitGUI();
						return;
					}
				}


				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(" ", GUILayout.Width(90));
				if (GUILayout.Button("Delete All Country Provinces", GUILayout.Width(180)))
				{
					if (EditorUtility.DisplayDialog("Delete All Country Provinces", "This option will delete all provinces of current country.\n\nContinue?", "Yes", "No"))
					{
						_editor.DeleteCountryProvinces();
						_editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
				}
				EditorGUILayout.EndHorizontal();

			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("City", GUILayout.Width(90));
			int citySelection = EditorGUILayout.Popup(_editor.GUICityIndex, cityNames);
			if (citySelection != _editor.GUICityIndex)
			{
				_editor.CitySelectByCombo(citySelection);
			}
			EditorGUILayout.EndHorizontal();
			if (_editor.cityIndex >= 0)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Name", GUILayout.Width(90));
				_editor.GUICityNewName = EditorGUILayout.TextField(_editor.GUICityNewName);
				if (GUILayout.Button("Change"))
				{
					UndoPushCityStartOperation("Undo Rename City");
					_editor.CityRename();
					UndoPushCityEndOperation();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Class", GUILayout.Width(90));
				_editor.GUICityClass = (CITY_CLASS)EditorGUILayout.IntPopup((int)_editor.GUICityClass, cityClassOptions, cityClassValues);
				if (GUILayout.Button("Update"))
				{
					UndoPushCityStartOperation("Undo Change City Class");
					_editor.CityClassChange();
					UndoPushCityEndOperation();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("   Population", GUILayout.Width(90));
				_editor.GUICityPopulation = EditorGUILayout.TextField(_editor.GUICityPopulation);
				if (GUILayout.Button("Change"))
				{
					int pop = 0;
					if (int.TryParse(_editor.GUICityPopulation, out pop))
					{
						UndoPushCityStartOperation("Undo Change Population");
						_editor.CityChangePopulation(pop);
						UndoPushCityEndOperation();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			if (_editor.countryIndex >= 0)
			{
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Mount Point", GUILayout.Width(90));
				int mpSelection = EditorGUILayout.Popup(_editor.GUIMountPointIndex, mountPointNames);
				if (mpSelection != _editor.GUIMountPointIndex)
				{
					_editor.MountPointSelectByCombo(mpSelection);
				}
				EditorGUILayout.EndHorizontal();
				if (_editor.mountPointIndex >= 0)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   Name", GUILayout.Width(90));
					_editor.GUIMountPointNewName = EditorGUILayout.TextField(_editor.GUIMountPointNewName);
					if (GUILayout.Button("Update"))
					{
						UndoPushMountPointStartOperation("Undo Rename Mount Point");
						_editor.MountPointRename();
						UndoPushMountPointEndOperation();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   Type", GUILayout.Width(90));
					_editor.GUIMountPointNewType = EditorGUILayout.TextField(_editor.GUIMountPointNewType);
					if (GUILayout.Button("Update"))
					{
						UndoPushMountPointStartOperation("Undo Change Mount Point Type");
						_editor.MountPointUpdateType();
						UndoPushMountPointEndOperation();
					}
					EditorGUILayout.EndHorizontal();
					MountPoint mp = _map.mountPoints[_editor.mountPointIndex];
					foreach (string key in mp.customTags.Keys)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Label("   Tag", GUILayout.Width(90));
						string newKey = EditorGUILayout.TextField(key);
						string currentValue = mp.customTags[key];
						if (!newKey.Equals(key))
						{
							mp.customTags.Remove(key);
							mp.customTags.Add(key, currentValue);
							_editor.mountPointChanges = true;
							GUIUtility.ExitGUI();
							break;
						}
						GUILayout.Label("Value");
						string newValue = EditorGUILayout.TextField(currentValue);
						if (!newValue.Equals(currentValue))
						{
							mp.customTags[key] = newValue;
							_editor.mountPointChanges = true;
							GUIUtility.ExitGUI();
							break;
						}
						if (GUILayout.Button("Remove"))
						{
							mp.customTags.Remove(key);
							_editor.mountPointChanges = true;
							GUIUtility.ExitGUI();
							break;
						}
						EditorGUILayout.EndHorizontal();
					}
					// new tag line
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("   Tag", GUILayout.Width(90));
					_editor.GUIMountPointNewTagKey = EditorGUILayout.TextField(_editor.GUIMountPointNewTagKey);
					GUILayout.Label("Value");
					_editor.GUIMountPointNewTagValue = EditorGUILayout.TextField(_editor.GUIMountPointNewTagValue);
					if (GUILayout.Button("Add"))
					{
						_editor.MountPointAddNewTag();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}

		/// <summary>
		/// Returns true if there're changes
		/// </summary>
		bool ShowRegionsGroup(IAdminEntity entity, int currentSelectedRegionIndex)
		{

			EditorGUILayout.BeginHorizontal();
			entity.foldOut = EditorGUILayout.Foldout(entity.foldOut, "Regions", attribHeaderStyle);
			EditorGUILayout.EndHorizontal();
			if (!entity.foldOut)
				return false;

			StringBuilder sb = new StringBuilder();
			int regionCount = entity.regions.Count;
			for (int k = 0; k < regionCount; k++)
			{
				EditorGUILayout.BeginHorizontal();
				sb.Length = 0;
				sb.Append("    ");
				sb.Append(k.ToString());
				if (k == entity.mainRegionIndex)
					sb.Append(" (Main)");
				if (currentSelectedRegionIndex == k)
					sb.Append(" (Selected)");
				GUILayout.Label(sb.ToString(), GUILayout.Width(140));
				if (GUILayout.Button("Select", GUILayout.Width(60)))
				{
					if (entity is Country)
					{
						_editor.countryRegionIndex = k;
						_editor.CountryRegionSelect();
					}
					else
					{
						_editor.provinceRegionIndex = k;
						_editor.ProvinceRegionSelect();
					}
				}
				if (GUILayout.Button("Remove", GUILayout.Width(60)))
				{
					if (EditorUtility.DisplayDialog("Remove Region", "Are you sure you want to remove this region?", "Ok", "Cancel"))
					{
						if (entity is Country)
						{
							_editor.countryRegionIndex = k;
							_editor.CountryRegionDelete();
						}
						else
						{
							_editor.provinceRegionIndex = k;
							_editor.ProvinceRegionDelete();
						}
						_editor.ClearSelection();
						GUIUtility.ExitGUI();
						return true;
					}
				}
				if (GUILayout.Button("Isolate", GUILayout.Width(60)))
				{
					if (EditorUtility.DisplayDialog("Isolate Region", "Are you sure you want to create a new country based on this region (note: any contained province will also be moved to the new country)?", "Ok", "Cancel"))
					{
						_editor.CountryCreate(entity.regions[k]);
						_editor.ClearSelection();
						GUIUtility.ExitGUI();
						return true;
					}
				}

				EditorGUILayout.EndHorizontal();
			}
			return false;
		}

		void ShowReshapingRegionTools()
		{
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawWarningLabel("REGION MODIFYING TOOLS");
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			RESHAPE_REGION_TOOL prevTool = _editor.reshapeRegionMode;
			int selectionGridRows = (reshapeRegionToolbarIcons.Length - 1) / 4 + 1;
			GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
			selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
			_editor.reshapeRegionMode = (RESHAPE_REGION_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeRegionMode, reshapeRegionToolbarIcons, 4, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(300));
			if (_editor.reshapeRegionMode != prevTool)
			{
				if (_editor.countryIndex >= 0)
				{
					tickStart = DateTime.Now.Ticks;
				}
				ProcessOperationMode();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
			explanationStyle.normal.textColor = new Color(0.52f, 0.66f, 0.9f);
			EditorGUILayout.HelpBox(reshapeRegionModeExplanation[(int)_editor.reshapeRegionMode], MessageType.Info); // explanationStyle, GUILayout.ExpandWidth (true));
			EditorGUILayout.EndHorizontal();

			if (_editor.reshapeRegionMode.hasCircle())
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("Circle Width", GUILayout.Width(90));
				_editor.reshapeCircleWidth = EditorGUILayout.Slider(_editor.reshapeCircleWidth, 0.001f, 0.1f);
				EditorGUILayout.EndHorizontal();
			}

			if (_editor.reshapeRegionMode == RESHAPE_REGION_TOOL.POINT || _editor.reshapeRegionMode == RESHAPE_REGION_TOOL.CIRCLE || _editor.reshapeRegionMode == RESHAPE_REGION_TOOL.ERASER)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Selected Region Only", "Applies modifications to points on the currently selected region. Enabling this option will prevent dragging shared points between two adjacent countries. Usually you won't need to enable this option unless you need to affect only the selected region and not the shared frontier."), GUILayout.Width(150));
				_editor.circleCurrentRegionOnly = EditorGUILayout.Toggle(_editor.circleCurrentRegionOnly, GUILayout.Width(20));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Keep Nearby Frontiers", "Always show all nearby country frontiers when moving points. If this option is disabled, only the current editing region will be redrawn."), GUILayout.Width(150));
				_editor.keepNearbyFrontiers = EditorGUILayout.Toggle(_editor.keepNearbyFrontiers, GUILayout.Width(20));
				EditorGUILayout.EndHorizontal();
			}

			switch (_editor.reshapeRegionMode)
			{
				case RESHAPE_REGION_TOOL.CIRCLE:
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Constant Move", GUILayout.Width(90));
					_editor.circleMoveConstant = EditorGUILayout.Toggle(_editor.circleMoveConstant, GUILayout.Width(20));
					EditorGUILayout.EndHorizontal();
					break;
				case RESHAPE_REGION_TOOL.MAGNET:
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Agressive Mode", GUILayout.Width(90));
					_editor.magnetAgressiveMode = EditorGUILayout.Toggle(_editor.magnetAgressiveMode, GUILayout.Width(20));
					EditorGUILayout.EndHorizontal();
					break;
				case RESHAPE_REGION_TOOL.SPLITV:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					DrawWarningLabel("Confirm split vertically?");
					if (GUILayout.Button("Split", GUILayout.Width(80)))
					{
						_editor.SplitVertically();
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
					if (GUILayout.Button("Cancel", GUILayout.Width(80)))
					{
						_editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;
				case RESHAPE_REGION_TOOL.SPLITH:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					DrawWarningLabel("Confirm split horizontally?");
					if (GUILayout.Button("Split", GUILayout.Width(80)))
					{
						_editor.SplitHorizontally();
						_editor.operationMode = OPERATION_MODE.SELECTION;
					}
					if (GUILayout.Button("Cancel", GUILayout.Width(80)))
					{
						_editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;
				case RESHAPE_REGION_TOOL.DELETE:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (_editor.entityIndex < 0)
					{
						DrawWarningLabel("Select a region to delete.");
					}
					else
					{
						if (_editor.editingMode == EDITING_MODE.COUNTRIES)
						{
							bool deletingRegion = _map.countries[_editor.countryIndex].regions.Count > 1;
							if (deletingRegion)
							{
								DrawWarningLabel("Confirm delete this region?");
							}
							else
							{
								DrawWarningLabel("Confirm delete this country?");
							}
							if (GUILayout.Button("Delete", GUILayout.Width(80)))
							{
								if (deletingRegion)
								{
									_editor.CountryRegionDelete();
									_editor.SetInfoMsg(INFO_MSG_REGION_DELETED);
								}
								else
								{
									_editor.CountryDelete();
									_editor.SetInfoMsg(INFO_MSG_COUNTRY_DELETED);
								}
								_editor.operationMode = OPERATION_MODE.SELECTION;
							}
						}
						else
						{
							if (_editor.provinceIndex >= 0 && _editor.provinceIndex < _map.provinces.Length)
							{
								if (_editor.provinceIndex >= 0 && _editor.provinceIndex < _map.provinces.Length)
								{
									bool deletingRegion = _map.provinces[_editor.provinceIndex].regions != null && _map.provinces[_editor.provinceIndex].regions.Count > 1;
									if (deletingRegion)
									{
										DrawWarningLabel("Confirm delete this region?");
									}
									else
									{
										DrawWarningLabel("Confirm delete this province/state?");
									}
									if (GUILayout.Button("Delete", GUILayout.Width(80)))
									{
										if (deletingRegion)
										{
											_editor.ProvinceRegionDelete();
											_editor.SetInfoMsg(INFO_MSG_REGION_DELETED);
										}
										else
										{
											_editor.ProvinceDelete();
											_editor.SetInfoMsg(INFO_MSG_PROVINCE_DELETED);
										}
										_editor.operationMode = OPERATION_MODE.SELECTION;
									}
								}
							}
						}

						if (GUILayout.Button("Cancel", GUILayout.Width(80)))
						{
							_editor.reshapeRegionMode = RESHAPE_REGION_TOOL.POINT;
						}
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
		}

		void ShowReshapingCityTools()
		{
			GUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawWarningLabel("CITY MODIFYING TOOLS");
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			RESHAPE_CITY_TOOL prevTool = _editor.reshapeCityMode;
			int selectionGridRows = (reshapeCityToolbarIcons.Length - 1) / 2 + 1;
			GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
			selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
			_editor.reshapeCityMode = (RESHAPE_CITY_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeCityMode, reshapeCityToolbarIcons, 2, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(150));
			if (_editor.reshapeCityMode != prevTool)
			{
				ProcessOperationMode();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
			explanationStyle.normal.textColor = new Color(0.52f, 0.66f, 0.9f);
			GUILayout.Box(reshapeCityModeExplanation[(int)_editor.reshapeCityMode], explanationStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();

			switch (_editor.reshapeCityMode)
			{
				case RESHAPE_CITY_TOOL.DELETE:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (_editor.cityIndex < 0)
					{
						DrawWarningLabel("Select a city to delete.");
					}
					else
					{
						DrawWarningLabel("Confirm delete this city?");
						if (GUILayout.Button("Delete", GUILayout.Width(80)))
						{
							UndoPushCityStartOperation("Undo Delete City");
							_editor.DeleteCity();
							UndoPushCityEndOperation();
							_editor.SetInfoMsg(INFO_MSG_CITY_DELETED);
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
						if (GUILayout.Button("Cancel", GUILayout.Width(80)))
						{
							_editor.reshapeCityMode = RESHAPE_CITY_TOOL.MOVE;
						}
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;
			}

			GUILayout.EndVertical();
		}

		void ShowReshapingMountPointTools()
		{
			GUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawWarningLabel("MOUNT POINT MODIFYING TOOLS");
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			RESHAPE_MOUNT_POINT_TOOL prevTool = _editor.reshapeMountPointMode;
			int selectionGridRows = (reshapeMountPointToolbarIcons.Length - 1) / 2 + 1;
			GUIStyle selectionGridStyle = new GUIStyle(GUI.skin.button);
			selectionGridStyle.margin = new RectOffset(2, 2, 2, 2);
			_editor.reshapeMountPointMode = (RESHAPE_MOUNT_POINT_TOOL)GUILayout.SelectionGrid((int)_editor.reshapeMountPointMode, reshapeMountPointToolbarIcons, 2, selectionGridStyle, GUILayout.Height(24 * selectionGridRows), GUILayout.MaxWidth(150));
			if (_editor.reshapeMountPointMode != prevTool)
			{
				ProcessOperationMode();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUIStyle explanationStyle = new GUIStyle(GUI.skin.box);
			explanationStyle.normal.textColor = new Color(0.52f, 0.66f, 0.9f);
			GUILayout.Box(reshapeMountPointModeExplanation[(int)_editor.reshapeMountPointMode], explanationStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();

			switch (_editor.reshapeMountPointMode)
			{
				case RESHAPE_MOUNT_POINT_TOOL.DELETE:
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (_editor.mountPointIndex < 0)
					{
						DrawWarningLabel("Select a mount point to delete.");
					}
					else
					{
						DrawWarningLabel("Confirm delete this mount point?");
						if (GUILayout.Button("Delete", GUILayout.Width(80)))
						{
							UndoPushMountPointStartOperation("Undo Delete Mount Point");
							_editor.DeleteMountPoint();
							UndoPushMountPointEndOperation();
							_editor.SetInfoMsg(INFO_MSG_MOUNT_POINT_DELETED);
							_editor.operationMode = OPERATION_MODE.SELECTION;
						}
						if (GUILayout.Button("Cancel", GUILayout.Width(80)))
						{
							_editor.reshapeMountPointMode = RESHAPE_MOUNT_POINT_TOOL.MOVE;
						}
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();
					break;
			}

			GUILayout.EndVertical();
		}

		void ShowCreateTools()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			CREATE_TOOL prevCTool = _editor.createMode;
			GUIStyle selectionCGridStyle = new GUIStyle(GUI.skin.button);
			int selectionCGridRows = (createToolbarIcons.Length - 1) / 3 + 1;
			selectionCGridStyle.margin = new RectOffset(2, 2, 2, 2);
			_editor.createMode = (CREATE_TOOL)GUILayout.SelectionGrid((int)_editor.createMode, createToolbarIcons, 3, selectionCGridStyle, GUILayout.Height(24 * selectionCGridRows), GUILayout.MaxWidth(310));
			if (_editor.createMode != prevCTool)
			{
				ProcessOperationMode();
				NewShapeInit();
				if (_editor.editingMode == EDITING_MODE.COUNTRIES && (_editor.createMode == CREATE_TOOL.PROVINCE || _editor.createMode == CREATE_TOOL.PROVINCE_REGION))
				{
					ChangeEditingMode(EDITING_MODE.PROVINCES);
				}
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUIStyle explanationCStyle = new GUIStyle(GUI.skin.box);
			explanationCStyle.normal.textColor = new Color(0.52f, 0.66f, 0.9f);
			GUILayout.Box(createModeExplanation[(int)_editor.createMode], explanationCStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			EditorGUILayout.EndHorizontal();
		}

		bool CheckSelectedCountry()
		{
			if (_editor.countryIndex >= 0)
				return true;
			EditorUtility.DisplayDialog("Tool", "Please select a country first.", "Ok");
			return false;
		}

		bool CheckSelectedProvince()
		{
			if (_editor.provinceIndex >= 0)
				return true;
			EditorUtility.DisplayDialog("Tool", "Please select a province first.", "Ok");
			return false;
		}

		#endregion

		#region Processing logic


		// Add a menu item called "Double Mass" to a Rigidbody's context menu.
		[MenuItem("CONTEXT/WorldMap2D_Editor/Restore Backup")]
		static void RestoreBackup(MenuCommand command)
		{
			if (!EditorUtility.DisplayDialog("Restore original geodata files?", "Current geodata files will be replaced by the original files from Backup folder. Any changes will be lost. This operation can't be undone.\n\nRestore files?", "Restore", "Cancel"))
			{
				return;
			}

			// Proceed and restore
			string[] paths = AssetDatabase.GetAllAssetPaths();
			bool backupFolderExists = false;
			string geoDataFolder = "", backupFolder = "";
			for (int k = 0; k < paths.Length; k++)
			{
				if (paths[k].EndsWith("Resources/Geodata"))
				{
					geoDataFolder = paths[k];
				}
				else if (paths[k].EndsWith("WorldPoliticalMap2DEdition/Backup"))
				{
					backupFolder = paths[k];
					backupFolderExists = true;
				}
			}

			WorldMap2D_Editor editor = (WorldMap2D_Editor)command.context;

			if (!backupFolderExists)
			{
				editor.SetInfoMsg(INFO_MSG_BACKUP_NOT_FOUND);
				return;
			}

			// Counterparts
			string fullFileName = backupFolder + "/counterparts.txt";
			if (File.Exists(fullFileName))
			{
				AssetDatabase.DeleteAsset(geoDataFolder + "/counterparts.txt");
				AssetDatabase.SaveAssets();
				AssetDatabase.CopyAsset(fullFileName, geoDataFolder + "/counterparts.txt");
			}
			// Regions
			fullFileName = backupFolder + "/regions.txt";
			if (File.Exists(fullFileName))
			{
				AssetDatabase.DeleteAsset(geoDataFolder + "/regions.txt");
				AssetDatabase.SaveAssets();
				AssetDatabase.CopyAsset(fullFileName, geoDataFolder + "/regions.txt");
			}
			// Zone places
			fullFileName = backupFolder + "/zone_places.txt";
			if (File.Exists(fullFileName))
			{
				AssetDatabase.DeleteAsset(geoDataFolder + "/zone_places.txt");
				AssetDatabase.SaveAssets();
				AssetDatabase.CopyAsset(fullFileName, geoDataFolder + "/zone_places.txt");
			}
			// Landmarks
			fullFileName = backupFolder + "/landmarks.txt";
			if (File.Exists(fullFileName))
			{
				AssetDatabase.DeleteAsset(geoDataFolder + "/landmarks.txt");
				AssetDatabase.SaveAssets();
				AssetDatabase.CopyAsset(fullFileName, geoDataFolder + "/landmarks.txt");
			}

			AssetDatabase.Refresh();

			// Save changes
			editor.SetInfoMsg(INFO_MSG_BACKUP_RESTORED);
			editor.DiscardChanges();
		}

		void SwitchEditingFrontiersFile()
		{
			_editor.DiscardChanges();
		}

		void ProcessOperationMode()
		{

			AdjustCityIconsScale();
			AdjustMountPointIconsScale();

			// Check mouse buttons state and react to possible undo/redo operations
			bool mouseDown = false;
			Event e = Event.current;
			var controlID = GUIUtility.GetControlID(FocusType.Passive);
			if (GUIUtility.hotControl == controlID)
			{   // release hot control to allow standard navigation
				GUIUtility.hotControl = 0;
			}
			// locks control on map
			var eventType = e.GetTypeForControl(controlID);
			if (eventType == EventType.MouseDown && Event.current.button == 0)
			{
				mouseDown = true;
				GUIUtility.hotControl = controlID;
				startedReshapeRegion = false;
				startedReshapeCity = false;
			}
			else if (eventType == EventType.MouseUp && e.button == 0)
			{
				if (undoPushStarted)
				{
					if (startedReshapeRegion)
					{
						UndoPushRegionEndOperation();
					}
					if (startedReshapeCity)
					{
						UndoPushCityEndOperation();
					}
				}
			}

			if (e.type == EventType.ValidateCommand && e.commandName.Equals("UndoRedoPerformed"))
			{
				_editor.UndoHandle();
				EditorUtility.SetDirty(target);
				return;
			}

			switch (_editor.operationMode)
			{
				case OPERATION_MODE.SELECTION:
					// do we click inside a country or province?
					if (Camera.current == null) // can't ray-trace
						return;
					if (mouseDown)
					{
						Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
						bool selected = _editor.CountrySelectByScreenClick(ray);
						if (!selected)
							_editor.ClearSelection();
						if (_editor.editingMode == EDITING_MODE.PROVINCES)
						{
							selected = _editor.ProvinceSelectByScreenClick(ray);
							if (!selected)
								_editor.ClearProvinceSelection();
						}
						if (!_editor.CitySelectByScreenClick(ray))
						{
							_editor.ClearCitySelection();
						}
						if (!_editor.MountPointSelectByScreenClick(ray))
						{
							_editor.ClearMountPointSelection();
						}
						// Reset the cursor if entity selected
						if (selected)
						{
							if (_editor.editingMode == EDITING_MODE.PROVINCES)
								_map.DrawProvinces(_editor.countryIndex, true, false);
							if (_editor.entities != null && _editor.entityIndex >= 0)
								_editor.cursor = _editor.entities[_editor.entityIndex].center;
						}
					}
					else
					{
						if (_editor.editingMode == EDITING_MODE.PROVINCES)
						{
							int targetCountryIndex, targetRegionIndex;
							Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
							if (_map.GetCountryIndex(ray, out targetCountryIndex, out targetRegionIndex))
							{
								_map.DrawProvinces(targetCountryIndex, true, false);
							}
						}
					}

					ShowShapePoints(false);
					ShowCitySelected();
					ShowMountPointSelected();
					break;

				case OPERATION_MODE.RESHAPE:
					// do we move any handle to change frontiers?
					switch (_editor.reshapeRegionMode)
					{
						case RESHAPE_REGION_TOOL.POINT:
						case RESHAPE_REGION_TOOL.CIRCLE:
							ExecuteMoveTool();
							break;
						case RESHAPE_REGION_TOOL.MAGNET:
						case RESHAPE_REGION_TOOL.ERASER:
						case RESHAPE_REGION_TOOL.SMOOTH:
							ExecuteClickTool(e.mousePosition, mouseDown);
							break;
						case RESHAPE_REGION_TOOL.SPLITH:
						case RESHAPE_REGION_TOOL.SPLITV:
						case RESHAPE_REGION_TOOL.DELETE:
							ShowShapePoints(false);
							break;
					}
					switch (_editor.reshapeCityMode)
					{
						case RESHAPE_CITY_TOOL.MOVE:
							ExecuteCityMoveTool();
							break;
					}
					switch (_editor.reshapeMountPointMode)
					{
						case RESHAPE_MOUNT_POINT_TOOL.MOVE:
							ExecuteMountPointMoveTool();
							break;
					}
					break;
				case OPERATION_MODE.CREATE:
					// pressed esc key? then removes last point 
					if (e.type == EventType.KeyDown)
					{
						if (e.keyCode == KeyCode.Escape)
							NewShapeRemoveLastPoint();
					}

					switch (_editor.createMode)
					{
						case CREATE_TOOL.CITY:
							ExecuteCityCreateTool(e.mousePosition, mouseDown);
							break;
						case CREATE_TOOL.COUNTRY:
							ExecuteShapeCreateTool(e.mousePosition, mouseDown);
							break;
						case CREATE_TOOL.COUNTRY_REGION:
						case CREATE_TOOL.PROVINCE:
							if (_editor.countryIndex >= 0 && _editor.countryIndex < _map.countries.Length)
							{
								ExecuteShapeCreateTool(e.mousePosition, mouseDown);
							}
							else
							{
								_editor.SetInfoMsg(INFO_MSG_CHOOSE_COUNTRY);
							}
							break;
						case CREATE_TOOL.PROVINCE_REGION:
							if (_editor.countryIndex <= 0 || _editor.countryIndex >= _map.countries.Length)
							{
								_editor.SetInfoMsg(INFO_MSG_CHOOSE_COUNTRY);
							}
							else if (_editor.provinceIndex < 0 || _editor.provinceIndex >= _map.provinces.Length)
							{
								_editor.SetInfoMsg(INFO_MSG_CHOOSE_PROVINCE);
							}
							else
							{
								ExecuteShapeCreateTool(e.mousePosition, mouseDown);
							}
							break;
						case CREATE_TOOL.MOUNT_POINT:
							ExecuteMountPointCreateTool(e.mousePosition, mouseDown);
							break;
					}
					break;
				case OPERATION_MODE.CONFIRM:
				case OPERATION_MODE.UNDO:
					break;
			}

			if (_editor.editingMode == EDITING_MODE.PROVINCES)
			{
				DrawEditorProvinceNames();
			}
			CheckHideEditorMesh();
		}

		void ExecuteMoveTool()
		{
			if (_editor.entityIndex < 0 || _editor.regionIndex < 0)
				return;
			bool frontiersUnchanged = true;
			if (_editor.entities[_editor.entityIndex].regions == null)
				return;
			Vector3[] points = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex].points;
			Vector3 oldPoint, newPoint, sourcePosition = Misc.Vector3zero, displacement = Misc.Vector3zero, newCoor = Misc.Vector3zero;
			Transform mapTransform = _map.transform;
			if (controlIds == null || controlIds.Length < points.Length)
				controlIds = new int[points.Length];

			bool onePointSelected = false;
			Vector3 selectedPoint = Misc.Vector3zero;
			for (int i = 0; i < points.Length; i++)
			{
				oldPoint = mapTransform.TransformPoint(points[i]);
				float handleSize = HandleUtility.GetHandleSize(oldPoint) * HANDLE_SIZE;
#if UNITY_5_6_OR_NEWER
				newPoint = Handles.FreeMoveHandle (oldPoint, mapTransform.rotation, handleSize, pointSnap,   
				                                   (handleControlID, position, rotation, size, eventType) =>
				{
					controlIds [i] = handleControlID;
					Handles.DotHandleCap (handleControlID, position, rotation, size, eventType);
				});
#else
				newPoint = Handles.FreeMoveHandle(oldPoint, mapTransform.rotation, handleSize, pointSnap,
								(handleControlID, position, rotation, size) =>
								{
									controlIds[i] = handleControlID;
									Handles.DotCap(handleControlID, position, rotation, size);
								});
#endif

				if (!onePointSelected && GUIUtility.keyboardControl == controlIds[i] && GUIUtility.keyboardControl != 0)
				{
					onePointSelected = true;
					selectedPoint = oldPoint;
				}
				if (frontiersUnchanged && oldPoint != newPoint)
				{
					frontiersUnchanged = false;
					newCoor = mapTransform.InverseTransformPoint(newPoint);
					sourcePosition = points[i];
					displacement = new Vector2(newCoor.x - points[i].x, newCoor.y - points[i].y);
				}
			}
			if (_editor.reshapeRegionMode.hasCircle())
			{
				if (!onePointSelected)
				{
					selectedPoint = mapTransform.TransformPoint(points[0]);
				}
				float size = _editor.reshapeCircleWidth * mapTransform.localScale.y;
#if UNITY_5_6_OR_NEWER
				Handles.CircleHandleCap (0, selectedPoint, mapTransform.rotation, size, EventType.Repaint);
#else
				Handles.CircleCap(0, selectedPoint, mapTransform.rotation, size);
#endif
			}

			if (!frontiersUnchanged)
			{
				List<Region> affectedRegions = null;
				switch (_editor.reshapeRegionMode)
				{
					case RESHAPE_REGION_TOOL.POINT:
						if (!startedReshapeRegion)
							UndoPushRegionStartOperation("Undo Point Move");
						affectedRegions = _editor.MovePoint(sourcePosition, displacement);
						break;
					case RESHAPE_REGION_TOOL.CIRCLE:
						if (!startedReshapeRegion)
							UndoPushRegionStartOperation("Undo Group Move");
						affectedRegions = _editor.MoveCircle(sourcePosition, displacement, _editor.reshapeCircleWidth);
						break;
				}
				_editor.RedrawFrontiers(affectedRegions, false);
				HandleUtility.Repaint();
			}
		}

		void ExecuteClickTool(Vector2 mousePosition, bool clicked)
		{
			if (_editor.entityIndex < 0 || _editor.entityIndex >= _editor.entities.Length)
				return;

			// Show the mouse cursor
			if (Camera.current == null)
				return;

			// Show the points
			ShowShapePoints(_editor.reshapeRegionMode != RESHAPE_REGION_TOOL.SMOOTH);
			Transform mapTransform = _map.transform;

			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			int layerMask = 1 << _map.gameObject.layer; // MAP BELONGS TO LAYER UI
			RaycastHit[] hits = Physics.RaycastAll(ray, 500, layerMask);
			if (hits.Length > 0)
			{
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].collider.gameObject == _map.gameObject)
					{
						Vector3 cursorPos = hits[k].point;
						_editor.cursor = mapTransform.InverseTransformPoint(cursorPos);
						_editor.cursor.z = 0;
						if (_editor.reshapeRegionMode == RESHAPE_REGION_TOOL.SMOOTH)
						{
							ShowCandidatePoint();
						}
						else
						{
							// Show circle cursor
							float seconds = (float)new TimeSpan(DateTime.Now.Ticks - tickStart).TotalSeconds;
							seconds *= 4.0f;
							float t = seconds % 2;
							if (t >= 1)
								t = 2 - t;
							float effect = Mathf.SmoothStep(0, 1, t) / 10.0f;
							float size = _editor.reshapeCircleWidth * mapTransform.localScale.y * (0.9f + effect);
#if UNITY_5_6_OR_NEWER
							Handles.CircleHandleCap (0, cursorPos, mapTransform.rotation, size, EventType.Repaint);
#else
							Handles.CircleCap(0, cursorPos, mapTransform.rotation, size);
#endif
						}

						if (clicked)
						{
							switch (_editor.reshapeRegionMode)
							{
								case RESHAPE_REGION_TOOL.MAGNET:
									if (!startedReshapeRegion)
										UndoPushRegionStartOperation("Undo Magnet");
									_editor.Magnet(_editor.cursor, _editor.reshapeCircleWidth);
									break;
								case RESHAPE_REGION_TOOL.ERASER:
									if (!startedReshapeRegion)
										UndoPushRegionStartOperation("Undo Eraser");
									_editor.Erase(_editor.cursor, _editor.reshapeCircleWidth);
									break;
								case RESHAPE_REGION_TOOL.SMOOTH:
									if (!startedReshapeRegion)
										UndoPushRegionStartOperation("Undo Smooth");
									_editor.AddPoint(_editor.cursor); // Addpoint manages the refresh
									break;
							}
						}
						HandleUtility.Repaint();
					}
				}
			}
		}

		void ExecuteCityCreateTool(Vector2 mousePosition, bool clicked)
		{

			// Show the mouse cursor
			if (Camera.current == null)
				return;

			// Show the points
			Transform mapTransform = _map.transform;

			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			int layerMask = 1 << _map.gameObject.layer; // MAP BELONGS TO LAYER UI
			RaycastHit[] hits = Physics.RaycastAll(ray, 500, layerMask);
			if (hits.Length > 0)
			{
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].collider.gameObject == _map.gameObject)
					{
						Vector3 cursorPos = hits[k].point;
						_editor.cursor = mapTransform.InverseTransformPoint(cursorPos);
						_editor.cursor.z = 0;

						Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
						Vector3 pt = mapTransform.TransformPoint(_editor.cursor);
						float handleSize = HandleUtility.GetHandleSize(pt) * HANDLE_SIZE * 4.0f;
#if UNITY_5_6_OR_NEWER
						Handles.SphereHandleCap (0, pt, mapTransform.rotation, handleSize, EventType.Repaint);
#else
						Handles.SphereCap(0, pt, mapTransform.rotation, handleSize);
#endif
						Handles.color = Color.white;

						if (clicked)
						{
							if (_editor.countryIndex < 0 || _editor.countryIndex >= _map.countries.Length)
							{
								EditorUtility.DisplayDialog("Add new city", "Please choose a country first.", "Ok");
								return;
							}
							UndoPushCityStartOperation("Undo Create City");
							_editor.CityCreate(_editor.cursor);
							UndoPushCityEndOperation();
						}
					}
					HandleUtility.Repaint();
				}
			}
		}

		void AdjustCityIconsScale()
		{
			// Adjust city icons in scene view
			if (_map == null || _map.cities == null)
				return;

			Transform t = _map.transform.Find("Cities");
			if (t != null)
			{
				CityScaler scaler = t.GetComponent<CityScaler>();
				scaler.ScaleCities(0.1f);
			}
			else
			{
				// This should not happen but maybe the user deleted the layer. Forces refresh.
				_map.showCities = true;
				_map.DrawCities();
			}
		}

		void ShowCitySelected()
		{
			if (_editor.cityIndex < 0 || _editor.cityIndex >= _map.cities.Count)
				return;
			Vector3 cityPos = _map.cities[_editor.cityIndex].unity2DLocation;
			Vector3 worldPos = _map.transform.TransformPoint(cityPos);
			float handleSize = HandleUtility.GetHandleSize(worldPos) * HANDLE_SIZE * 2.0f;
#if UNITY_5_6_OR_NEWER
			Handles.RectangleHandleCap(0, worldPos, _map.transform.rotation, handleSize, EventType.Repaint);
#else
			Handles.RectangleCap(0, worldPos, _map.transform.rotation, handleSize);
#endif
		}

		void ExecuteCityMoveTool()
		{
			if (_editor.cityIndex < 0 || _editor.cityIndex >= _map.cities.Count)
				return;

			Transform mapTransform = _map.transform;
			Vector3 cityPos = _map.cities[_editor.cityIndex].unity2DLocation;
			Vector3 oldPoint = mapTransform.TransformPoint(cityPos);
			float handleSize = HANDLE_SIZE * 1.2f;
#if UNITY_5_6_OR_NEWER
			Vector3 newPoint = Handles.FreeMoveHandle (oldPoint, mapTransform.rotation, handleSize, pointSnap,   
			                                           (handleControlID, position, rotation, size, eventType) =>
			                                           {
				Handles.RectangleHandleCap (handleControlID, position, rotation, size, eventType);
			});
#else
			Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, mapTransform.rotation, handleSize, pointSnap,
											   (handleControlID, position, rotation, size) =>
											   {
												   Handles.RectangleCap(handleControlID, position, rotation, size);
											   });
#endif
			if (newPoint != oldPoint)
			{
				newPoint = mapTransform.InverseTransformPoint(newPoint);
				newPoint.z = 0;
				if (!startedReshapeCity)
					UndoPushCityStartOperation("Undo City Move");
				_editor.CityMove(newPoint);
				HandleUtility.Repaint();
			}
		}

		void AdjustMountPointIconsScale()
		{
			// Adjust city icons in scene view
			if (_map == null || _map.mountPoints == null)
				return;

			Transform t = _map.transform.Find("Mount Points");
			if (t != null)
			{
				MountPointScaler scaler = t.GetComponent<MountPointScaler>();
				scaler.ScaleMountPoints(0.1f);
			}
			else
			{
				// This should not happen but maybe the user deleted the layer. Forces refresh.
				_map.DrawMountPoints();
			}
		}

		void ExecuteMountPointCreateTool(Vector2 mousePosition, bool clicked)
		{

			// Show the mouse cursor
			if (Camera.current == null)
				return;

			// Show the points
			Transform mapTransform = _map.transform;

			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			int layerMask = 1 << _map.gameObject.layer; // MAP BELONGS TO LAYER UI
			RaycastHit[] hits = Physics.RaycastAll(ray, 5000, layerMask);
			if (hits.Length > 0)
			{
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].collider.gameObject == _map.gameObject)
					{
						Vector3 cursorPos = hits[k].point;
						_editor.cursor = mapTransform.InverseTransformPoint(cursorPos);

						Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
						float handleSize = HandleUtility.GetHandleSize(cursorPos) * HANDLE_SIZE * 4.0f;
#if UNITY_5_6_OR_NEWER
						Handles.SphereHandleCap (0, cursorPos, mapTransform.rotation, handleSize, EventType.Repaint);
#else
						Handles.SphereCap(0, cursorPos, mapTransform.rotation, handleSize);
#endif
						Handles.color = Color.white;

						if (clicked)
						{
							if (_editor.countryIndex < 0 || _editor.countryIndex >= _map.countries.Length)
							{
								EditorUtility.DisplayDialog("Add new city", "Please choose a country first.", "Ok");
								return;
							}
							UndoPushMountPointStartOperation("Undo Create Mount Point");
							_editor.MountPointCreate(_editor.cursor);
							UndoPushMountPointEndOperation();
						}
					}
					HandleUtility.Repaint();
				}
			}
		}

		void ShowMountPointSelected()
		{
			if (_editor.mountPointIndex < 0 || _editor.mountPointIndex >= _map.mountPoints.Count)
				return;
			Vector3 mountPointPos = _map.mountPoints[_editor.mountPointIndex].unity2DLocation;
			Vector3 worldPos = _map.transform.TransformPoint(mountPointPos);
			float handleSize = HandleUtility.GetHandleSize(worldPos) * HANDLE_SIZE * 2.0f;
#if UNITY_5_6_OR_NEWER
			Handles.RectangleHandleCap(0, worldPos, _map.transform.rotation, handleSize, EventType.Repaint);
#else
			Handles.RectangleCap(0, worldPos, _map.transform.rotation, handleSize);
#endif
		}

		void ExecuteMountPointMoveTool()
		{
			if (_map.mountPoints == null || _editor.mountPointIndex < 0 || _editor.mountPointIndex >= _map.mountPoints.Count)
				return;

			Transform mapTransform = _map.transform;
			Vector3 mountPointPos = _map.mountPoints[_editor.mountPointIndex].unity2DLocation;
			Vector3 oldPoint = mapTransform.TransformPoint(mountPointPos);
			float handleSize = HandleUtility.GetHandleSize(oldPoint) * HANDLE_SIZE * 2.0f;
#if UNITY_5_6_OR_NEWER
			Vector3 newPoint = Handles.FreeMoveHandle (oldPoint, mapTransform.rotation, handleSize, pointSnap,   
			                                           (handleControlID, position, rotation, size, eventType) =>
			                                           {
				Handles.RectangleHandleCap (handleControlID, position, rotation, size, eventType);
			});
#else
			Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, mapTransform.rotation, handleSize, pointSnap,
											   (handleControlID, position, rotation, size) =>
											   {
												   Handles.RectangleCap(handleControlID, position, rotation, size);
											   });
#endif
			if (newPoint != oldPoint)
			{
				newPoint = mapTransform.InverseTransformPoint(newPoint);
				if (!startedReshapeMountPoint)
					UndoPushMountPointStartOperation("Undo Mount Point Move");
				_editor.MountPointMove(newPoint);
				HandleUtility.Repaint();
			}
		}

		void UndoPushRegionStartOperation(string operationName)
		{
			startedReshapeRegion = !startedReshapeRegion;
			undoPushStarted = true;
			Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
			_editor.UndoRegionsPush(_editor.highlightedRegions);

		}

		void UndoPushRegionEndOperation()
		{
			undoPushStarted = false;
			_editor.UndoRegionsInsertAtCurrentPos(_editor.highlightedRegions);
			if (_editor.reshapeRegionMode != RESHAPE_REGION_TOOL.SMOOTH)
			{ // Smooth operation doesn't need to refresh labels nor frontiers
				_map.RedrawMapLabels();
				bool refreshAllFrontiers = _editor.reshapeRegionMode != RESHAPE_REGION_TOOL.CIRCLE && _editor.reshapeRegionMode != RESHAPE_REGION_TOOL.POINT;
				if (refreshAllFrontiers)
				{
					_editor.RedrawFrontiers(null, true);
				}
				else
				{
					_editor.RedrawFrontiers();
				}
			}
		}

		void UndoPushCityStartOperation(string operationName)
		{
			startedReshapeCity = !startedReshapeCity;
			undoPushStarted = true;
			Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
			_editor.UndoCitiesPush();
		}

		void UndoPushCityEndOperation()
		{
			undoPushStarted = false;
			_editor.UndoCitiesInsertAtCurrentPos();
		}

		void UndoPushMountPointStartOperation(string operationName)
		{
			startedReshapeMountPoint = !startedReshapeMountPoint;
			undoPushStarted = true;
			Undo.RecordObject(target, operationName);   // record changes to the undo dummy flag
			_editor.UndoMountPointsPush();
		}

		void UndoPushMountPointEndOperation()
		{
			undoPushStarted = false;
			_editor.UndoMountPointsInsertAtCurrentPos();
		}

		static void CheckBackup(out string geoDataFolder)
		{
			string[] paths = AssetDatabase.GetAllAssetPaths();
			bool backupFolderExists = false;
			string rootFolder = "";
			geoDataFolder = "";
			for (int k = 0; k < paths.Length; k++)
			{
				if (paths[k].EndsWith("Resources/Geodata"))
				{
					geoDataFolder = paths[k];
				}
				else if (paths[k].EndsWith("WorldPoliticalMap2DEdition"))
				{
					rootFolder = paths[k];
				}
				else if (paths[k].EndsWith("WorldPoliticalMap2DEdition/Backup"))
				{
					backupFolderExists = true;
				}
			}

			if (!backupFolderExists)
			{
				// Do the backup
				AssetDatabase.CreateFolder(rootFolder, "Backup");
				string backupFolder = rootFolder + "/Backup";
				string fullFileName = geoDataFolder + "/countries110.txt";
				if (File.Exists(fullFileName))
				{
					AssetDatabase.CopyAsset(fullFileName, backupFolder + "/countries110.txt");
				}
				fullFileName = geoDataFolder + "/countries10.txt";
				if (File.Exists(fullFileName))
				{
					AssetDatabase.CopyAsset(fullFileName, backupFolder + "/countries10.txt");
				}
				fullFileName = geoDataFolder + "/provinces10.txt";
				if (File.Exists(fullFileName))
				{
					AssetDatabase.CopyAsset(fullFileName, backupFolder + "/provinces10.txt");
				}
				fullFileName = geoDataFolder + "/cities10.txt";
				if (File.Exists(fullFileName))
				{
					AssetDatabase.CopyAsset(fullFileName, backupFolder + "/cities10.txt");
				}
				fullFileName = geoDataFolder + "/mountPoints.txt";
				if (File.Exists(fullFileName))
				{
					AssetDatabase.CopyAsset(fullFileName, backupFolder + "/mountPoints.txt");
				}
			}
		}

		string GetAssetsFolder()
		{
			string fullPathName = Application.dataPath;
			int pos = fullPathName.LastIndexOf("/Assets");
			if (pos > 0)
				fullPathName = fullPathName.Substring(0, pos + 1);
			return fullPathName;
		}

		bool SaveChanges()
		{

			if (!_editor.countryChanges && !_editor.provinceChanges && !_editor.cityChanges && !_editor.mountPointChanges)
				return false;

			// First we make a backup if it doesn't exist
			string geoDataFolder;
			CheckBackup(out geoDataFolder);

			string dataFileName, fullPathName;
			// Save changes to countries
			if (_editor.countryChanges)
			{
				dataFileName = _editor.GetCountryGeoDataFileName();
				fullPathName = GetAssetsFolder() + geoDataFolder + "/" + dataFileName;
				string data = _editor.GetCountryGeoData();
				File.WriteAllText(fullPathName, data, System.Text.Encoding.UTF8);
				_editor.countryChanges = false;
			}
			// Save changes to provinces
			if (_editor.provinceChanges)
			{
				dataFileName = _editor.GetProvinceGeoDataFileName();
				fullPathName = GetAssetsFolder();
				string fullAssetPathName = fullPathName + geoDataFolder + "/" + dataFileName;
				string data = _editor.GetProvinceGeoData();
				File.WriteAllText(fullAssetPathName, data, System.Text.Encoding.UTF8);
				_editor.provinceChanges = false;
			}
			// Save changes to cities
			if (_editor.cityChanges)
			{
				dataFileName = _editor.GetCityGeoDataFileName();
				fullPathName = GetAssetsFolder() + geoDataFolder + "/" + dataFileName;
				File.WriteAllText(fullPathName, _editor.GetCityGeoData(), System.Text.Encoding.UTF8);
				_editor.cityChanges = false;
			}
			// Save changes to mount points
			if (_editor.mountPointChanges)
			{
				dataFileName = _editor.GetMountPointGeoDataFileName();
				fullPathName = GetAssetsFolder() + geoDataFolder + "/" + dataFileName;
				File.WriteAllText(fullPathName, _editor.GetMountPointsGeoData(), System.Text.Encoding.UTF8);
				_editor.mountPointChanges = false;
			}
			AssetDatabase.Refresh();
			return true;
		}


		#endregion

		#region Editor UI handling

		void CheckHideEditorMesh()
		{
			if (!_editor.shouldHideEditorMesh)
				return;
			_editor.shouldHideEditorMesh = false;
			Transform s = _map.transform.Find("Surfaces");
			if (s == null)
				return;
			Renderer[] rr = s.GetComponentsInChildren<Renderer>(true);
			for (int k = 0; k < rr.Length; k++)
			{
#if UNITY_5_5_OR_NEWER
																EditorUtility.SetSelectedRenderState (rr [k], EditorSelectedRenderState.Hidden);
#else
				EditorUtility.SetSelectedWireframeHidden(rr[k], true);
#endif
			}
		}

		void ShowShapePoints(bool highlightInsideCircle)
		{
			if (_map.countries == null)
				return;
			if (_editor.entityIndex >= 0 && _editor.entities != null && _editor.entityIndex < _editor.entities.Length && _editor.regionIndex >= 0)
			{
				if (_editor.entities[_editor.entityIndex].regions == null || _editor.entities[_editor.entityIndex].regions.Count <= _editor.regionIndex)
					return;
				Region region = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex];
				Transform mapTransform = _map.transform;
				float circleSizeSqr = _editor.reshapeCircleWidth * _editor.reshapeCircleWidth;
				for (int i = 0; i < region.points.Length; i++)
				{
					Vector3 rp = region.points[i];
					Vector3 p = mapTransform.TransformPoint(rp);
					float handleSize = HandleUtility.GetHandleSize(p) * HANDLE_SIZE;
					if (highlightInsideCircle)
					{
						float dist = (rp.x - _editor.cursor.x) * (rp.x - _editor.cursor.x) * 4.0f + (rp.y - _editor.cursor.y) * (rp.y - _editor.cursor.y);
						if (dist < circleSizeSqr)
						{
							Handles.color = Color.green;
#if UNITY_5_6_OR_NEWER
							Handles.DotHandleCap (0, p, mapTransform.rotation, handleSize, EventType.Repaint);
#else
							Handles.DotCap(0, p, mapTransform.rotation, handleSize);
#endif
							continue;
						}
						else
						{
							Handles.color = Color.white;
						}
					}
#if UNITY_5_6_OR_NEWER
					Handles.RectangleHandleCap (0, p, mapTransform.rotation, handleSize, EventType.Repaint);
#else
					Handles.RectangleCap(0, p, mapTransform.rotation, handleSize);
#endif
				}
			}
			Handles.color = Color.white;
		}

		/// <summary>
		/// Shows a potential new point near from cursor location (point parameter, which is in local coordinates)
		/// </summary>
		void ShowCandidatePoint()
		{
			if (_editor.entityIndex < 0 || _editor.regionIndex < 0 || _editor.entities[_editor.entityIndex].regions == null)
				return;
			Region region = _editor.entities[_editor.entityIndex].regions[_editor.regionIndex];
			int max = region.points.Length;
			float minDist = float.MaxValue;
			int nearest = -1, previous = 0;
			for (int p = 0; p < max; p++)
			{
				int q = p == 0 ? max - 1 : p - 1;
				Vector3 rp = (region.points[p] + region.points[q]) * 0.5f;
				float dist = (rp.x - _editor.cursor.x) * (rp.x - _editor.cursor.x) * 4 + (rp.y - _editor.cursor.y) * (rp.y - _editor.cursor.y);
				if (dist < minDist)
				{
					// Get nearest point
					minDist = dist;
					nearest = p;
					previous = q;
				}
			}

			if (nearest >= 0)
			{
				Transform mapTransform = _map.transform;
				Vector3 pointToInsert = (region.points[nearest] + region.points[previous]) * 0.5f;
				Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
				Vector3 pt = mapTransform.TransformPoint(pointToInsert);
				float handleSize = HandleUtility.GetHandleSize(pt) * HANDLE_SIZE;
#if UNITY_5_6_OR_NEWER
				Handles.DotHandleCap (0, pt, mapTransform.rotation, handleSize, EventType.Repaint);
#else
				Handles.DotCap(0, pt, mapTransform.rotation, handleSize);
#endif
				Handles.color = Color.white;
			}
		}

		void NewShapeInit()
		{
			if (_editor.newShape == null)
				_editor.newShape = new List<Vector3>();
			else
				_editor.newShape.Clear();
		}

		void NewShapeRemoveLastPoint()
		{
			if (_editor.newShape != null && _editor.newShape.Count > 0)
				_editor.newShape.RemoveAt(_editor.newShape.Count - 1);
		}


		/// <summary>
		/// Returns any city near the point specified in local coordinates.
		/// </summary>
		int NewShapeGetIndexNearPoint(Vector3 localPoint)
		{
			float rl = localPoint.x - HIT_PRECISION;
			float rr = localPoint.x + HIT_PRECISION;
			float rt = localPoint.y + HIT_PRECISION;
			float rb = localPoint.y - HIT_PRECISION;
			for (int c = 0; c < _editor.newShape.Count; c++)
			{
				Vector3 cityLoc = _editor.newShape[c];
				if (cityLoc.x > rl && cityLoc.x < rr && cityLoc.y > rb && cityLoc.y < rt)
				{
					return c;
				}
			}
			return -1;
		}

		/// <summary>
		/// Shows a potential point to be added to the new shape and draws current shape polygon
		/// </summary>
		void ExecuteShapeCreateTool(Vector3 mousePosition, bool mouseDown)
		{
			// Show the mouse cursor
			if (Camera.current == null)
				return;

			// Show the points
			Transform mapTransform = _map.transform;

			int numPoints = _editor.newShape.Count;
			Vector3[] shapePoints = new Vector3[numPoints + 1];
			for (int k = 0; k < numPoints; k++)
			{
				shapePoints[k] = mapTransform.TransformPoint(_editor.newShape[k]);
			}
			shapePoints[numPoints] = mapTransform.TransformPoint(_editor.cursor);

			// Draw shape polygon in same color as corresponding frontiers
			if (numPoints >= 1)
			{
				if (_editor.createMode == CREATE_TOOL.COUNTRY || _editor.createMode == CREATE_TOOL.COUNTRY_REGION)
				{
					Handles.color = _map.frontiersColor;
				}
				else
				{
					Handles.color = _map.provincesColor;
				}
				Handles.DrawPolyLine(shapePoints);
				Handles.color = Color.white;
			}

			// Draw handles
			for (int i = 0; i < shapePoints.Length - 1; i++)
			{
				float handleSize = HandleUtility.GetHandleSize(shapePoints[i]) * HANDLE_SIZE;
#if UNITY_5_6_OR_NEWER
				Handles.RectangleHandleCap (0, shapePoints[i], mapTransform.rotation, handleSize, EventType.Repaint);
#else
				Handles.RectangleCap(0, shapePoints[i], mapTransform.rotation, handleSize);
#endif
			}

			// Draw cursor
			if (editorCaptionLabelStyle == null)
			{
				editorCaptionLabelStyle = new GUIStyle();
				editorCaptionLabelStyle.normal.textColor = Color.white;
			}

			// Show tooltip and handle hotkeys
			if (Camera.current != null)
			{
				Vector3 labelPos = Camera.current.ScreenToWorldPoint(new Vector3(10, 20, 1f));
				Handles.Label(labelPos, "Hotkeys: Shift+C = Close polygon (requires +5 vertices, currently: " + numPoints + "), Shift+X = Remove last point, Shift+S: Snap to nearest vertex, Esc = Clear All", editorCaptionLabelStyle);
			}

			bool snapRequested = false;
			if (Event.current != null && Event.current.type == EventType.KeyDown)
			{
				// Shift + X: remove last point
				if (numPoints > 0 && Event.current.shift && Event.current.keyCode == KeyCode.X)
				{
					_editor.newShape.RemoveAt(numPoints - 1);
					Event.current.Use();
					// Escape: remove all points
				}
				else if (Event.current.keyCode == KeyCode.Escape)
				{
					_editor.newShape.Clear();
					Event.current.Use();
				}
				else if (Event.current.keyCode == KeyCode.S)
				{
					snapRequested = true;
					Event.current.Use();
				}
			}

			// Draw handles
			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			int layerMask = 1 << _map.gameObject.layer; // MAP BELONGS TO LAYER UI
			RaycastHit[] hits = Physics.RaycastAll(ray, 500, layerMask);
			bool canClosePolygon = false;
			if (hits.Length > 0)
			{
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].collider.gameObject == _map.gameObject)
					{
						Vector3 cursorPos = hits[k].point;
						Vector3 newPos = mapTransform.InverseTransformPoint(cursorPos);

						// Shift + S: create a new vertex next to another near existing vertex
						if (snapRequested)
						{
							Vector2 nearPos;
							if (_editor.GetVertexNearSpherePos(newPos, out nearPos))
							{
								newPos = nearPos;
								mouseDown = true;
							}
						}

						newPos.z = 0;
						_editor.cursor = newPos;
						if (numPoints > 2)
						{ // Check if we're over the first point
							int i = NewShapeGetIndexNearPoint(newPos);
							if (i == 0)
							{
								Vector3 labelPos;
								if (Camera.current != null)
								{
									Vector3 screenPos = Camera.current.WorldToScreenPoint(cursorPos);
									labelPos = Camera.current.ScreenToWorldPoint(screenPos + Vector3.up * 20f + Vector3.right * 12f);
								}
								else
								{
									labelPos = cursorPos + Vector3.up * 0.17f;
								}
								if (numPoints > 5)
								{
									canClosePolygon = true;
									Handles.Label(labelPos, "Click to close polygon", editorCaptionLabelStyle);
								}
								else
								{
									Handles.Label(labelPos, "Add " + (6 - numPoints) + " more point(s)", editorCaptionLabelStyle);
								}
							}
						}
						Handles.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
						Vector3 pt = mapTransform.TransformPoint(_editor.cursor);
						float handleSize = HandleUtility.GetHandleSize(pt) * HANDLE_SIZE;
#if UNITY_5_6_OR_NEWER
						Handles.DotHandleCap (0, pt, mapTransform.rotation, handleSize, EventType.Repaint);
#else
						Handles.DotCap(0, pt, mapTransform.rotation, handleSize);
#endif
						Handles.color = Color.white;

						// Hotkey for closing polygon (Control + C)
						if (numPoints > 4 && (Event.current != null && Event.current.shift && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.C))
						{
							mouseDown = true;
							canClosePolygon = true;
							Event.current.Use();
						}

						if (mouseDown)
						{
							if (canClosePolygon)
							{
								switch (_editor.createMode)
								{
									case CREATE_TOOL.COUNTRY:
										_editor.CountryCreate();
										break;
									case CREATE_TOOL.COUNTRY_REGION:
										_editor.CountryRegionCreate();
										break;
									case CREATE_TOOL.PROVINCE:
										_editor.ProvinceCreate();
										break;
									case CREATE_TOOL.PROVINCE_REGION:
										_editor.ProvinceRegionCreate();
										break;
								}
								NewShapeInit();
							}
							else
							{
								_editor.newShape.Add(_editor.cursor);
								break;
							}
						}
						HandleUtility.Repaint();
						break;
					}
				}
			}
		}


		void DrawWarningLabel(string s)
		{
			if (warningLabelStyle == null)
				warningLabelStyle = new GUIStyle(GUI.skin.label);
			warningLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.26f, 0.5f);
			GUILayout.Label(s, warningLabelStyle);
		}

		void DrawEditorProvinceNames()
		{
			if (_editor.highlightedRegions == null)
				return;
			Transform mapTransform = _map.transform;
			for (int p = 0; p < _editor.highlightedRegions.Count; p++)
			{
				Region region = _editor.highlightedRegions[p];
				if (region.regionIndex == region.entity.mainRegionIndex)
				{
					Vector3 regionCenter = mapTransform.TransformPoint(region.center);
					Handles.Label(regionCenter, region.entity.name, labelsStyle);
				}
			}
		}

		void CheckScale()
		{
			if (EditorPrefs.HasKey(EDITORPREF_SCALE_WARNED))
				return;
			EditorPrefs.SetBool(EDITORPREF_SCALE_WARNED, true);
		}

		#endregion

	}

}