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

namespace WPMF
{

	[Serializable]
	[ExecuteInEditMode]
	public partial class WorldMap2D : MonoBehaviour
	{

		public const float MAP_PRECISION = 5000000f;

		const string OVERLAY_BASE = "OverlayLayer";
		const string OVERLAY_TEXT_ROOT = "TextRoot";
		const string SURFACE_LAYER = "Surfaces";
		const string MAPPER_CAM_PREFIX = "WorldMap2DMapperCam";

		#region Internal variables

		// resources
		Material coloredMat, texturizedMat;
		Material outlineMat, cursorMatH, cursorMatV, gridMat;
		Material lineMarkerMat;

		// gameObjects
		GameObject _surfacesLayer;

		GameObject surfacesLayer
		{
			get
			{
				if (_surfacesLayer == null)
					CreateSurfacesLayer();
				return _surfacesLayer;
			}
		}

		GameObject cursorLayerHLine, cursorLayerVLine, latitudeLayer, longitudeLayer;
		GameObject markersLayer;
		GameObject textRoot;

		// caché and gameObject lifetime control
		Dictionary<int, GameObject> surfaces;
		Dictionary<Color, Material> coloredMatCache;
		Dictionary<double, Region> frontiersCacheHit;
		List<Vector3> frontiersPoints;

		// FlyTo functionality
		Quaternion flyToStartQuaternion, flyToEndQuaternion;
		Vector3 flyToStartLocation, flyToEndLocation;
		bool flyToActive;
        DateTime flyToStartTime; 
        float flyToDuration; // In Seconds

		// UI interaction variables
		int mapUnityLayer;
		// will use the UI Layer for the culling of overlay layers
		Vector3 mouseDragStart, dragDirection, mouseDragStartHitPos;
		int dragDamping;
		float wheelAccel, dragSpeed, maxFrustumDistanceSqr, lastDistanceFromCamera;
		bool dragging, hasDragged, lastMouseMapHitPosGood;
		float lastCamOrtographicSize;
		Vector3 lastMapPosition, lastCamPosition;
		Vector3 lastMouseMapHitPos, prevMouseMapHitPos;
		//		bool hasMoved;	// true if map / camera has moved in las frame

		// Overlay (Labels, tickers, ...)
		GameObject overlayLayer;

		public static float mapWidth { get { return WorldMap2D.instance != null ? WorldMap2D.instance.transform.localScale.x : 200.0f; } }

		public static float mapHeight { get { return WorldMap2D.instance != null ? WorldMap2D.instance.transform.localScale.y : 100.0f; } }

		Font labelsFont;
		Material labelsShadowMaterial;
		RenderTexture overlayRT;
		Camera _currentCamera, mapperCam;
		string mapperCamName;

		int layerMask
		{
			get
			{
				if (!Application.isPlaying || currentCamera == _mainCamera)
					return 1 << mapUnityLayer;
				else
					return 1 << _renderViewport.layer;
			}
		}

		public Camera currentCamera
		{
			get
			{
				if (_currentCamera == null)
					SetupViewport();
				return _currentCamera;
			}
		}

		public int currentDecoratorCount;
		bool updateDoneThisTurn;
		public bool isDirty;
		// internal variable used to confirm changes in custom inspector - don't change its value

		#endregion



		#region Game loop events

		void OnEnable()
		{
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": enable wpm");
#endif
			if (countries == null)
			{
				Init();
			}

			// Check material
			Renderer renderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
			if (renderer.sharedMaterial == null)
			{
				RestyleEarth();
			}

			if (hudMatCountry != null && hudMatCountry.color != _fillColor)
			{
				hudMatCountry.color = _fillColor;
			}
			if (frontiersMat != null)
			{
				frontiersMat.color = _frontiersColor;
				frontiersMat.SetColor("_OuterColor", frontiersColorOuter);
			}
			if (hudMatProvince != null && hudMatProvince.color != _provincesFillColor)
			{
				hudMatProvince.color = _provincesFillColor;
			}
			if (provincesMat != null && provincesMat.color != _provincesColor)
			{
				provincesMat.color = _provincesColor;
			}
			if (citiesNormalMat.color != _citiesColor)
			{
				citiesNormalMat.color = _citiesColor;
			}
			if (citiesRegionCapitalMat.color != _citiesRegionCapitalColor)
			{
				citiesRegionCapitalMat.color = _citiesRegionCapitalColor;
			}
			if (citiesCountryCapitalMat.color != _citiesCountryCapitalColor)
			{
				citiesCountryCapitalMat.color = _citiesCountryCapitalColor;
			}
			if (outlineMat.color != _outlineColor)
			{
				outlineMat.color = _outlineColor;
			}
			if (cursorMatH.color != _cursorColor)
			{
				cursorMatH.color = _cursorColor;
			}
			if (cursorMatV.color != _cursorColor)
			{
				cursorMatV.color = _cursorColor;
			}
			if (gridMat.color != _gridColor)
			{
				gridMat.color = _gridColor;
			}

			if (_renderViewport == null)
			{
				SetupViewport();
			}
		}

		void OnDisable()
		{
			if (_currentCamera != null && _currentCamera == mapperCam)
				_currentCamera.enabled = false;
		}

		void OnDestroy()
		{
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": destroy wpm");
#endif
			if (_surfacesLayer != null)
				GameObject.DestroyImmediate(_surfacesLayer);
			overlayLayer = null;
			DestroyMapperCam();
		}

		void Reset()
		{
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": reset");
#endif
			Redraw();
		}

		void Update()
		{
			if (currentCamera == null || !Application.isPlaying)
				return;

			if (updateDoneThisTurn)
				return;
			updateDoneThisTurn = true;

			// Check Viewport scale
			CheckViewportScale();

			// Check if navigateTo... has been called and in this case rotate the globe until the country is centered
			if (flyToActive)
				MoveToDestination();

			// Check whether the points is on an UI element, then cancels
			if (UnityEngine.EventSystems.EventSystem.current != null)
			{
				if (Input.touchSupported && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
				{
					return;
				}
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
					return;
			}

			// Verify if mouse enter a country boundary - we only check if mouse is inside the sphere of world
			if (mouseIsOver)
			{
				if (!Input.touchSupported || Input.GetMouseButton(0))
				{
					bool goodHit = GetLocalHitFromMousePos(out lastMouseMapHitPos);
					lastMouseMapHitPosGood = goodHit;
				}

				CheckMousePos();
				// Remember the last element clicked
				if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
				{
					_countryLastClicked = _countryHighlightedIndex;
					_countryRegionLastClicked = _countryRegionHighlightedIndex;
					_provinceLastClicked = _provinceHighlightedIndex;
					_provinceRegionLastClicked = _provinceRegionHighlightedIndex;
					_cityLastClicked = _cityHighlightedIndex;
				}

				if (!hasDragged && wheelAccel == 0 && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
				{
					if (_countryLastClicked >= 0 && OnCountryClick != null)
						OnCountryClick(_countryLastClicked, _countryRegionLastClicked);
					if (_provinceLastClicked >= 0 && OnProvinceClick != null)
						OnProvinceClick(_provinceLastClicked, _provinceRegionLastClicked);
					if (_cityLastClicked >= 0 && OnCityClick != null)
						OnCityClick(_cityLastClicked);
				}

			}

			if (hasDragged && (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
			{
				if (OnDragEnd != null)
					OnDragEnd();
			}

			bool buttonLeftPressed = Input.GetMouseButton(0) && (!Input.touchSupported || Input.touchCount == 1);
			// if mouse/finger is over map, implement drag and zoom of the world
			if (mouseIsOver)
			{
				// Use left mouse button to drag the map
				if (_allowUserDrag && !flyToActive)
				{
					if (Input.GetMouseButtonDown(0))
					{
						mouseDragStart = Input.mousePosition;
						mouseDragStartHitPos = transform.TransformPoint(lastMouseMapHitPos);
						prevMouseMapHitPos = lastMouseMapHitPos;
						dragging = true;
						hasDragged = false;
					}

					// Use right mouse button and fly and center on target country
					if (Input.GetMouseButtonDown(1) && !Input.touchSupported)
					{   // two fingers can be interpreted as right mouse button -> prevent this.
						if (_countryHighlightedIndex >= 0 && Input.GetMouseButtonDown(1) && _centerOnRightClick)
						{
							FlyToCountry(_countryHighlightedIndex, 0.8f);
						}
					}
				}
			}

			if (dragging)
			{
				if (buttonLeftPressed)
				{
					if (_dragConstantSpeed)
					{
						if (lastMouseMapHitPosGood && mouseIsOver)
						{
							Vector3 hitPos = transform.TransformPoint(lastMouseMapHitPos);
							if (_staticCamera)
							{
								dragDirection = hitPos - transform.TransformPoint(prevMouseMapHitPos);
							}
							else
							{
								dragDirection = hitPos - mouseDragStartHitPos;
							}
							dragDirection.x = ApplyDragThreshold(dragDirection.x);
							dragDirection.y = ApplyDragThreshold(dragDirection.y);
							if (dragDirection.x != 0 || dragDirection.y != 0)
							{
								dragDamping = 1;
							}
						}
					}
					else
					{
						dragDirection = (Input.mousePosition - mouseDragStart);
						dragDirection.x = ApplyDragThreshold(dragDirection.x);
						dragDirection.y = ApplyDragThreshold(dragDirection.y);
						if (dragDirection.x != 0 || dragDirection.y != 0)
						{
							if (_currentCamera.orthographic)
							{
								dragSpeed = _currentCamera.orthographicSize * _mouseDragSensitivity * 0.00035f;
							}
							else
							{
								dragSpeed = Mathf.Sqrt(lastDistanceFromCamera) * _mouseDragSensitivity * 0.00035f;
							}
							dragDirection *= dragSpeed;
							dragDamping = 1;
						}
					}
				}
				else
					dragging = false;
			}

			// Check special keys
			if (_allowUserKeys && _allowUserDrag)
			{
				bool pressed = false;
				dragDirection = Misc.Vector3zero;
				if (Input.GetKey(KeyCode.W))
				{
					dragDirection += Misc.Vector3down;
					pressed = true;
				}
				if (Input.GetKey(KeyCode.S))
				{
					dragDirection += Misc.Vector3up;
					pressed = true;
				}
				if (Input.GetKey(KeyCode.A))
				{
					dragDirection += Misc.Vector3right;
					pressed = true;
				}
				if (Input.GetKey(KeyCode.D))
				{
					dragDirection += Misc.Vector3left;
					pressed = true;
				}
				if (pressed)
				{
					if (_currentCamera.orthographic)
					{
						dragSpeed = Mathf.Sqrt(_currentCamera.orthographicSize) * 10.0f * _mouseDragSensitivity;
					}
					else
					{
						dragSpeed = Mathf.Sqrt(lastDistanceFromCamera) * _mouseDragSensitivity;
					}
					dragDirection *= 0.1f * dragSpeed;
					if (dragFlipDirection)
						dragDirection *= -1;
					dragDamping = 1;
				}
			}

			if (!hasDragged)
			{
				if (dragDirection != Misc.Vector3zero)
				{
					hasDragged = true;
					if (OnDragStart != null)
						OnDragStart();
				}
			}

			// Check scroll on borders
			if (_allowScrollOnScreenEdges && _allowUserDrag)
			{
				bool onEdge = false;
				float mx = Input.mousePosition.x;
				float my = Input.mousePosition.y;
				if (mx >= 0 && mx < Screen.width && my >= 0 && my < Screen.height)
				{
					if (my < _screenEdgeThickness)
					{
						dragDirection += Misc.Vector3up;
						onEdge = true;
					}
					if (my >= Screen.height - _screenEdgeThickness)
					{
						dragDirection += Misc.Vector3down;
						onEdge = true;
					}
					if (mx < _screenEdgeThickness)
					{
						dragDirection += Misc.Vector3right;
						onEdge = true;
					}
					if (mx >= Screen.width - _screenEdgeThickness)
					{
						dragDirection += Misc.Vector3left;
						onEdge = true;
					}
				}
				if (onEdge)
				{
					if (_currentCamera.orthographic)
					{
						dragSpeed = Mathf.Sqrt(_currentCamera.orthographicSize) * 10.0f * _mouseDragSensitivity;
					}
					else
					{
						dragSpeed = Mathf.Sqrt(lastDistanceFromCamera) * _mouseDragSensitivity;
					}
					dragDirection *= 0.1f * dragSpeed;
					if (dragFlipDirection)
						dragDirection *= -1;
					dragDamping = 1;
				}
			}

			if (dragDamping > 0)
			{ // && !buttonLeftPressed) {
				if (dragDamping < 20)
				{
					dragging = true;
					if (_staticCamera)
					{
						transform.Translate(dragDirection / dragDamping);
						if (_dragConstantSpeed)
						{
							GetLocalHitFromMousePos(out prevMouseMapHitPos);
						}
					}
					else
					{
						_currentCamera.transform.Translate(-dragDirection / dragDamping);
					}
					if (_dragConstantSpeed)
						dragDamping = 0;
					else
						dragDamping++;
				}
				else
				{
					dragDamping = 0;
				}
			}

			// Use mouse wheel to zoom in and out
			if (allowUserZoom && (mouseIsOver || wheelAccel != 0))
			{
				float wheel = Input.GetAxis("Mouse ScrollWheel");
				wheelAccel += wheel * (_invertZoomDirection ? -1 : 1);
				;

				// Support for pinch on mobile
				if (Input.touchSupported && Input.touchCount == 2)
				{
					// Store both touches.
					Touch touchZero = Input.GetTouch(0);
					Touch touchOne = Input.GetTouch(1);

					// Find the position in the previous frame of each touch.
					Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
					Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

					// Find the magnitude of the vector (the distance) between the touches in each frame.
					float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
					float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

					// Find the difference in the distances between each frame.
					float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

					// Pass the delta to the wheel accel
					wheelAccel += deltaMagnitudeDiff;
				}

				if (wheelAccel != 0)
				{
					wheelAccel = Mathf.Clamp(wheelAccel, -0.1f, 0.1f);
					if (wheelAccel >= 0.01f || wheelAccel <= -0.01f)
					{
						Vector3 dest = lastMouseMapHitPosGood ? transform.TransformPoint(lastMouseMapHitPos) : transform.position;

						if (_currentCamera.orthographic)
						{
							_currentCamera.orthographicSize += _currentCamera.orthographicSize * wheelAccel * _mouseWheelSensitivity;
							Vector3 v = (dest - _currentCamera.transform.position) * wheelAccel * _mouseWheelSensitivity;
							v = Vector3.ProjectOnPlane(v, transform.forward);
							transform.Translate(v);
						}
						else
						{
							if (_staticCamera)
							{
								transform.Translate((dest - _currentCamera.transform.position) * wheelAccel * _mouseWheelSensitivity);
							}
							else
							{
								_currentCamera.transform.Translate(-(dest - _currentCamera.transform.position) * wheelAccel * _mouseWheelSensitivity);
							}
						}
						if (_zoomConstantSpeed)
							wheelAccel = 0;
						else
							wheelAccel /= 1.15f;
					}
					else
					{
						wheelAccel = 0;
					}
				}
			}

			// Check boundaries
			if (transform.position != lastMapPosition || _currentCamera.transform.position != lastCamPosition || _currentCamera.orthographicSize != lastCamOrtographicSize)
			{
				// Last distance
				if (_currentCamera.orthographic)
				{
					_currentCamera.orthographicSize = Mathf.Clamp(_currentCamera.orthographicSize, 0.001f, maxFrustumDistanceSqr);
					// updates frontiers LOD
					float orthoScale = transform.localScale.y / 100f;
					if (_frontiersThinLines)
					{
						frontiersMat.shader.maximumLOD = 300;
					}
					else
					{
						frontiersMat.shader.maximumLOD = _currentCamera.orthographicSize < 2.2 * orthoScale ? 100 : (_currentCamera.orthographicSize < 8 * orthoScale ? 200 : 300);
					}
				}
				else
				{
					Plane plane = new Plane(transform.forward, transform.position);
					Ray ray = _currentCamera.ScreenPointToRay(Input.mousePosition);
					float enterPos;
					if (plane.Raycast(ray, out enterPos))
					{
						Vector3 dest = ray.GetPoint(enterPos);
						float planeDistance = Mathf.Abs(plane.GetDistanceToPoint(_currentCamera.transform.position));
						float camDistanceSqr = planeDistance * planeDistance;
						lastDistanceFromCamera = camDistanceSqr;
						float minDistance = _zoomMinDistance * transform.localScale.y;
						minDistance *= minDistance;
						if (camDistanceSqr < minDistance)
						{
							if (_staticCamera)
							{
								transform.position -= (dest - _currentCamera.transform.position).normalized * (Mathf.Sqrt(camDistanceSqr) - Mathf.Sqrt(minDistance));
							}
							else
							{
								Vector3 planePoint = _currentCamera.transform.position + plane.normal * planeDistance;
								_currentCamera.transform.position = planePoint - _currentCamera.transform.forward * Mathf.Sqrt(minDistance); // (lastDistanceFromCamera - Mathf.Sqrt (minDistance));
							}
							wheelAccel = 0;
						}
						else
						{
							float maxDistance = Mathf.Min(maxFrustumDistanceSqr, (_zoomMaxDistance * transform.localScale.y) * (_zoomMaxDistance * transform.localScale.y));
							if (camDistanceSqr > maxDistance)
							{
								// Get intersection point from camera with plane
								Vector3 planePoint = _currentCamera.transform.position + plane.normal * planeDistance;
								if (_staticCamera)
								{
									transform.position -= (planePoint - _currentCamera.transform.position).normalized * (Mathf.Sqrt(camDistanceSqr) - Mathf.Sqrt(maxDistance));
								}
								else
								{
									_currentCamera.transform.position += (planePoint - _currentCamera.transform.position).normalized * (Mathf.Sqrt(camDistanceSqr) - Mathf.Sqrt(maxDistance));
								}
								wheelAccel = 0;
							}
						}
						// updates frontiers LOD
						if (_frontiersThinLines)
						{
							frontiersMat.shader.maximumLOD = 300;
						}
						else
						{
							frontiersMat.shader.maximumLOD = lastDistanceFromCamera < 20 ? 100 : (lastDistanceFromCamera < 320 ? 200 : 300);
						}
					}
				}

				// Constraint to limits if user interaction is enabled

				if (_allowUserDrag || _allowUserZoom)
				{
					float limitLeft, limitRight;
					if (_fitWindowWidth)
					{
						limitLeft = 0;
						limitRight = 1.0f;
					}
					else
					{
						limitLeft = 0.9f;
						limitRight = 0.1f;
					}

					// Reduce floating-point errors
					Vector3 apos = transform.position;
					if (_renderViewport != gameObject)
					{
						transform.position -= apos;
						_currentCamera.transform.position -= apos;
					}
					Vector3 posEdge = transform.TransformPoint(0.5f, 0, 0);
					Vector3 pos = _currentCamera.WorldToViewportPoint(posEdge);
					if (pos.x < limitRight)
					{
						pos.x = limitRight;
						if (_staticCamera)
						{
							transform.position = _currentCamera.ViewportToWorldPoint(pos) - transform.right * 0.5f * mapWidth;
						}
						else
						{
							pos = _currentCamera.ViewportToWorldPoint(pos);
							_currentCamera.transform.position += (posEdge - pos);
						}
						dragDamping = 0;
					}
					else
					{
						posEdge = transform.TransformPoint(-0.5f, 0, 0);
						pos = _currentCamera.WorldToViewportPoint(posEdge);
						if (pos.x > limitLeft)
						{
							pos.x = limitLeft;
							if (_staticCamera)
							{
								transform.position = _currentCamera.ViewportToWorldPoint(pos) + transform.right * 0.5f * mapWidth;
							}
							else
							{
								pos = _currentCamera.ViewportToWorldPoint(pos);
								_currentCamera.transform.position += (posEdge - pos);
							}
						}
					}

					float limitTop, limitBottom;
					if (_fitWindowHeight)
					{
						limitTop = 1.0f;
						limitBottom = 0;
					}
					else
					{
						limitTop = 0.1f;
						limitBottom = 0.9f;
					}

					posEdge = transform.TransformPoint(0, 0.5f, 0);
					pos = _currentCamera.WorldToViewportPoint(posEdge);
					if (pos.y < limitTop)
					{
						pos.y = limitTop;
						if (_staticCamera)
						{
							transform.position = _currentCamera.ViewportToWorldPoint(pos) - transform.up * 0.5f * mapHeight;
						}
						else
						{
							pos = _currentCamera.ViewportToWorldPoint(pos);
							_currentCamera.transform.position += (posEdge - pos);
						}
					}
					else
					{
						posEdge = transform.TransformPoint(0, -0.5f, 0);
						pos = _currentCamera.WorldToViewportPoint(posEdge);
						if (pos.y > limitBottom)
						{
							pos.y = limitBottom;
							if (_staticCamera)
							{
								transform.position = _currentCamera.ViewportToWorldPoint(pos) + transform.up * 0.5f * mapHeight;
							}
							else
							{
								pos = _currentCamera.ViewportToWorldPoint(pos);
								_currentCamera.transform.position += (posEdge - pos);
							}
						}
					}
					// Reduce floating-point errors
					if (_renderViewport != gameObject)
					{
						transform.position += apos;
						_currentCamera.transform.position += apos;
					}
				}
				lastMapPosition = transform.position;
				lastCamPosition = _currentCamera.transform.position;
				lastCamOrtographicSize = _currentCamera.orthographicSize;
			}
		}

		void LateUpdate()
		{
			updateDoneThisTurn = false;
		}


		public void OnMouseEnter()
		{
			mouseIsOver = true;
		}

		public void OnMouseExit()
		{
			// Make sure it's outside of map
			Vector3 mousePos = Input.mousePosition;
			if (mousePos.x >= 0 && mousePos.x < Screen.width && mousePos.y >= 0 && mousePos.y < Screen.height)
			{
				Ray ray = Camera.main.ScreenPointToRay(mousePos);
				RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 2000);
				for (int k = 0; k < hits.Length; k++)
				{
					if (hits[k].collider.gameObject == _renderViewport)
						return;
				}
			}

			mouseIsOver = false;
			HideCountryRegionHighlight();
		}

		public void OnMouseClick()
		{
			mouseIsOver = true;
			Update();
		}

		public void OnMouseRelease()
		{
			Update();
			mouseIsOver = false;
			HideCountryRegionHighlight();
		}

		#endregion

		#region System initialization

		public void Init()
		{
			// Load materials
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": init");
#endif

			mapUnityLayer = gameObject.layer;
			// Updates layer in children
			foreach (Transform t in transform)
			{
				t.gameObject.layer = mapUnityLayer;
			}

			// Labels materials
			ReloadFont();

			// Map materials
			frontiersMat = Instantiate(Resources.Load<Material>("Materials/Frontiers"));
			frontiersMat.hideFlags = HideFlags.DontSave;
			frontiersMat.shader.maximumLOD = 300;
			hudMatCountry = Instantiate(Resources.Load<Material>("Materials/HudCountry"));
			hudMatCountry.hideFlags = HideFlags.DontSave;
			hudMatProvince = Instantiate(Resources.Load<Material>("Materials/HudProvince"));
			hudMatProvince.hideFlags = HideFlags.DontSave;
			hudMatProvince.renderQueue++;   // render on top of country highlight
			citySpot = Resources.Load<GameObject>("Prefabs/CitySpot");
			citySpotCapitalRegion = Resources.Load<GameObject>("Prefabs/CityCapitalRegionSpot");
			citySpotCapitalCountry = Resources.Load<GameObject>("Prefabs/CityCapitalCountrySpot");
			citiesNormalMat = Instantiate(Resources.Load<Material>("Materials/Cities"));
			citiesNormalMat.name = "Cities";
			citiesNormalMat.hideFlags = HideFlags.DontSave;
			citiesRegionCapitalMat = Instantiate(Resources.Load<Material>("Materials/CitiesCapitalRegion"));
			citiesRegionCapitalMat.name = "CitiesCapitalRegion";
			citiesRegionCapitalMat.hideFlags = HideFlags.DontSave;
			citiesCountryCapitalMat = Instantiate(Resources.Load<Material>("Materials/CitiesCapitalCountry"));
			citiesCountryCapitalMat.name = "CitiesCapitalCountry";
			citiesCountryCapitalMat.hideFlags = HideFlags.DontSave;

			provincesMat = Instantiate(Resources.Load<Material>("Materials/Provinces"));
			provincesMat.hideFlags = HideFlags.DontSave;
			outlineMat = Instantiate(Resources.Load<Material>("Materials/Outline"));
			outlineMat.hideFlags = HideFlags.DontSave;
			coloredMat = Instantiate(Resources.Load<Material>("Materials/ColorizedRegion"));
			coloredMat.hideFlags = HideFlags.DontSave;
			texturizedMat = Instantiate(Resources.Load<Material>("Materials/TexturizedRegion"));
			texturizedMat.hideFlags = HideFlags.DontSave;
			cursorMatH = Instantiate(Resources.Load<Material>("Materials/CursorH"));
			cursorMatH.hideFlags = HideFlags.DontSave;
			cursorMatV = Instantiate(Resources.Load<Material>("Materials/CursorV"));
			cursorMatV.hideFlags = HideFlags.DontSave;
			gridMat = Instantiate(Resources.Load<Material>("Materials/Grid"));
			gridMat.hideFlags = HideFlags.DontSave;
			lineMarkerMat = Instantiate(Resources.Load<Material>("Materials/LineMarker"));
			lineMarkerMat.hideFlags = HideFlags.DontSave;
			mountPointSpot = Resources.Load<GameObject>("Prefabs/MountPointSpot");
			mountPointsMat = Instantiate(Resources.Load<Material>("Materials/Mount Points"));
			mountPointsMat.hideFlags = HideFlags.DontSave;

			coloredMatCache = new Dictionary<Color, Material>();

			GetFrustumDistance(); // init maxFrustumDistanceSqr

			ReloadData();
		}

		void ReloadFont()
		{
			if (_countryLabelsFont == null)
			{
				labelsFont = Instantiate(Resources.Load<Font>("Font/Lato"));
			}
			else
			{
				labelsFont = Instantiate(_countryLabelsFont);
			}
			labelsFont.hideFlags = HideFlags.DontSave;

			Material fontMaterial = Instantiate(Resources.Load<Material>("Materials/Font")); // this material is linked to a shader that has into account zbuffer
			if (labelsFont.material != null)
			{
				fontMaterial.mainTexture = labelsFont.material.mainTexture;
			}
			fontMaterial.hideFlags = HideFlags.DontSave;
			labelsFont.material = fontMaterial;
			labelsShadowMaterial = GameObject.Instantiate(fontMaterial);
			labelsShadowMaterial.hideFlags = HideFlags.DontSave;
			labelsShadowMaterial.renderQueue--;
		}

		/// <summary>
		/// Reloads the data of frontiers and cities from datafiles and redraws the map.
		/// </summary>
		public void ReloadData()
		{
			// Destroy surfaces layer
			DestroySurfaces();
			// read precomputed data
			ReadCountriesPackedString();
			ReadCitiesPackedString();
			if (_showProvinces || GetComponent<WorldMap2D_Editor>() != null)
				ReadProvincesPackedString();
			ReadMountPointsPackedString();

			// Redraw frontiers and cities -- destroy layers if they already exists
			Redraw();
		}


		void DestroySurfaces()
		{
			HideCountryRegionHighlights(true);
			HideProvinceRegionHighlight();
			if (frontiersCacheHit != null)
				frontiersCacheHit.Clear();
			InitSurfacesCache();
			if (_surfacesLayer != null)
				DestroyImmediate(_surfacesLayer);
			if (provincesObj != null)
				DestroyImmediate(provincesObj);
		}


		#endregion

		#region Drawing stuff

		/// <summary>
		/// Used internally and by other components to redraw the layers in specific moments. You shouldn't call this method directly.
		/// </summary>
		public void Redraw()
		{
			if (!gameObject.activeInHierarchy)
				return;
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": Redraw");
#endif

			InitSurfacesCache();    // Initialize surface cache, destroys already generated surfaces

			RestyleEarth(); // Apply texture to Earth

			DrawFrontiers();    // Redraw frontiers -- the next method is also called from width property when this is changed

			DrawAllProvinceBorders(false); // Redraw province borders

			DrawCities();       // Redraw cities layer

			DrawMountPoints();  // Redraw mount points (only in Editor time)

			DrawCursor();       // Draw cursor lines

			DrawGrid();     // Draw longitude & latitude lines

			DrawMapLabels();    // Destroy existing texts and draw them again

			SetupViewport();
			if (lastDistanceFromCamera == 0)
				lastDistanceFromCamera = (transform.position - currentCamera.transform.position).sqrMagnitude;

		}

		void InitSurfacesCache()
		{
			if (surfaces != null)
			{
				List<GameObject> cached = new List<GameObject>(surfaces.Values);
				for (int k = 0; k < cached.Count; k++)
				{
					if (cached[k] != null)
						DestroyImmediate(cached[k]);
				}
				surfaces.Clear();
			}
			else
			{
				surfaces = new Dictionary<int, GameObject>();
			}
		}

		void CreateSurfacesLayer()
		{
			Transform t = transform.Find(SURFACE_LAYER);
			if (t != null)
			{
				DestroyImmediate(t.gameObject);
				for (int k = 0; k < countries.Length; k++)
					for (int r = 0; r < countries[k].regions.Count; r++)
						countries[k].regions[r].customMaterial = null;
			}
			_surfacesLayer = new GameObject(SURFACE_LAYER);
			_surfacesLayer.transform.SetParent(transform, false);
			_surfacesLayer.transform.localPosition = Misc.Vector3back * 0.001f;
			_surfacesLayer.layer = gameObject.layer;
		}

		void RestyleEarth()
		{
			if (gameObject == null)
				return;

			string materialName;
			switch (_earthStyle)
			{
				case EARTH_STYLE.Alternate1:
					materialName = "Earth2";
					break;
				case EARTH_STYLE.Alternate2:
					materialName = "Earth4";
					break;
				case EARTH_STYLE.Alternate3:
					materialName = "Earth5";
					break;
				case EARTH_STYLE.SolidColor:
					materialName = "EarthSolidColor";
					break;
				case EARTH_STYLE.NaturalHighRes:
					materialName = "EarthHighRes";
					break;
				case EARTH_STYLE.NaturalScenic:
					materialName = "EarthScenic";
					break;
				case EARTH_STYLE.NaturalHighRes16K:
					materialName = "EarthHighRes16K";
					break;
				case EARTH_STYLE.NaturalScenic16K:
					materialName = "EarthScenic16K";
					break;
				default:
					materialName = "Earth";
					break;
			}
			MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
			if (renderer.sharedMaterial == null || !renderer.sharedMaterial.name.Equals(materialName))
			{
				Material earthMaterial = Instantiate(Resources.Load<Material>("Materials/" + materialName));
				earthMaterial.hideFlags = HideFlags.DontSave;
				if (_earthStyle == EARTH_STYLE.SolidColor)
				{
					earthMaterial.color = _earthColor;
				}
				earthMaterial.name = materialName;
				renderer.material = earthMaterial;
			}
		}

		#endregion



		#region Highlighting

		bool GetLocalHitFromMousePos(out Vector3 localPoint)
		{
			Vector3 mousePos = Input.mousePosition;
			if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
			{
				localPoint = Misc.Vector3zero;
				return false;
			}
			return GetLocalHitFromScreenPos(mousePos, out localPoint);
		}

		/// <summary>
		/// Check mouse hit on the map and return the local plane coordinate. Handles viewports.
		/// </summary>
		public bool GetLocalHitFromScreenPos(Vector3 screenPos, out Vector3 localPoint)
		{
			Ray ray = _mainCamera.ScreenPointToRay(screenPos);
			RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 2000, layerMask);
			if (hits.Length > 0)
			{
				for (int k = 0; k < hits.Length; k++)
				{
					// Hit the map?
					if (hits[k].collider.gameObject == _renderViewport)
					{
						localPoint = _renderViewport.transform.InverseTransformPoint(hits[k].point);
						// Is the viewport a render viewport or the map itself? If it's a render viewport projects hit into mapper cam space
						if (_renderViewport != gameObject)
						{
							// Get plane in screen space
							Vector3 tl = _currentCamera.WorldToScreenPoint(transform.TransformPoint(new Vector3(-0.5f, 0.5f)));
							Vector3 br = _currentCamera.WorldToScreenPoint(transform.TransformPoint(new Vector3(0.5f, -0.5f)));
							// Trace the ray from this position in mapper cam space
							localPoint.x = _currentCamera.pixelWidth * (localPoint.x + 0.5f);
							localPoint.y = _currentCamera.pixelHeight * (localPoint.y + 0.5f);
							if (localPoint.x >= tl.x && localPoint.x <= br.x && localPoint.y >= br.y && localPoint.y <= tl.y)
							{
								localPoint.x = (localPoint.x - tl.x) / (br.x - tl.x) - 0.5f;
								localPoint.y = (localPoint.y - br.y) / (tl.y - br.y) - 0.5f;
								return true;
							}
						}
						else
							return true;
					}
				}
			}
			localPoint = Misc.Vector3zero;
			return false;
		}

		void CheckMousePos()
		{

			if (lastMouseMapHitPosGood)
			{
				// Cursor follow
				if (_cursorFollowMouse)
				{
					cursorLocation = lastMouseMapHitPos;
				}

				// verify if hitPos is inside any country polygon
				int countryCount = _countriesOrderedBySize.Count;
				for (int oc = 0; oc < countryCount; oc++)
				{
					int c = _countriesOrderedBySize[oc];
					Country country = countries[c];
					if (country.hidden || !country.regionsRect2D.Contains(lastMouseMapHitPos))
						continue;
					for (int cr = 0; cr < country.regions.Count; cr++)
					{
						if (country.regions[cr].Contains(lastMouseMapHitPos))
						{
							if (c != _countryHighlightedIndex || (c == _countryHighlightedIndex && cr != _countryRegionHighlightedIndex))
							{
								HighlightCountryRegion(c, cr, false, _showOutline);
								// Raise enter event
								if (OnCountryEnter != null)
									OnCountryEnter(c, cr);
							}
							// if show provinces is enabled, then we draw provinces borders
							if (_showProvinces && _countryHighlighted.provinces != null)
							{
								DrawProvinces(_countryHighlightedIndex, false, false); // draw provinces borders if not drawn
								for (int p = 0; p < _countryHighlighted.provinces.Length; p++)
								{
									// and now, we check if the mouse if inside a province, so highlight it
									Province province = _countryHighlighted.provinces[p];
									if (!province.regionsRect2D.Contains(lastMouseMapHitPos))
										continue;
									int provinceIndex = GetProvinceIndex(province);
									for (int pr = 0; pr < province.regions.Count; pr++)
									{
										if (province.regions[pr].Contains(lastMouseMapHitPos))
										{
											if (provinceIndex != _provinceHighlightedIndex || (provinceIndex == _provinceHighlightedIndex && pr != _provinceRegionHighlightedIndex))
											{
												HighlightProvinceRegion(provinceIndex, pr, false);
												// Raise enter event
												if (OnProvinceEnter != null)
													OnProvinceEnter(provinceIndex, pr);
											}
										}
									}
								}
							}
							// Verify if a city is hit
							if (_showCities)
							{
								int ci = GetCityNearPoint(lastMouseMapHitPos, _countryHighlightedIndex);
								if (ci >= 0)
								{
									if (ci != _cityHighlightedIndex)
									{
										HideCityHighlight();
										HighlightCity(ci);
									}
								}
								else if (_cityHighlightedIndex >= 0)
								{
									HideCityHighlight();
								}
							}
							return;
						}
					}
				}
			}
			HideCountryRegionHighlight();
			if (!_drawAllProvinces)
				HideProvinces();

			// Verify if a standalone city is hit
			if (_showCities)
			{
				int ci = GetCityNearPoint(lastMouseMapHitPos);
				if (ci >= 0)
				{
					if (ci != _cityHighlightedIndex)
					{
						HideCityHighlight();
						HighlightCity(ci);
					}
				}
				else if (_cityHighlightedIndex >= 0)
				{
					HideCityHighlight();
				}
			}
		}

		#endregion

		#region Internal API area

		float ApplyDragThreshold(float value)
		{
			if (_mouseDragThreshold > 0)
			{
				if (value < 0)
				{
					value += _mouseDragThreshold;
					if (value > 0)
						value = 0;
				}
				else
				{
					value -= _mouseDragThreshold;
					if (value < 0)
						value = 0;
				}
			}
			return value;

		}


		/// <summary>
		/// Returns the overlay base layer (parent gameObject), useful to overlay stuff on the map (like labels). It will be created if it doesn't exist.
		/// </summary>
		public GameObject GetOverlayLayer(bool createIfNotExists)
		{
			if (overlayLayer != null)
			{
				return overlayLayer;
			}
			else if (createIfNotExists)
			{
				return CreateOverlay();
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Returns optimum distance between camera and a region of width/height
		/// </summary>
		float GetFrustumZoomLevel(float width, float height)
		{
			if (currentCamera == null)
				return 1;
			float fv = _currentCamera.fieldOfView;
			float aspect = _currentCamera.aspect;
			float radAngle = fv * Mathf.Deg2Rad;
			float distance, frustumDistanceW, frustumDistanceH;
			if (currentCamera.orthographic)
			{
				distance = 1;
			}
			else
			{
				frustumDistanceH = height * 0.5f / Mathf.Tan(radAngle * 0.5f);
				frustumDistanceW = (width / aspect) * 0.5f / Mathf.Tan(radAngle * 0.5f);
				distance = Mathf.Max(frustumDistanceH, frustumDistanceW);
			}
			float referenceDistance = GetZoomLevelDistance(1f);
			return distance / referenceDistance;
		}


		/// <summary>
		/// Returns optimum distance between camera and map having into account fitToWindow options
		/// </summary>
		float GetFrustumDistance()
		{
			if (!gameObject.activeInHierarchy)
				return float.MaxValue;
			if (_mainCamera == null)
				_mainCamera = Camera.main;
			if (_renderViewport == null)
				SetupViewport();
			lastMapPosition = transform.position;
			lastCamPosition = currentCamera.transform.position;
			transform.localScale = new Vector3(mapWidth, mapHeight, 1);
			float fv = currentCamera.fieldOfView;
			float aspect = currentCamera.aspect;
			float radAngle = fv * Mathf.Deg2Rad;
			float distance, frustumDistanceW, frustumDistanceH;
			if (currentCamera.orthographic)
			{
				if (_fitWindowHeight)
				{
					_currentCamera.orthographicSize = mapHeight * 0.5f;
					maxFrustumDistanceSqr = _currentCamera.orthographicSize;
				}
				else if (_fitWindowWidth)
				{
					_currentCamera.orthographicSize = mapWidth * 0.5f / aspect;
					maxFrustumDistanceSqr = _currentCamera.orthographicSize;
				}
				else
				{
					maxFrustumDistanceSqr = float.MaxValue;
				}
				distance = 1;
			}
			else
			{
				frustumDistanceH = mapHeight * 0.5f / Mathf.Tan(radAngle * 0.5f);
				frustumDistanceW = (mapWidth / aspect) * 0.5f / Mathf.Tan(radAngle * 0.5f);
				if (_fitWindowHeight)
				{
					distance = Mathf.Min(frustumDistanceH, frustumDistanceW);
					maxFrustumDistanceSqr = distance * distance;
				}
				else if (_fitWindowWidth)
				{
					distance = Mathf.Max(frustumDistanceH, frustumDistanceW);
					maxFrustumDistanceSqr = distance * distance;
				}
				else
				{
					distance = Vector3.Distance(transform.position, currentCamera.transform.position);
					maxFrustumDistanceSqr = float.MaxValue;
				}
			}
			return distance;
		}

		float GetZoomLevelDistance(float zoomLevel)
		{
			zoomLevel = Mathf.Clamp01(zoomLevel);

			float fv = currentCamera.fieldOfView;
			float radAngle = fv * Mathf.Deg2Rad;
			float aspect = currentCamera.aspect;
			float frustumDistanceH = mapHeight * 0.5f / Mathf.Tan(radAngle * 0.5f);
			float frustumDistanceW = (mapWidth / aspect) * 0.5f / Mathf.Tan(radAngle * 0.5f);
			float distance;
			if (_fitWindowWidth)
			{
				distance = Mathf.Max(frustumDistanceH, frustumDistanceW);
			}
			else
			{
				distance = Mathf.Min(frustumDistanceH, frustumDistanceW);
			}
			return distance * zoomLevel;
		}

		void SetDestination(Vector2 point, float _duration = -1, float _zoomLevel = -1)
		{
            float duration = _duration >= 0 ? _duration : _navigationTime;
            float zoomLevel = _zoomLevel >= 0 ? _zoomLevel : GetZoomLevel();
            // setup lerping parameters
            if (_staticCamera)
			{
				flyToStartQuaternion = transform.rotation;
				flyToStartLocation = transform.position;
				flyToEndQuaternion = currentCamera.transform.rotation;
				flyToEndLocation = transform.TransformPoint(point) - transform.forward * GetZoomLevelDistance(zoomLevel);
			}
			else
			{
				flyToStartQuaternion = _currentCamera.transform.rotation;
				flyToStartLocation = _currentCamera.transform.position;
				flyToEndQuaternion = transform.rotation;
				flyToEndLocation = transform.TransformPoint(point) - transform.forward * GetZoomLevelDistance(zoomLevel);
			}
			flyToDuration = duration;
			flyToActive = true;
			flyToStartTime = DateTime.Now;
			if (flyToDuration == 0)
				MoveToDestination();
		}

        void SetUnityDestination(Vector2 point, float _duration = -1, float _zoomLevel = -1)
        {
            float duration = _duration >= 0 ? _duration : _navigationTime;
            float zoomLevel = _zoomLevel >= 0 ? _zoomLevel : GetZoomLevel();
            // setup lerping parameters
            if (_staticCamera)
            {
                flyToStartQuaternion = transform.rotation;
                flyToStartLocation = transform.position;
                flyToEndQuaternion = currentCamera.transform.rotation;
                flyToEndLocation = transform.TransformPoint(point) - transform.forward * GetZoomLevelDistance(zoomLevel);
            }
            else
            {
                flyToStartQuaternion = _currentCamera.transform.rotation;
                flyToStartLocation = _currentCamera.transform.position;
                flyToEndQuaternion = transform.rotation;
                flyToEndLocation = new Vector3(point.x, point.y, 100) - transform.forward * GetZoomLevelDistance(zoomLevel);
            }
            flyToDuration = duration;
            flyToActive = true;
            flyToStartTime = DateTime.Now;
            if (flyToDuration == 0)
                MoveToDestination();
        }

        /// <summary>
        /// Used internally to rotate the globe during FlyTo operations. Use FlyTo method.
        /// </summary>
        void MoveToDestination()
		{
			float delta;
			Quaternion rotation;
			Vector3 destination;
			if (flyToDuration == 0)
			{
				delta = flyToDuration;
				rotation = flyToEndQuaternion;
				destination = flyToEndLocation;
			}
			else
			{
                delta = (float)(DateTime.Now - flyToStartTime).TotalMilliseconds/1000.0f;
				float t = delta / flyToDuration;
				rotation = Quaternion.Lerp(flyToStartQuaternion, flyToEndQuaternion, Mathf.SmoothStep(0, 1, t));
				destination = Vector3.Lerp(flyToStartLocation, flyToEndLocation, Mathf.SmoothStep(0, 1, t));
			}
			if (_staticCamera)
			{
				transform.rotation = rotation;
				transform.position = destination;
			}
			else
			{
				_currentCamera.transform.rotation = rotation;
				_currentCamera.transform.position = destination;
			}
			if (delta >= flyToDuration)
				flyToActive = false;
		}

		Material GetColoredTexturedMaterial(Color color, Texture2D texture)
		{
			if (texture == null && coloredMatCache.ContainsKey(color))
			{
				return coloredMatCache[color];
			}
			else
			{
				Material customMat;
				if (texture != null)
				{
					customMat = Instantiate(texturizedMat);
					customMat.name = texturizedMat.name;
					customMat.mainTexture = texture;
				}
				else
				{
					customMat = Instantiate(coloredMat);
					customMat.name = coloredMat.name;
					coloredMatCache[color] = customMat;
				}
				customMat.color = color;
				customMat.hideFlags = HideFlags.DontSave;
                customMat.shader = Shader.Find("Transparent/Diffuse");
                return customMat;
			}
		}

		void ApplyMaterialToSurface(GameObject obj, Material sharedMaterial)
		{
			if (obj != null)
			{
				Renderer[] rr = obj.GetComponentsInChildren<Renderer>(true);    // surfaces can be saved under parent when Include All Regions is enabled
				for (int k = 0; k < rr.Length; k++)
				{
					if (rr[k].sharedMaterial != outlineMat)
					{
						rr[k].sharedMaterial = sharedMaterial;
					}
				}
			}
		}

		void GetPointFromPackedString(string s, out float x, out float y)
		{
			int j = s.IndexOf(",");
			string sx = s.Substring(0, j);
			string sy = s.Substring(j + 1);
			x = float.Parse(sx, CultureInfo.InvariantCulture) / MAP_PRECISION;
			y = float.Parse(sy, CultureInfo.InvariantCulture) / MAP_PRECISION;
		}



		#endregion

		#region World Gizmos

		void DrawCursor()
		{

			if (!_showCursor)
				return;

			// Generate line V **********************
			Vector3[] points = new Vector3[2];
			int[] indices = new int[2];
			indices[0] = 0;
			indices[1] = 1;
			points[0] = Misc.Vector3up * -0.5f;
			points[1] = Misc.Vector3up * 0.5f;

			Transform t = transform.Find("CursorV");
			if (t != null)
				DestroyImmediate(t.gameObject);
			cursorLayerVLine = new GameObject("CursorV");
			cursorLayerVLine.hideFlags = HideFlags.DontSave;
			cursorLayerVLine.transform.SetParent(transform, false);
			cursorLayerVLine.transform.localPosition = Misc.Vector3zero;
			cursorLayerVLine.transform.localRotation = Quaternion.Euler(Misc.Vector3zero);
			cursorLayerVLine.layer = gameObject.layer;
			cursorLayerVLine.SetActive(_showCursor);

			Mesh mesh = new Mesh();
			mesh.vertices = points;
			mesh.SetIndices(indices, MeshTopology.Lines, 0);
			mesh.RecalculateBounds();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = cursorLayerVLine.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh;

			MeshRenderer mr = cursorLayerVLine.AddComponent<MeshRenderer>();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			//			mr.useLightProbes = false;
			mr.sharedMaterial = cursorMatV;


			// Generate line H **********************
			points[0] = Misc.Vector3right * -0.5f;
			points[1] = Misc.Vector3right * 0.5f;

			t = transform.Find("CursorH");
			if (t != null)
				DestroyImmediate(t.gameObject);
			cursorLayerHLine = new GameObject("CursorH");
			cursorLayerHLine.hideFlags = HideFlags.DontSave;
			cursorLayerHLine.transform.SetParent(transform, false);
			cursorLayerHLine.transform.localPosition = Misc.Vector3zero;
			cursorLayerHLine.transform.localRotation = Quaternion.Euler(Misc.Vector3zero);
			cursorLayerHLine.layer = gameObject.layer;
			cursorLayerHLine.SetActive(_showCursor);

			mesh = new Mesh();
			mesh.vertices = points;
			mesh.SetIndices(indices, MeshTopology.Lines, 0);
			mesh.RecalculateBounds();
			mesh.hideFlags = HideFlags.DontSave;

			mf = cursorLayerHLine.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh;

			mr = cursorLayerHLine.AddComponent<MeshRenderer>();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			//			mr.useLightProbes = false;
			mr.sharedMaterial = cursorMatH;


		}

		void DrawGrid()
		{
			DrawLatitudeLines();
			DrawLongitudeLines();
		}

		void DrawLatitudeLines()
		{
			if (!_showLatitudeLines)
				return;

			// Generate latitude lines
			List<Vector3> points = new List<Vector3>();
			List<int> indices = new List<int>();
			float r = 0.5f;
			int idx = -1;

			for (float a = 0; a < 90; a += _latitudeStepping)
			{
				for (int h = 1; h >= -1; h--)
				{
					if (h == 0)
						continue;
					float y = h * a / 90.0f * r;
					points.Add(new Vector3(-r, y, 0));
					points.Add(new Vector3(r, y, 0));
					indices.Add(++idx);
					indices.Add(++idx);
					if (a == 0)
						break;
				}
			}

			Transform t = transform.Find("LatitudeLines");
			if (t != null)
				DestroyImmediate(t.gameObject);
			latitudeLayer = new GameObject("LatitudeLines");
			latitudeLayer.hideFlags = HideFlags.DontSave;
			latitudeLayer.transform.SetParent(transform, false);
			latitudeLayer.transform.localPosition = Misc.Vector3zero;
			latitudeLayer.transform.localRotation = Quaternion.Euler(Misc.Vector3zero);
			latitudeLayer.layer = gameObject.layer;
			latitudeLayer.SetActive(_showLatitudeLines);

			Mesh mesh = new Mesh();
			mesh.vertices = points.ToArray();
			mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
			mesh.RecalculateBounds();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = latitudeLayer.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh;

			MeshRenderer mr = latitudeLayer.AddComponent<MeshRenderer>();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			//			mr.useLightProbes = false;
			mr.sharedMaterial = gridMat;

		}

		void DrawLongitudeLines()
		{
			if (!_showLongitudeLines)
				return;

			// Generate longitude lines
			List<Vector3> points = new List<Vector3>();
			List<int> indices = new List<int>();
			float r = 0.5f;
			int idx = -1;
			int step = 180 / _longitudeStepping;

			for (float a = 0; a < 90; a += step)
			{
				for (int h = 1; h >= -1; h--)
				{
					if (h == 0)
						continue;
					float x = h * a / 90.0f * r;
					points.Add(new Vector3(x, -r, 0));
					points.Add(new Vector3(x, r, 0));
					indices.Add(++idx);
					indices.Add(++idx);
					if (a == 0)
						break;
				}
			}


			Transform t = transform.Find("LongitudeLines");
			if (t != null)
				DestroyImmediate(t.gameObject);
			longitudeLayer = new GameObject("LongitudeLines");
			longitudeLayer.hideFlags = HideFlags.DontSave;
			longitudeLayer.transform.SetParent(transform, false);
			longitudeLayer.transform.localPosition = Misc.Vector3zero;
			longitudeLayer.transform.localRotation = Quaternion.Euler(Misc.Vector3zero);
			longitudeLayer.layer = gameObject.layer;
			longitudeLayer.SetActive(_showLongitudeLines);

			Mesh mesh = new Mesh();
			mesh.vertices = points.ToArray();
			mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
			mesh.RecalculateBounds();
			mesh.hideFlags = HideFlags.DontSave;

			MeshFilter mf = longitudeLayer.AddComponent<MeshFilter>();
			mf.sharedMesh = mesh;

			MeshRenderer mr = longitudeLayer.AddComponent<MeshRenderer>();
			mr.receiveShadows = false;
			mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.sharedMaterial = gridMat;

		}

		#endregion

		#region Overlay & Render viewport

		public GameObject CreateOverlay()
		{
#if TRACE_CTL
			Debug.Log ("CTL " + DateTime.Now + ": CreateOverlay");
#endif

			if (!gameObject.activeInHierarchy)
				return null;

			// 2D labels layer
			Transform t = transform.Find(OVERLAY_BASE);
			if (t == null)
			{
				overlayLayer = new GameObject(OVERLAY_BASE);
				overlayLayer.hideFlags = HideFlags.DontSave;
				overlayLayer.transform.SetParent(transform, false);
				overlayLayer.transform.localPosition = Misc.Vector3back * 0.002f;
				overlayLayer.transform.localScale = Misc.Vector3one;
				overlayLayer.layer = gameObject.layer;
			}
			else
			{
				overlayLayer = t.gameObject;
				overlayLayer.SetActive(true);
			}
			return overlayLayer;
		}

		void SetupViewport()
		{

			if (!gameObject.activeInHierarchy)
			{
				_currentCamera = _mainCamera;
				_renderViewport = gameObject;
				return;
			}
			if (_renderViewport == null)
				_renderViewport = gameObject;

			if (_renderViewport == gameObject)
			{
				if (overlayRT != null)
				{
					if (_currentCamera != null)
						_currentCamera.targetTexture = null;
					RenderTexture.active = null;
					overlayRT.Release();
					DestroyImmediate(overlayRT);
					overlayRT = null;
				}
				if (this.overlayLayer != null)
				{
					DestroyMapperCam();
				}
				_currentCamera = _mainCamera;
				return;
			}

			// Setup Render texture
			int imageWidth, imageHeight;
			imageWidth = 2048;
			imageHeight = 1024;
			if (overlayRT != null && (overlayRT.width != imageWidth || overlayRT.height != imageHeight || overlayRT.filterMode != _renderViewportFilterMode))
			{
				if (_currentCamera != null)
					_currentCamera.targetTexture = null;
				RenderTexture.active = null;
				overlayRT.Release();
				DestroyImmediate(overlayRT);
				overlayRT = null;
			}

			GameObject overlayLayer = GetOverlayLayer(true);
			if (overlayRT == null)
			{
				overlayRT = new RenderTexture(imageWidth, imageHeight, 0);
				overlayRT.hideFlags = HideFlags.DontSave;
				overlayRT.filterMode = _renderViewportFilterMode; // FilterMode.Trilinear; -> trilinear causes blurry issues with NGUI
				overlayRT.anisoLevel = 0;
				overlayRT.useMipMap = (_renderViewportFilterMode == FilterMode.Trilinear);
			}

			// Camera
			mapperCamName = MAPPER_CAM_PREFIX + "_" + gameObject.name;
			GameObject camObj = GameObject.Find(mapperCamName);
			if (camObj == null)
			{
				camObj = new GameObject(mapperCamName, typeof(Camera));
				camObj.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
				camObj.layer = overlayLayer.layer;
			}
			mapperCam = camObj.GetComponent<Camera>();
			CheckViewportScale();
			mapperCam.cullingMask = 1 << camObj.layer;
			mapperCam.clearFlags = CameraClearFlags.SolidColor;
			mapperCam.backgroundColor = new Color(0, 0, 0, 0);
			mapperCam.targetTexture = overlayRT;
			mapperCam.nearClipPlane = 0.01f;
			mapperCam.farClipPlane = 1000;
			mapperCam.enabled = true;
			mapperCam.targetTexture = overlayRT;
			if (_currentCamera != mapperCam)
			{
				_currentCamera = mapperCam;
				if (transform.position.x < 5000)
				{
					transform.position = new Vector3(5000, 1000, 5000); // moves the main gameobject away: note: can't use 10000,10000,10000 for precision problems
				}
				_currentCamera.transform.position = transform.position + Misc.Vector3back * 86.5f; // default camera position for a standard height of 100
				CenterMap();
			}
			// Assigns render texture to current material and recreates the camera
			Renderer rvRenderer = _renderViewport.GetComponent<Renderer>();
			if (rvRenderer.sharedMaterial == null)
			{
				rvRenderer.sharedMaterial = Resources.Load<Material>("Materials/ViewportMaterial");
			}
			Material rvMat = Instantiate(rvRenderer.sharedMaterial);
			rvMat.hideFlags = HideFlags.DontSave;
			rvMat.mainTexture = overlayRT;
			rvRenderer.sharedMaterial = rvMat;
			// Assign mouse interaction proxy on viewport
			PointerTrigger pt = _renderViewport.GetComponent<PointerTrigger>() ?? _renderViewport.AddComponent<PointerTrigger>();
			pt.map = this;

			// Position the viewport if overlay mode is enabled
			if (_renderViewportScreenOverlay)
			{
				_renderViewport.transform.SetParent(_mainCamera.transform, false);
				_renderViewport.transform.localRotation = Quaternion.Euler(0, 0, 0);
				float dist = (_mainCamera.nearClipPlane + 0.1f);
				Vector3 bl = _mainCamera.ViewportToWorldPoint(new Vector3(_renderViewportScreenRect.xMin, _renderViewportScreenRect.yMin, dist));
				Vector3 tr = _mainCamera.ViewportToWorldPoint(new Vector3(_renderViewportScreenRect.xMax, _renderViewportScreenRect.yMax, dist));
				_renderViewport.transform.localScale = new Vector3(tr.x - bl.x, tr.y - bl.y, 1);
				Vector3 vc = _mainCamera.transform.InverseTransformPoint((bl + tr) * 0.5f);
				_renderViewport.transform.localPosition = vc;
			}
			else
			{
				if (_renderViewport.transform.parent == _mainCamera.transform)
				{
					_renderViewport.transform.SetParent(null, true);
				}
			}
		}

		void DestroyMapperCam()
		{
			if (mapperCam != null)
			{
				DestroyImmediate(mapperCam.gameObject);
				mapperCam = null;
				if (!string.IsNullOrEmpty(mapperCamName))
				{
					GameObject o = GameObject.Find(mapperCamName);
					if (o != null)
						DestroyImmediate(o);
				}
			}
		}

		/// <summary>
		/// Ensure the proportions of the main map fit the aspect ratio of the render viewport
		/// </summary>
		void CheckViewportScale()
		{
			if (_renderViewport == null || _renderViewport == gameObject)
				return;
			float aspect = _renderViewport.transform.lossyScale.x / _renderViewport.transform.lossyScale.y;
			if (mapperCam != null)
				mapperCam.aspect = aspect;
		}

		#endregion

		#region Markers support

		void CheckMarkersLayer()
		{
			if (markersLayer == null)
			{ // try to capture an existing marker layer
				Transform t = transform.Find("Markers");
				if (t != null)
				{
					markersLayer = t.gameObject;
					markersLayer.layer = mapUnityLayer;
				}
			}
			if (markersLayer == null)
			{ // create it otherwise
				markersLayer = new GameObject("Markers");
				markersLayer.transform.SetParent(transform, false);
				markersLayer.layer = mapUnityLayer;
			}
		}


		#endregion


	}

}