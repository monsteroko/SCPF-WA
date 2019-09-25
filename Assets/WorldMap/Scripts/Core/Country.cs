using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	public class Country: IAdminEntity {

		/// <summary>
		/// Country name.
		/// </summary>
		public string name { get; set; }
		
		/// <summary>
		/// List of all regions for the admin entity.
		/// </summary>
		public List<Region> regions { get; set; }

		/// <summary>
		/// Computed Rect area that includes all regions. Used to fast hovering.
		/// </summary>
		public Rect regionsRect2D;

		/// <summary>
		/// Setting hidden to true will hide completely the country (border, label) and it won't be highlighted
		/// </summary>
		public bool hidden;

		/// <summary>
		/// Center of the admin entity in the plane
		/// </summary>
		public Vector2 center { get; set; }

		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		/// <summary>
		/// Continent name.
		/// </summary>
		public string continent;

		/// <summary>
		/// List of provinces that belongs to this country.
		/// </summary>
		public Province[] provinces;

		/// <summary>
		/// Optional custom label. It set, it will be displayed instead of the country name.
		/// </summary>
		public string customLabel; 

		/// <summary>
		/// Set it to true to specify a custom color for the label.
		/// </summary>
		public bool labelColorOverride;

		/// <summary>
		/// The color of the label.
		/// </summary>
		public Color labelColor = Color.white;
		Font _labelFont;
		Material _labelShadowFontMaterial;

		/// <summary>
		/// Internal method used to obtain the shadow material associated to a custom Font provided.
		/// </summary>
		/// <value>The label shadow font material.</value>
		public Material labelFontShadowMaterial { get { return _labelShadowFontMaterial; } }

		/// <summary>
		/// Optional font for this label. Note that the font material will be instanced so it can change color without affecting other labels.
		/// </summary>
		public Font labelFontOverride { 
			get {
				return _labelFont;
			}
			set {
				if (value != _labelFont) {
					_labelFont = value;
					if (_labelFont != null) {
						Material fontMaterial = GameObject.Instantiate (_labelFont.material);
						fontMaterial.hideFlags = HideFlags.DontSave;
						_labelFont.material = fontMaterial;
						_labelShadowFontMaterial = GameObject.Instantiate (fontMaterial);
						_labelShadowFontMaterial.hideFlags = HideFlags.DontSave;
						_labelShadowFontMaterial.renderQueue--;
					}
				}
			}
		}

		/// <summary>
		/// Sets whether the country name will be shown or not.
		/// </summary>
		public bool labelVisible = true;
		
		/// <summary>
		/// If set to a value > 0 degrees then label will be rotated according to this value (and not automatically calculated).
		/// </summary>
		public float labelRotation = 0;
		
		/// <summary>
		/// If set to a value != 0 in both x/y then label will be moved according to this value (and not automatically calculated).
		/// </summary>
		public Vector2 labelOffset = Misc.Vector2zero;

		
		/// <summary>
		/// If the label has its own font size.
		/// </summary>
		public bool labelFontSizeOverride = false;
		
		/// <summary>
		/// Manual font size for the label. Must set labelOverridesFontSize = true to have effect.
		/// </summary>
		public float labelFontSize = 0.2f;

		/// <summary>
		/// Used internally by Editor.
		/// </summary>
		public bool foldOut { get; set; }

		/// <summary>
		/// Set to false to prevent drawing provinces for this country
		/// </summary>
		public bool allowShowProvinces = true;


		#region internal fields
		// Used internally. Don't change fields below.
		public GameObject labelGameObject;
		public float labelMeshWidth, labelMeshHeight;
		public Vector2 labelMeshCenter;
		#endregion

		public Country (string name, string continent) {
			this.name = name;
			this.continent = continent;
			this.regions = new List<Region> ();
		}

		public Country Clone() {
			Country c = new Country(name, continent);
			c.center = center;
			c.regions = regions;
			c.customLabel = customLabel;
			c.labelColor = labelColor;
			c.labelColorOverride = labelColorOverride;
			c.labelFontOverride = labelFontOverride;
			c.labelVisible = labelVisible;
			c.labelOffset = labelOffset;
			c.labelRotation = labelRotation;
			c.provinces = provinces;
			c.hidden = this.hidden;
			return c;
		}

	}

}