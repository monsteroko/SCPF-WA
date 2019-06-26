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
	public delegate void OnSimpleMapEvent();

	public partial class WorldMap2D : MonoBehaviour
	{

		#region Public properties

		public event OnSimpleMapEvent OnDragStart;
		public event OnSimpleMapEvent OnDragEnd;

		/// <summary>
		/// Returns true is mouse has entered the Earth's collider.
		/// </summary>
		[NonSerialized]
		public bool
			mouseIsOver;

		/// <summary>
		/// The navigation time in seconds.
		/// </summary>
		[SerializeField]
		[Range(1.0f, 16.0f)]
		float
			_navigationTime = 4.0f;
		
		public float navigationTime {
			get {
				return _navigationTime;
			}
			set {
				if (_navigationTime != value) {
					_navigationTime = value;
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Returns whether a navigation is taking place at this moment.
		/// </summary>
		public bool isFlying { get { return flyToActive; } }

		[SerializeField]
		bool
			_fitWindowWidth = true;
		/// <summary>
		/// Ensure the map is always visible and occupy the entire Window.
		/// </summary>
		public bool fitWindowWidth { 
			get {
				return _fitWindowWidth; 
			}
			set {
				if (value != _fitWindowWidth) {
					_fitWindowWidth = value;
					isDirty = true;
					CenterMap ();
				}
			}
		}

		[SerializeField]
		bool
			_fitWindowHeight = true;
		/// <summary>
		/// Ensure the map is always visible and occupy the entire Window.
		/// </summary>
		public bool fitWindowHeight { 
			get {
				return _fitWindowHeight; 
			}
			set {
				if (value != _fitWindowHeight) {
					_fitWindowHeight = value;
					isDirty = true;
					CenterMap ();
				}
			}
		}

		[SerializeField]
		bool
			_allowUserKeys = false;
		
		public bool	allowUserKeys {
			get { return _allowUserKeys; }
			set {
				if (_allowUserKeys!=value) {
					_allowUserKeys = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_dragFlipDirection = false;
		
		public bool	dragFlipDirection {
			get { return _dragFlipDirection; }
			set {
				if (_dragFlipDirection!=value) {
					_dragFlipDirection = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_dragConstantSpeed = false;
		
		public bool	dragConstantSpeed {
			get { return _dragConstantSpeed; }
			set {
				if (_dragConstantSpeed!=value) {
					_dragConstantSpeed = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_allowUserDrag = true;
		
		public bool	allowUserDrag {
			get { return _allowUserDrag; }
			set {
				if (_allowUserDrag!=value) {
					_allowUserDrag = value;
					isDirty = true;
				}
			}
		}

		
		
		[SerializeField]
		bool
			_allowScrollOnScreenEdges = false;
		
		public bool	allowScrollOnScreenEdges {
			get { return _allowScrollOnScreenEdges; }
			set {
				if (_allowScrollOnScreenEdges!=value) {
					_allowScrollOnScreenEdges = value;
					isDirty = true;
				}
			}
		}

		
		[SerializeField]
		int
			_screenEdgeThickness = 2;
		
		public int	screenEdgeThickness {
			get { return _screenEdgeThickness; }
			set {
				if (_screenEdgeThickness!=value) {
					_screenEdgeThickness = value;
					isDirty = true;
				}
			}
		}

		
		[SerializeField]
		bool
			_centerOnRightClick = true;
		
		public bool	centerOnRightClick {
			get { return _centerOnRightClick; }
			set {
				if (_centerOnRightClick!=value) {
					_centerOnRightClick = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		bool
			_allowUserZoom = true;
		
		public bool allowUserZoom {
			get { return _allowUserZoom; }
			set {
				if (_allowUserZoom!=value) {
					_allowUserZoom = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		float _zoomMaxDistance = 10;
		public float zoomMaxDistance {
			get { return _zoomMaxDistance; }
			set {
				if (value != _zoomMaxDistance) {
					_zoomMaxDistance = value;
					isDirty = true;
				}
			}
		}
		
		[SerializeField]
		float _zoomMinDistance = 0.01f;
		public float zoomMinDistance {
			get { return _zoomMinDistance; }
			set {
				if (value != _zoomMinDistance) {
					_zoomMinDistance = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_invertZoomDirection = false;
		
		public bool invertZoomDirection {
			get { return _invertZoomDirection; }
			set {
				if (value != _invertZoomDirection) {
					_invertZoomDirection = value;
					isDirty = true;
				}
			}
		}

		
		[SerializeField]
		bool
			_zoomConstantSpeed = false;
		
		public bool	zoomConstantSpeed {
			get { return _zoomConstantSpeed; }
			set {
				if (_zoomConstantSpeed!=value) {
					_zoomConstantSpeed = value;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		[Range(0.1f, 3)]
		float
			_mouseWheelSensitivity = 0.5f;
		
		public float mouseWheelSensitivity {
			get { return _mouseWheelSensitivity; }
			set {
				if (_mouseWheelSensitivity!=value) {
					_mouseWheelSensitivity = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		[Range(0.1f, 3)]
		float
			_mouseDragSensitivity = 0.5f;
		
		public float mouseDragSensitivity {
			get { return _mouseDragSensitivity; }
			set {
				if (_mouseDragSensitivity!=value) {
					_mouseDragSensitivity = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		int
			_mouseDragThreshold = 0;
		
		public int mouseDragThreshold {
			get { return _mouseDragThreshold; }
			set {
				if (_mouseDragThreshold!=value) {
					_mouseDragThreshold = value;
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_staticCamera = false;

		/// <summary>
		/// If set to true, it's the map that moves instead of camera when user drags or zooms in/out
		/// </summary>
		public bool staticCamera {
			get { return _staticCamera; }
			set {
				if (value != _staticCamera) {
					_staticCamera = value;
					isDirty = true;
				}
			}
		}

	#endregion

	#region Public API area

		/// <summary>
		/// Moves the map in front of the camera so it fits the viewport.
		/// </summary>
		public void CenterMap ()
		{
			float distance = GetFrustumDistance();
			if (_staticCamera) {
				transform.rotation = currentCamera.transform.rotation;
				transform.position = currentCamera.transform.position + currentCamera.transform.forward * distance;
			} else {
				_currentCamera.transform.rotation = transform.rotation;
				_currentCamera.transform.position = transform.position - currentCamera.transform.forward * distance;
			}
		}


		
		/// <summary>
		/// Sets the zoom level
		/// </summary>
		/// <param name="zoomLevel">Value from 0 to 1</param>
		public void SetZoomLevel (float zoomLevel)
		{
			zoomLevel = Mathf.Clamp01 (zoomLevel);
			// Gets the max distance from the map
			lastMapPosition = transform.position;
			lastCamPosition = currentCamera.transform.position;
			float fv = currentCamera.fieldOfView;
			float aspect = currentCamera.aspect; 
			float radAngle = fv * Mathf.Deg2Rad;
			float distance, frustumDistanceW, frustumDistanceH;

			if (currentCamera.orthographic) {
				if (_fitWindowWidth) {
					frustumDistanceH = mapWidth * 0.5f / aspect;
				} else {
					frustumDistanceH = mapHeight * 0.5f;
				}
				currentCamera.orthographicSize = Mathf.Max(frustumDistanceH * zoomLevel, 0.001f);
			} else {
				frustumDistanceH = mapHeight * 0.5f / Mathf.Tan (radAngle * 0.5f);
				frustumDistanceW = (mapWidth / aspect) * 0.5f / Mathf.Tan (radAngle * 0.5f);
				if (_fitWindowWidth) {
					distance = Mathf.Max (frustumDistanceH, frustumDistanceW);
				} else {
					distance = Mathf.Min (frustumDistanceH, frustumDistanceW);
				}
				// Takes the distance from the focus point and adjust it according to the zoom level
				Vector3 dest;
				GetLocalHitFromScreenPos (new Vector3 (Screen.width * 0.5f, Screen.height * 0.5f), out dest);
				if (dest != Misc.Vector3zero)
					dest = transform.TransformPoint (dest);
				else
					dest = transform.position;
				currentCamera.transform.position = dest - (dest - currentCamera.transform.position).normalized * distance * zoomLevel;
				float minDistance = 0.01f * transform.localScale.y;
				float camDistance = (dest - currentCamera.transform.position).sqrMagnitude;
				// Last distance
				lastDistanceFromCamera = camDistance; 
				if (camDistance < minDistance) {
					currentCamera.transform.position = dest - transform.forward * minDistance;
				}
			}
		}

		/// <summary>
		/// Gets the current zoom level (0..1)
		/// </summary>
		public float GetZoomLevel ()
		{
			// Gets the max distance from the map
			float fv = currentCamera.fieldOfView;
			float aspect = currentCamera.aspect; 
			float distance, frustumDistanceW, frustumDistanceH;

			if (currentCamera.orthographic) {
				if (_fitWindowWidth) {
					frustumDistanceH = mapWidth * 0.5f / aspect;
				} else {
					frustumDistanceH = mapHeight * 0.5f;
				}
				return currentCamera.orthographicSize / frustumDistanceH;
			}

			float radAngle = fv * Mathf.Deg2Rad;
			frustumDistanceH = mapHeight * 0.5f / Mathf.Tan (radAngle * 0.5f);
			frustumDistanceW = (mapWidth / aspect) * 0.5f / Mathf.Tan (radAngle * 0.5f);
			if (_fitWindowWidth) {
				distance = Mathf.Max (frustumDistanceH, frustumDistanceW);
			} else {
				distance = Mathf.Min (frustumDistanceH, frustumDistanceW);
			}
			// Takes the distance from the focus point and adjust it according to the zoom level
			Vector3 dest;
			GetLocalHitFromScreenPos (new Vector3 (Screen.width * 0.5f, Screen.height * 0.5f), out dest);
			if (dest != Misc.Vector3zero)
				dest = transform.TransformPoint (dest);
			else
				dest = transform.position;
			
			return (currentCamera.transform.position - dest).magnitude / distance;
		}

		/// <summary>
		/// Starts navigation to target location in local 2D coordinates.
		/// </summary>
		public void FlyToLocation (Vector2 destination)
		{
			FlyToLocation (destination.x, destination.y, _navigationTime);
		}
		
		/// <summary>
		/// Starts navigation to target location in local 2D coordinates.
		/// </summary>
		public void FlyToLocation (Vector2 destination, float duration)
		{
			FlyToLocation (destination.x, destination.y, duration);
		}
		
		/// <summary>
		/// Starts navigation to target location in local 2D coordinates.
		/// </summary>
		public void FlyToLocation (float x, float y)
		{
			FlyToLocation (x, y, _navigationTime);
		}
		
		/// <summary>
		/// Starts navigation to target location in local 2D coordinates.
		/// </summary>
		public void FlyToLocation (float x, float y, float duration)
		{
			SetDestination (new Vector2 (x, y), duration);
		}

		#endregion

	}

}