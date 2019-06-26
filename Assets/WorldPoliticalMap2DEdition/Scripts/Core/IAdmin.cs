using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {

	public interface IAdminEntity {

		/// <summary>
		/// Entity name.
		/// </summary>
		string name { get; set; }

		/// <summary>
		/// List of all regions for the admin entity.
		/// </summary>
		List<Region> regions { get; set; }
	
		/// <summary>
		/// Center of the admin entity in the plane
		/// </summary>
		Vector2 center { get; set; }
		int mainRegionIndex { get; set; }

		/// <summary>
		/// Used internally by Editor.
		/// </summary>
		bool foldOut { get; set; }
	}
}
