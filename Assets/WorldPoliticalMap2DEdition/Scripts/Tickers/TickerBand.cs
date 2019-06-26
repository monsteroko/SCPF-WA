using UnityEngine;
using System;
using System.Collections;

namespace WPMF {

	[Serializable]
	public class TickerBand {
		/// <summary>
		/// The vertical offset.  -0.5f .. 0.5f
		/// </summary>
		[Range(-0.5f,0.5f)]
		public float verticalOffset;	

		/// <summary>
		/// 0.01f .. 1.0f
		/// </summary>
		[Range(-0.01f,1.0f)]
		public float verticalSize;	

		/// <summary>
		/// Background color for the ticker band
		/// </summary>
		public Color backgroundColor = new Color(0,0,0,0.9f);  

		/// <summary>
		/// If false, will automatically hide if not texts is available on this ticker
		/// </summary>
		public bool autoHide = true;		

		/// <summary>
		/// Reference to the TextMesh object
		/// </summary>
		[NonSerialized]
		public GameObject gameObject;

		public bool visible;

		/// <summary>
		/// Set it to 0 to disable scrolling (text will remain fixed for "duration" seconds)
		/// </summary>
		public float scrollSpeed = -0.1f;

		/// <summary>
		/// Fade speed for the ticker band. Set it to 0 to disable fade (instantly appears/disappears)
		/// </summary>
		public float fadeSpeed = 0.5f;

		/// <summary>
		/// Used internally to control the smooth appearance of the ticker band.
		/// </summary>
		[NonSerialized]
		public float alphaStart;
		[NonSerialized]
		public DateTime alphaTimer;
		[NonSerialized]
		public bool alphaChanging;
	}
}