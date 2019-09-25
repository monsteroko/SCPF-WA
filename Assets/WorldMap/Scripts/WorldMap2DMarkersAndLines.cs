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

	public partial class WorldMap2D : MonoBehaviour
	{

		#region Public properties

		[SerializeField]
		bool
			_showCursor = true;
		
		/// <summary>
		/// Toggle cursor lines visibility.
		/// </summary>
		public bool showCursor { 
			get {
				return _showCursor; 
			}
			set {
				if (value != _showCursor) {
					_showCursor = value;
					isDirty = true;

					if (cursorLayerVLine != null) {
						cursorLayerVLine.SetActive (_showCursor);
					}
					if (cursorLayerHLine != null) {
						cursorLayerHLine.SetActive (_showCursor);
					}
				}
			}
		}

		/// <summary>
		/// Cursor lines color.
		/// </summary>
		[SerializeField]
		Color
			_cursorColor = new Color (0.56f, 0.47f, 0.68f);
		
		public Color cursorColor {
			get {
				if (cursorMatH != null) {
					return cursorMatH.color;
				} else {
					return _cursorColor;
				}
			}
			set {
				if (value != _cursorColor) {
					_cursorColor = value;
					isDirty = true;
					
					if (cursorMatH != null && _cursorColor != cursorMatH.color) {
						cursorMatH.color = _cursorColor;
					}
					if (cursorMatV != null && _cursorColor != cursorMatV.color) {
						cursorMatV.color = _cursorColor;
					}
				}
			}
		}

		[SerializeField]
		bool
			_cursorFollowMouse = true;
		
		/// <summary>
		/// Makes the cursor follow the mouse when it's over the World.
		/// </summary>
		public bool cursorFollowMouse { 
			get {
				return _cursorFollowMouse; 
			}
			set {
				if (value != _cursorFollowMouse) {
					_cursorFollowMouse = value;
					isDirty = true;
				}
			}
		}

		Vector3
			_cursorLocation;

		public Vector3
			cursorLocation {
			get {
				return _cursorLocation;
			}
			set {
				if (_cursorLocation != value) {
					_cursorLocation = value;
					if (_showCursor) {
						if (cursorLayerVLine != null) {
							cursorLayerVLine.transform.localPosition = transform.right * _cursorLocation.x;
						}
						if (cursorLayerHLine != null) {
							cursorLayerHLine.transform.localPosition = transform.up * _cursorLocation.y;
						}
					}
				}
			}
		}

		[SerializeField]
		bool
			_showLatitudeLines = true;
		
		/// <summary>
		/// Toggle latitude lines visibility.
		/// </summary>
		public bool showLatitudeLines { 
			get {
				return _showLatitudeLines; 
			}
			set {
				if (value != _showLatitudeLines) {
					_showLatitudeLines = value;
					isDirty = true;
					
					if (latitudeLayer != null) {
						latitudeLayer.SetActive (_showLatitudeLines);
					} else if (_showLatitudeLines) {
						DrawLatitudeLines();
					}
				}
			}
		}

		[SerializeField]
		[Range(5.0f, 45.0f)]
		int
			_latitudeStepping = 15;
		/// <summary>
		/// Specify latitude lines separation.
		/// </summary>
		public int latitudeStepping { 
			get {
				return _latitudeStepping; 
			}
			set {
				if (value != _latitudeStepping) {
					_latitudeStepping = value;
					isDirty = true;

					if (gameObject.activeInHierarchy)
						DrawLatitudeLines ();
				}
			}
		}

		[SerializeField]
		bool
			_showLongitudeLines = true;
		
		/// <summary>
		/// Toggle longitude lines visibility.
		/// </summary>
		public bool showLongitudeLines { 
			get {
				return _showLongitudeLines; 
			}
			set {
				if (value != _showLongitudeLines) {
					_showLongitudeLines = value;
					isDirty = true;
					
					if (longitudeLayer != null) {
						longitudeLayer.SetActive (_showLongitudeLines);
					} else if (_showLongitudeLines) {
						DrawLongitudeLines();
					}
				}
			}
		}
		
		[SerializeField]
		[Range(5.0f, 45.0f)]
		int
			_longitudeStepping = 15;
		/// <summary>
		/// Specify longitude lines separation.
		/// </summary>
		public int longitudeStepping { 
			get {
				return _longitudeStepping; 
			}
			set {
				if (value != _longitudeStepping) {
					_longitudeStepping = value;
					isDirty = true;

					if (gameObject.activeInHierarchy)
						DrawLongitudeLines ();
				}
			}
		}

		/// <summary>
		/// Color for imaginary lines (longitude and latitude).
		/// </summary>
		[SerializeField]
		Color
			_gridColor = new Color (0.16f, 0.33f, 0.498f);
		
		public Color gridLinesColor {
			get {
				if (gridMat != null) {
					return gridMat.color;
				} else {
					return _gridColor;
				}
			}
			set {
				if (value != _gridColor) {
					_gridColor = value;
					isDirty = true;
					
					if (gridMat != null && _gridColor != gridMat.color) {
						gridMat.color = _gridColor;
					}
				}
			}
		}

	#endregion

	#region Public API area
	
		/// <summary>
		/// Adds a custom marker (gameobject) to the globe on specified location and with custom scale.
		/// </summary>
		public void AddMarker (GameObject marker, Vector3 planeLocation, float markerScale)
		{
			// Try to get the height of the object
			float height = 0;
			if (marker.GetComponent<MeshFilter> () != null)
				height = marker.GetComponent<MeshFilter> ().sharedMesh.bounds.size.y;
			else if (marker.GetComponent<Collider> () != null) 
				height = marker.GetComponent<Collider> ().bounds.size.y;
			
			float h = height * markerScale / planeLocation.magnitude; // lift the marker so it appears on the surface of the globe
			
			CheckMarkersLayer ();
			marker.transform.SetParent (markersLayer.transform, false);
			marker.transform.localPosition = planeLocation + Misc.Vector3back * (0.001f + h * 0.5f);
			marker.layer = mapUnityLayer;

			// apply custom scale
			float prop = mapWidth / mapHeight;
			marker.transform.localScale = new Vector3 (markerScale, prop * markerScale, markerScale);
		}
		
		/// <summary>
		/// Adds a line to the 2D map with options (returns the line gameobject).
		/// </summary>
		/// <param name="start">starting location on the plane (-0.5, -0.5)-(0.5,0.5)</param>
		/// <param name="end">end location on the plane (-0.5, -0.5)-(0.5,0.5)</param>
		/// <param name="Color">line color</param>
		/// <param name="arcElevation">arc elevation (-0.5 .. 0.5)</param>
		/// <param name="duration">drawing speed (0 for instant drawing)</param>
		/// <param name="fadeOutAfter">duration of the line once drawn after which it fades out (set this to zero to make the line stay)</param>
		public GameObject AddLine (Vector2 start, Vector2 end, Color color, float arcElevation, float duration, float lineWidth, float fadeOutAfter)
		{
			CheckMarkersLayer ();
			GameObject newLine = new GameObject ("MarkerLine");
			newLine.transform.SetParent (markersLayer.transform, false);
			newLine.layer = markersLayer.layer;
			LineMarkerAnimator lma = newLine.AddComponent<LineMarkerAnimator> ();
			lma.start = start;
			lma.end = end;
			lma.color = color;
			lma.arcElevation = arcElevation;
			lma.duration = duration;
			lma.lineWidth = lineWidth;
			lma.lineMaterial = lineMarkerMat;
			lma.autoFadeAfter = fadeOutAfter;
			return newLine;
		}
		
		/// <summary>
		/// Deletes all custom markers and lines
		/// </summary>
		public void ClearMarkers ()
		{
			if (markersLayer == null)
				return;
			Destroy (markersLayer);
		}
		
		
		/// <summary>
		/// Removes all marker lines.
		/// </summary>
		public void ClearLineMarkers ()
		{
			if (markersLayer == null)
				return;
			LineRenderer[] t = markersLayer.transform.GetComponentsInChildren<LineRenderer> ();
			for (int k=0; k<t.Length; k++)
				Destroy (t [k].gameObject);
		}
	
		#endregion

	}

}