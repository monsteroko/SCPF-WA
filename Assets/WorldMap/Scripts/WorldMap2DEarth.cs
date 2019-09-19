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
	public enum EARTH_STYLE
	{
		Natural = 0,
		Alternate1= 1,
		Alternate2= 2,
		Alternate3= 3,
		SolidColor = 4,
		NaturalHighRes = 5,
		NaturalScenic = 6,
		NaturalHighRes16K = 7,
		NaturalScenic16K = 8
	}

	public partial class WorldMap2D : MonoBehaviour
	{

		#region Public properties

		[SerializeField]
		bool
			_showWorld = true;
		/// <summary>
		/// Toggle Earth visibility.
		/// </summary>
		public bool showEarth { 
			get {
				return _showWorld; 
			}
			set {
				if (value != _showWorld) {
					_showWorld = value;
					isDirty = true;
					gameObject.GetComponent<MeshRenderer> ().enabled = _showWorld;
				}
			}
		}

		[SerializeField]
		EARTH_STYLE
			_earthStyle = EARTH_STYLE.Natural;

		public EARTH_STYLE earthStyle {
			get {
				return _earthStyle;
			}
			set {
				if (value != _earthStyle) {
					_earthStyle = value;
					isDirty = true;
					RestyleEarth ();
				}
			}
		}


		/// <summary>
		/// Color for Earth (for SolidColor style)
		/// </summary>
		[SerializeField]
		Color
			_earthColor = Color.black;
		
		public Color earthColor {
			get {
				return _earthColor;
			}
			set {
				if (value != _earthColor) {
					_earthColor = value;
					isDirty = true;

					if (_earthStyle == EARTH_STYLE.SolidColor) {
						Material mat = GetComponent<Renderer> ().sharedMaterial;
						mat.color = _earthColor;
					}
				}
			}
		}

	#endregion

	}

}