using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {
	public class Province: IAdminEntity {
		/// <summary>
		/// Province name.
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
		/// Center of the admin entity in the plane
		/// </summary>
		public Vector2 center { get; set; }
		
		/// <summary>
		/// Index of the biggest region
		/// </summary>
		public int mainRegionIndex { get; set; }

		/// <summary>
		/// Used internally by Editor.
		/// </summary>
		public bool foldOut { get; set; }


		#region internal fields
		// Used internally. Don't change fields below.
		public string packedRegions;
		public int countryIndex;
		#endregion

		public Province (string name, int countryIndex) {
			this.name = name;
			this.countryIndex = countryIndex;
			this.regions = null; // lazy load during runtime due to size of data
			this.center = Misc.Vector3zero;
		}

		public Province Clone() {
			Province p = new Province(name, countryIndex);
			p.countryIndex = countryIndex;
			p.regions = regions;
			p.center = center;
			p.mainRegionIndex = mainRegionIndex;
			return p;
		}

	}
}

