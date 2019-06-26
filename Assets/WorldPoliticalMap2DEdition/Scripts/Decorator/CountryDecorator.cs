using UnityEngine;
using System;
using System.Collections;

namespace WPMF {
	[Serializable]
	public class CountryDecorator {

		/// <summary>
		/// The name of the country which is being decorated.
		/// </summary>
		public string countryName;

		/// <summary>
		/// If country is completely hidden.
		/// </summary>
		public bool hidden = false;

		/// <summary>
		/// Custom label that replaces region name
		/// </summary>
		public string customLabel = "";

		/// <summary>
		/// The fill color
		/// </summary>
		public Color fillColor = Color.white;

		/// <summary>
		/// If the region is colorized with fillColor.
		/// </summary>
		public bool isColorized;

		/// <summary>
		/// If the label has its own color different from general property.
		/// </summary>
		public bool labelOverridesColor = false;

		/// <summary>
		/// Optional label color (labelOverridesColor must be true to have effect)
		/// </summary>
		public Color labelColor = Color.yellow;

		/// <summary>
		/// Whether the country label will be printed or not.
		/// </summary>
		public bool labelVisible = true;
		
		/// <summary>
		/// Manual offset of the label with respect to the country center. Setting this value to different than zero will make this country ignore auto-positioning.
		/// </summary>
		public Vector2 labelOffset = Misc.Vector2zero;
		
		/// <summary>
		/// Manual rotation of the label in degrees. Setting this value to different than zero will force the label to be rotated to the specified degree.
		/// </summary>
		public float labelRotation = 0;

		/// <summary>
		/// If the label has its own font size.
		/// </summary>
		public bool labelOverridesFontSize = false;

		/// <summary>
		/// Manual font size for the label. Must set labelOverridesFontSize = true to have effect.
		/// </summary>
		public float labelFontSize = 0.2f;

		/// <summary>
		/// Colorize / texture all regions or only main region?
		/// </summary>
		public bool includeAllRegions = true;
		
		/// <summary>
		/// Apply texture to all regions or only main region?
		/// </summary>
		public bool applyTextureToAllRegions = true;

		/// <summary>
		/// Optional texture
		/// </summary>
		public Texture2D texture;

		/// <summary>
		/// The texture offset.
		/// </summary>
		public Vector2 textureOffset = Misc.Vector2zero;

		/// <summary>
		/// The texture scale.
		/// </summary>
		public Vector2 textureScale = Misc.Vector2one;

		/// <summary>
		/// The texture rotation. Note that applying a rotation will add some performance overhead during preparation of the material but not afterwards.
		/// </summary>
		public float textureRotation = 0;

		/// <summary>
		/// Optional font for the label
		/// </summary>
		public Font labelFontOverride;

		/// <summary>
		/// Internally used for decorators which have not been assigned yet.
		/// </summary>
		[HideInInspector]
		public bool isNew = true;

		public CountryDecorator() {
		}

		public CountryDecorator(string countryName) {
			this.countryName = countryName;
		}

		public void Reset() {
			customLabel = "";
			labelOverridesColor = false;
			isColorized = false;
			labelOffset = Misc.Vector2zero;
			labelRotation = 0;
			labelVisible = true;
			labelOverridesFontSize = false;
			includeAllRegions = true;
			hidden = false;
		}
	}

}