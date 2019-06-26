using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

//using WPMF;

namespace WPMF {

				public enum OPERATION_MODE {
								SELECTION = 0,
								RESHAPE = 1,
								CREATE = 2,
								UNDO = 3,
								CONFIRM = 4
				}

				public enum RESHAPE_REGION_TOOL {
								POINT = 0,
								CIRCLE = 1,
								SPLITV = 2,
								SPLITH = 3,
								MAGNET = 4,
								SMOOTH = 5,
								ERASER = 6,
								DELETE = 7
				}

				public enum RESHAPE_CITY_TOOL {
								MOVE = 0,
								DELETE = 1
				}

				public enum RESHAPE_MOUNT_POINT_TOOL {
								MOVE = 0,
								DELETE = 1
				}

				public enum CREATE_TOOL {
								CITY = 0,
								COUNTRY = 1,
								COUNTRY_REGION = 2,
								PROVINCE = 3,
								PROVINCE_REGION = 4,
								MOUNT_POINT = 5
				}


				public enum EDITING_MODE {
								COUNTRIES,
								PROVINCES
				}

				public enum EDITING_COUNTRY_FILE {
								COUNTRY_HIGHDEF = 0,
								COUNTRY_LOWDEF = 1
				}

				public static class ReshapeToolExtensons {
								public static bool hasCircle (this RESHAPE_REGION_TOOL r) {
												return r == RESHAPE_REGION_TOOL.CIRCLE || r == RESHAPE_REGION_TOOL.MAGNET || r == RESHAPE_REGION_TOOL.ERASER;
								}
				}

				[RequireComponent (typeof(WorldMap2D))]
				[ExecuteInEditMode]
				public partial class WorldMap2D_Editor : MonoBehaviour {

								public int entityIndex {
												get {
																if (editingMode == EDITING_MODE.PROVINCES)
																				return provinceIndex;
																else
																				return countryIndex;
												}
								}

								public int regionIndex {
												get {
																if (editingMode == EDITING_MODE.PROVINCES)
																				return provinceRegionIndex;
																else
																				return countryRegionIndex;
												}
								}

								public OPERATION_MODE operationMode;
								public RESHAPE_REGION_TOOL reshapeRegionMode;
								public RESHAPE_CITY_TOOL reshapeCityMode;
								public RESHAPE_MOUNT_POINT_TOOL reshapeMountPointMode;
								public CREATE_TOOL createMode;
								public Vector3 cursor;
								public bool circleMoveConstant, circleCurrentRegionOnly;
								public float reshapeCircleWidth = 0.01f;
								public bool shouldHideEditorMesh;
								public bool magnetAgressiveMode = false;
								public bool keepNearbyFrontiers = false;

								public string infoMsg = "";
								public DateTime infoMsgStartTime;
								public EDITING_MODE editingMode;
								public EDITING_COUNTRY_FILE editingCountryFile;

								[NonSerialized]
								public List<Region> highlightedRegions;

								List<List<Region>> _undoRegionsList;

								List<List<Region>> undoRegionsList {
												get {
																if (_undoRegionsList == null)
																				_undoRegionsList = new List<List<Region>> ();
																return _undoRegionsList;
												}
								}

								public int undoRegionsDummyFlag;

								List<List<City>> _undoCitiesList;

								List<List<City>> undoCitiesList {
												get {
																if (_undoCitiesList == null)
																				_undoCitiesList = new List<List<City>> ();
																return _undoCitiesList;
												}
								}

								public int undoCitiesDummyFlag;

								List<List<MountPoint>> _undoMountPointsList;

								List<List<MountPoint>> undoMountPointsList {
												get {
																if (_undoMountPointsList == null)
																				_undoMountPointsList = new List<List<MountPoint>> ();
																return _undoMountPointsList;
												}
								}

								public int undoMountPointsDummyFlag;

								public IAdminEntity[] entities {
												get {
																if (editingMode == EDITING_MODE.PROVINCES)
																				return map.provinces;
																else
																				return map.countries;
												}
								}

								public List<Vector3> newShape;
								WorldMap2D _map;

								[SerializeField]
								int lastMinPopulation;
								List<Region> nearbyRegions;


								void OnEnable () {
												lastMinPopulation = map.minPopulation;
												map.minPopulation = 0;
								}

								void OnDisable () {
												if (_map != null) {
																if (_map.minPopulation == 0)
																				_map.minPopulation = lastMinPopulation;
												}
								}

								#region Editor functionality


								/// <summary>
								/// Accesor to the World Map Globe core API
								/// </summary>
								public WorldMap2D map {
												get {
																if (_map == null)
																				_map = GetComponent<WorldMap2D> ();
																return _map;
												}
								}

								public void ClearSelection () {
												map.HideCountryRegionHighlights (true);
												highlightedRegions = null;
												countryIndex = -1;
												countryRegionIndex = -1;
												GUICountryName = "";
												GUICountryNewName = "";
												GUICountryIndex = -1;
												_GUICountryHidden = false;
												ClearProvinceSelection ();
												ClearCitySelection ();
												ClearMountPointSelection ();
								}

								/// <summary>
								/// Removes special characters from string.
								/// </summary>
								string DataEscape (string s) {
												s = s.Replace ("$", "");
												s = s.Replace ("|", "");
												return s;
								}

								/// <summary>
								/// Redraws all frontiers and highlights current selected regions.
								/// </summary>
								public void RedrawFrontiers () {
												RedrawFrontiers (highlightedRegions, true);
								}

								/// <summary>
								/// Redraws the frontiers and highlights specified regions filtered by provided list of regions. Also highlights current selected country/province.
								/// </summary>
								/// <param name="filterRegions">Regions.</param>
								/// <param name="highlightSelected">Pass false to just redraw borders.</param>
								public void RedrawFrontiers (List <Region> filterRegions, bool highlightSelected) {
												if (filterRegions != null && keepNearbyFrontiers) {
																// Draw frontiers for all nearby regions
																if (nearbyRegions == null)
																				nearbyRegions = new List<Region> (250);
																else
																				nearbyRegions.Clear ();
																Rect rect = _map.countries [countryIndex].regions [regionIndex].rect2D;
																rect.xMin -= 0.05f;
																rect.yMin -= 0.05f;
																rect.xMax += 0.05f;
																rect.yMax += 0.05f;
																for (int k = 0; k < _map.countries.Length; k++) {
																				Country oc = _map.countries [k];
																				if (oc.regionsRect2D.Overlaps (rect)) {
																								int rc = oc.regions.Count;
																								for (int r = 0; r < rc; r++) {
																												Region or = oc.regions [r];
																												if (or.rect2D.Overlaps (rect)) {
																																nearbyRegions.Add (or);
																												}
																								}
																				}
																}
																map.RefreshCountryDefinition (countryIndex, nearbyRegions);
												} else {
																map.RefreshCountryDefinition (countryIndex, filterRegions);
												}
												if (highlightSelected) {
																map.HideProvinces ();
																CountryHighlightSelection (filterRegions);
												}
												if (editingMode == EDITING_MODE.PROVINCES) {
																map.RefreshProvinceDefinition (provinceIndex);
																if (highlightSelected) {
																				ProvinceHighlightSelection ();
																}
												}
								}

								public void DiscardChanges () {
												ClearSelection ();
												map.ReloadData ();
												RedrawFrontiers ();
												cityChanges = false;
												countryChanges = false;
												provinceChanges = false;
								}

								/// <summary>
								/// Moves any point inside circle.
								/// </summary>
								/// <returns>Returns a list with changed regions</returns>
								public List<Region> MoveCircle (Vector3 position, Vector3 dragAmount, float circleSize) {
												if (entityIndex < 0 || entityIndex >= entities.Length)
																return null;

												float circleSizeSqr = circleSize * circleSize;
												List<Region> regions = new List<Region> (100); 
												// Current region
												Region currentRegion = entities [entityIndex].regions [regionIndex];
												regions.Add (currentRegion);
												// Current region's neighbours
												if (!circleCurrentRegionOnly) {
																for (int r = 0; r < currentRegion.neighbours.Count; r++) {
																				Region region = currentRegion.neighbours [r];
																				if (!regions.Contains (region))
																								regions.Add (region);
																}
																// If we're editing provinces, check if country points can be moved as well
																if (editingMode == EDITING_MODE.PROVINCES) {
																				// Moves current country
																				for (int cr = 0; cr < map.countries [countryIndex].regions.Count; cr++) {
																								Region countryRegion = map.countries [countryIndex].regions [cr];
																								if (!regions.Contains (countryRegion))
																												regions.Add (countryRegion);
																								// Moves neighbours
																								for (int r = 0; r < countryRegion.neighbours.Count; r++) {
																												Region region = countryRegion.neighbours [r];
																												if (!regions.Contains (region))
																																regions.Add (region);
																								}
																				}
																}
												}
												// Execute move operation on each point
												List<Region> affectedRegions = new List<Region> (regions.Count);
												for (int r = 0; r < regions.Count; r++) {
																Region region = regions [r];
																bool regionAffected = false;
																for (int p = 0; p < region.points.Length; p++) {
																				Vector3 rp = region.points [p];
																				float dist = (rp.x - position.x) * (rp.x - position.x) * 4.0f + (rp.y - position.y) * (rp.y - position.y);
																				if (dist < circleSizeSqr) {
																								if (circleMoveConstant) {
																												region.points [p] += dragAmount;
																								} else {
																												region.points [p] += dragAmount - dragAmount * (dist / circleSizeSqr);
																								}
																								regionAffected = true;
																				}
																}
																if (regionAffected)
																				affectedRegions.Add (region);
												}
												return affectedRegions;
								}


								/// <summary>
								/// Moves a single point.
								/// </summary>
								/// <returns>Returns a list of affected regions</returns>
								public List<Region> MovePoint (Vector3 position, Vector3 dragAmount) {
												return MoveCircle (position, dragAmount, 0.0001f);
								}

								/// <summary>
								/// Moves points of other regions towards current frontier
								/// </summary>
								public bool Magnet (Vector3 position, float circleSize) {
												if (entityIndex < 0 || entityIndex >= entities.Length)
																return false;

												Region currentRegion = entities [entityIndex].regions [regionIndex];
												float circleSizeSqr = circleSize * circleSize;

												Dictionary<Vector3, bool> attractorsUse = new Dictionary<Vector3, bool> ();
												// Attract points of other regions/countries
												List<Region> regions = new List<Region> ();
												for (int c = 0; c < entities.Length; c++) {
																IAdminEntity entity = entities [c];
																if (entity.regions == null)
																				continue;
																for (int r = 0; r < entity.regions.Count; r++) {
																				if (c != entityIndex || r != regionIndex) {
																								regions.Add (entities [c].regions [r]);
																				}
																}
												}
												if (editingMode == EDITING_MODE.PROVINCES) {
																// Also add regions of current country and neighbours
																for (int r = 0; r < map.countries [countryIndex].regions.Count; r++) {
																				Region region = map.countries [countryIndex].regions [r];
																				regions.Add (region);
																				for (int n = 0; n < region.neighbours.Count; n++) {
																								Region nregion = region.neighbours [n];
																								if (!regions.Contains (nregion))
																												regions.Add (nregion);
																				}
																}
												}

												bool changes = false;
												Vector3 goodAttractor = Misc.Vector3zero;

												for (int r = 0; r < regions.Count; r++) {
																Region region = regions [r];
																bool changesInThisRegion = false;
																for (int p = 0; p < region.points.Length; p++) {
																				Vector3 rp = region.points [p];
																				float dist = (rp.x - position.x) * (rp.x - position.x) * 4.0f + (rp.y - position.y) * (rp.y - position.y);
																				if (dist < circleSizeSqr) {
																								float minDist = float.MaxValue;
																								int nearest = -1;
																								for (int a = 0; a < currentRegion.points.Length; a++) {
																												Vector3 attractor = currentRegion.points [a];
																												dist = (rp.x - attractor.x) * (rp.x - attractor.x) * 4.0f + (rp.y - attractor.y) * (rp.y - attractor.y);
																												if (dist < circleSizeSqr && dist < minDist) {
																																minDist = dist;
																																nearest = a;
																																goodAttractor = attractor;
																												}
																								}
																								if (nearest >= 0) {
																												changes = true;
																												// Check if this attractor is being used by other point
																												bool used = attractorsUse.ContainsKey (goodAttractor);
																												if (!used || magnetAgressiveMode) {
																																region.points [p] = goodAttractor;
																																if (!used)
																																				attractorsUse.Add (goodAttractor, true);
																																changesInThisRegion = true;
																												}
																								}
																				}
																}
																if (changesInThisRegion) {
																				// Remove duplicate points in this region
																				Dictionary<Vector3, bool> repeated = new Dictionary<Vector3, bool> ();
																				for (int k = 0; k < region.points.Length; k++)
																								if (!repeated.ContainsKey (region.points [k]))
																												repeated.Add (region.points [k], true);
																				region.points = new List<Vector3> (repeated.Keys).ToArray ();
																}
												}
												return changes;
								}


								/// <summary>
								/// Erase points inside circle.
								/// </summary>
								public bool Erase (Vector3 position, float circleSize) {
												if (entityIndex < 0 || entityIndex >= entities.Length)
																return false;

												if (circleCurrentRegionOnly) {
																Region currentRegion = entities [entityIndex].regions [regionIndex];
																return EraseFromRegion (currentRegion, position, circleSize);
												} else {
																return EraseFromAnyRegion (position, circleSize);
												}
								}

								bool EraseFromRegion (Region region, Vector3 position, float circleSize) {

												float circleSizeSqr = circleSize * circleSize;
			
												// Erase points inside the circle
												bool changes = false;
												List<Vector3> temp = new List<Vector3> (region.points.Length);
												for (int p = 0; p < region.points.Length; p++) {
																Vector3 rp = region.points [p];
																float dist = (rp.x - position.x) * (rp.x - position.x) * 4.0f + (rp.y - position.y) * (rp.y - position.y);
																if (dist > circleSizeSqr) {
																				temp.Add (rp);
																} else {
																				changes = true;
																}
												}
												if (changes) {
																Vector3[] newPoints = temp.ToArray ();
																if (newPoints.Length >= 3) {
																				region.points = newPoints;
																} else {
//					// Remove region from entity
//					if (region.entity.regions.Contains(region)) {
//						region.entity.regions.Remove(region);
//					}
//					SetInfoMsg("Region removed from entity!");
																				SetInfoMsg ("Minimum of 3 points is required. To remove the region use the DELETE button.");
																				return false;
																}
																if (region.entity is Country) {
																				countryChanges = true;
																} else {
																				provinceChanges = true;
																}
												}

												return changes;
								}

								bool EraseFromAnyRegion (Vector3 position, float circleSize) {

												// Try to delete from any region of any country
												bool changes = false;
												for (int c = 0; c < _map.countries.Length; c++) {
																Country country = _map.countries [c];
																for (int cr = 0; cr < country.regions.Count; cr++) {
																				Region region = country.regions [cr];
																				if (EraseFromRegion (region, position, circleSize))
																								changes = true;
																}
												}

												// Try to delete from any region of any province
												if (editingMode == EDITING_MODE.PROVINCES && _map.provinces != null) {
																for (int p = 0; p < _map.provinces.Length; p++) {
																				Province province = _map.provinces [p];
																				for (int pr = 0; pr < province.regions.Count; pr++) {
																								Region region = province.regions [pr];
																								if (EraseFromRegion (region, position, circleSize))
																												changes = true;
																				}
																}
												}
												return changes;
								}


								public void UndoRegionsPush (List<Region> regions) {
												UndoRegionsInsertAtCurrentPos (regions);
												undoRegionsDummyFlag++;
												if (editingMode == EDITING_MODE.COUNTRIES) {
																countryChanges = true;
												} else
																provinceChanges = true;
								}

								public void UndoRegionsInsertAtCurrentPos (List<Region> regions) {
												if (regions == null)
																return;
												List<Region> clonedRegions = new List<Region> ();
												for (int k = 0; k < regions.Count; k++) {
																clonedRegions.Add (regions [k].Clone ());
												}
												if (undoRegionsDummyFlag > undoRegionsList.Count)
																undoRegionsDummyFlag = undoRegionsList.Count;
												undoRegionsList.Insert (undoRegionsDummyFlag, clonedRegions);
								}

								public void UndoCitiesPush () {
												UndoCitiesInsertAtCurrentPos ();
												undoCitiesDummyFlag++;
								}

								public void UndoCitiesInsertAtCurrentPos () {
												List<City> cities = new List<City> (map.cities.Count);
												for (int k = 0; k < map.cities.Count; k++)
																cities.Add (map.cities [k].Clone ());
												if (undoCitiesDummyFlag > undoCitiesList.Count)
																undoCitiesDummyFlag = undoCitiesList.Count;
												undoCitiesList.Insert (undoCitiesDummyFlag, cities);
								}

								public void UndoMountPointsPush () {
												UndoMountPointsInsertAtCurrentPos ();
												undoMountPointsDummyFlag++;
								}

								public void UndoMountPointsInsertAtCurrentPos () {
												if (map.mountPoints == null)
																map.mountPoints = new List<MountPoint> ();
												List<MountPoint> mountPoints = new List<MountPoint> (map.mountPoints.Count);
												for (int k = 0; k < map.mountPoints.Count; k++)
																mountPoints.Add (map.mountPoints [k].Clone ());
												if (undoMountPointsDummyFlag > undoMountPointsList.Count)
																undoMountPointsDummyFlag = undoMountPointsList.Count;
												undoMountPointsList.Insert (undoMountPointsDummyFlag, mountPoints);
								}

								public void UndoHandle () {
												if (undoRegionsList != null && undoRegionsList.Count >= 2) {
																if (undoRegionsDummyFlag >= undoRegionsList.Count) {
																				undoRegionsDummyFlag = undoRegionsList.Count - 2;
																}
																List<Region> savedRegions = undoRegionsList [undoRegionsDummyFlag];
																RestoreRegions (savedRegions);
												}
												if (undoCitiesList != null && undoCitiesList.Count >= 2) {
																if (undoCitiesDummyFlag >= undoCitiesList.Count) {
																				undoCitiesDummyFlag = undoCitiesList.Count - 2;
																}
																List<City> savedCities = undoCitiesList [undoCitiesDummyFlag];
																RestoreCities (savedCities);
												}
												if (undoMountPointsList != null && undoMountPointsList.Count >= 2) {
																if (undoMountPointsDummyFlag >= undoMountPointsList.Count) {
																				undoMountPointsDummyFlag = undoMountPointsList.Count - 2;
																}
																List<MountPoint> savedMountPoints = undoMountPointsList [undoMountPointsDummyFlag];
																RestoreMountPoints (savedMountPoints);
												}
								}

		
								void RestoreRegions (List<Region> savedRegions) {
												for (int k = 0; k < savedRegions.Count; k++) {
																IAdminEntity entity = savedRegions [k].entity;
																int regionIndex = savedRegions [k].regionIndex;
																entity.regions [regionIndex] = savedRegions [k];
												}
												RedrawFrontiers ();
								}

								void RestoreCities (List<City> savedCities) {
												map.cities = savedCities;
												lastCityCount = -1;
												ReloadCityNames ();
												map.DrawCities ();
								}

								void RestoreMountPoints (List<MountPoint> savedMountPoints) {
												map.mountPoints = savedMountPoints;
												lastMountPointCount = -1;
												ReloadMountPointNames ();
												map.DrawMountPoints ();
								}

								void EntityAdd (IAdminEntity newEntity) {
												if (newEntity is Country) {
																map.CountryAdd ((Country)newEntity);
												} else {
																map.ProvinceAdd ((Province)newEntity);
												}
								}



								public void SplitHorizontally () {
												if (entityIndex < 0 || entityIndex >= entities.Length)
																return;

												IAdminEntity currentEntity = entities [entityIndex];
												Region currentRegion = currentEntity.regions [regionIndex];
												Vector2 center = currentRegion.center;
												List<Vector3> half1 = new List<Vector3> ();
												List<Vector3> half2 = new List<Vector3> ();
												int prevSide = 0;
												for (int k = 0; k < currentRegion.points.Length; k++) {
																Vector3 p = currentRegion.points [k];
																if (p.y > currentRegion.center.y) {
																				half1.Add (p);
																				if (prevSide == -1)
																								half2.Add (p);
																				prevSide = 1;
																}
																if (p.y <= currentRegion.center.y) {
																				half2.Add (p);
																				if (prevSide == 1)
																								half1.Add (p);
																				prevSide = -1;
																}
												}
												// Setup new entity
												IAdminEntity newEntity;
												if (currentEntity is Country) {
																newEntity = new Country ("New " + currentEntity.name, ((Country)currentEntity).continent);
												} else {
																newEntity = new Province ("New " + currentEntity.name, ((Province)currentEntity).countryIndex);
																newEntity.regions = new List<Region> ();
												}
												EntityAdd (newEntity);

												// Update polygons
												Region newRegion = new Region (newEntity, 0);
												if (entities [countryIndex].center.y > center.y) {
																currentRegion.points = half1.ToArray ();
																newRegion.points = half2.ToArray ();
												} else {
																currentRegion.points = half2.ToArray ();
																newRegion.points = half1.ToArray ();
												}
												newEntity.regions.Add (newRegion);

												// Refresh old entity and selects the new
												if (currentEntity is Country) {
																map.RefreshCountryDefinition (countryIndex, highlightedRegions);
																countryIndex = map.countries.Length - 1;
																countryRegionIndex = 0;
												} else {
																map.RefreshProvinceDefinition (provinceIndex);
																provinceIndex = map.provinces.Length - 1;
																provinceRegionIndex = 0;
												}

												// Refresh lines
												highlightedRegions.Add (newRegion);
												RedrawFrontiers ();
												map.RedrawMapLabels ();
								}

								public void SplitVertically () {
												if (entityIndex < 0 || entityIndex >= entities.Length)
																return;

												IAdminEntity currentEntity = entities [entityIndex];
												Region currentRegion = currentEntity.regions [regionIndex];
												Vector2 center = currentRegion.center;
												List<Vector3> half1 = new List<Vector3> ();
												List<Vector3> half2 = new List<Vector3> ();
												int prevSide = 0;
												for (int k = 0; k < currentRegion.points.Length; k++) {
																Vector3 p = currentRegion.points [k];
																if (p.x > currentRegion.center.x) {
																				half1.Add (p);
																				if (prevSide == -1)
																								half2.Add (p);
																				prevSide = 1;
																}
																if (p.x <= currentRegion.center.x) {
																				half2.Add (p);
																				if (prevSide == 1)
																								half1.Add (p);
																				prevSide = -1;
																}
												}

												// Setup new entity
												IAdminEntity newEntity;
												if (currentEntity is Country) {
																newEntity = new Country ("New " + currentEntity.name, ((Country)currentEntity).continent);
												} else {
																newEntity = new Province ("New " + currentEntity.name, ((Province)currentEntity).countryIndex);
																newEntity.regions = new List<Region> ();
												}
												EntityAdd (newEntity);
			
												// Update polygons
												Region newRegion = new Region (newEntity, 0);
												if (entities [countryIndex].center.x > center.x) {
																currentRegion.points = half1.ToArray ();
																newRegion.points = half2.ToArray ();
												} else {
																currentRegion.points = half2.ToArray ();
																newRegion.points = half1.ToArray ();
												}
												newEntity.regions.Add (newRegion);
			
												// Refresh old entity and selects the new
												if (currentEntity is Country) {
																map.RefreshCountryDefinition (countryIndex, highlightedRegions);
																countryIndex = map.countries.Length - 1;
																countryRegionIndex = 0;
												} else {
																map.RefreshProvinceDefinition (provinceIndex);
																provinceIndex = map.provinces.Length - 1;
																provinceRegionIndex = 0;
												}
			
												// Refresh lines
												highlightedRegions.Add (newRegion);
												RedrawFrontiers ();
												map.RedrawMapLabels ();
								}

	
								/// <summary>
								/// Adds the new point to currently selected region.
								/// </summary>
								public void AddPoint (Vector2 newPoint) {
												if (entities == null || entityIndex < 0 || entityIndex >= entities.Length || regionIndex < 0 || entities [entityIndex].regions == null || regionIndex >= entities [entityIndex].regions.Count)
																return;
												List<Region> affectedRegions = new List<Region> ();
												Region region = entities [entityIndex].regions [regionIndex];
												float minDist = float.MaxValue;
												int nearest = -1, previous = -1;
												int max = region.points.Length;
												for (int p = 0; p < max; p++) {
																int q = p == 0 ? max - 1 : p - 1;
																Vector3 rp = (region.points [p] + region.points [q]) * 0.5f;
																float dist = (rp.x - newPoint.x) * (rp.x - newPoint.x) * 4 + (rp.y - newPoint.y) * (rp.y - newPoint.y);
																if (dist < minDist) {
																				// Get nearest point
																				minDist = dist;
																				nearest = p;
																				previous = q;
																}
												}

												if (nearest >= 0) {
																Vector3 pointToInsert = (region.points [nearest] + region.points [previous]) * 0.5f;

																// Check if nearest and previous exists in any neighbour
																int nearest2 = -1, previous2 = -1;
																for (int n = 0; n < region.neighbours.Count; n++) {
																				Region nregion = region.neighbours [n];
																				for (int p = 0; p < nregion.points.Length; p++) {
																								if (nregion.points [p] == region.points [nearest]) {
																												nearest2 = p;
																								}
																								if (nregion.points [p] == region.points [previous]) {
																												previous2 = p;
																								}
																				}
																				if (nearest2 >= 0 && previous2 >= 0) {
																								nregion.points = InsertPoint (nregion.points, previous2, pointToInsert);
																								affectedRegions.Add (nregion);
																								break;
																				}
																}

																// Insert the point in the current region (must be done after inserting in the neighbour so nearest/previous don't unsync)
																region.points = InsertPoint (region.points, nearest, pointToInsert);
																affectedRegions.Add (region);
												} 
								}

								Vector3[] InsertPoint (Vector3[] pointArray, int index, Vector3 pointToInsert) {
												List<Vector3> temp = new List<Vector3> (pointArray.Length + 1);
												for (int k = 0; k < pointArray.Length; k++) {
																if (k == index)
																				temp.Add (pointToInsert);
																temp.Add (pointArray [k]);
												}
												return temp.ToArray ();
								}


								public void SetInfoMsg (string msg) {
												this.infoMsg = msg;
												infoMsgStartTime = DateTime.Now;
								}

								public bool GetVertexNearSpherePos (Vector3 mapPos, out Vector2 nearPoint) {
												// Iterate country regions
												int numCountries = _map.countries.Length;
												Vector2 np = mapPos;
												float minDist = float.MaxValue;
												// Countries
												for (int c = 0; c < numCountries; c++) {
																Country country = _map.countries [c];
																int regCount = country.regions.Count;
																for (int cr = 0; cr < regCount; cr++) {
																				Region region = country.regions [cr];
																				int pointCount = region.points.Length;
																				for (int p = 0; p < pointCount; p++) {
																								float dist = (mapPos - region.points [p]).sqrMagnitude;
																								if (dist < minDist) {
																												minDist = dist;
																												np = region.points [p];
																								}
																				}
																}
												}
												// Provinces
												if (_map.editor.editingMode == EDITING_MODE.PROVINCES) {
																int numProvinces = _map.provinces.Length;
																for (int p = 0; p < numProvinces; p++) {
																				Province province = _map.provinces [p];
																				if (province.regions == null)
																								_map.ReadProvincePackedString (province);
																				if (province.regions == null)
																								continue;
																				int regCount = province.regions.Count;
																				for (int pr = 0; pr < regCount; pr++) {
																								Region region = province.regions [pr];
																								int pointCount = region.points.Length;
																								for (int po = 0; po < pointCount; po++) {
																												float dist = (mapPos - region.points [po]).sqrMagnitude;
																												if (dist < minDist) {
																																minDist = dist;
																																np = region.points [p];
																												}
																								}
																				}
																}
												}
			
												nearPoint = np;
												return nearPoint.x != mapPos.x || nearPoint.y != mapPos.y;
								}

								string GetCountryUniqueName (string proposedName) {
			
												string goodName = proposedName;
												int suffix = 0;
			
												while (_map.GetCountryIndex (goodName) >= 0) {
																suffix++;
																goodName = proposedName + suffix.ToString ();
												}
												return goodName;
			
								}


								#endregion

				}
}
