using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WPMF {

	/// <summary>
	/// Mount Point record. Mount points are stored in the mountPoints file, in packed string editable format inside Resources/Geodata folder.
	/// </summary>
	public class MountPoint {

		/// <summary>
		/// Name of this mount point.
		/// </summary>
		public string name;

		/// <summary>
		/// Type of mount point. This is an optional, user-defined integer value.
		/// </summary>
		public int type;

		/// <summary>
		/// The index of the country.
		/// </summary>
		public int countryIndex;

		/// <summary>
		/// The index of the province or -1 if the mount point is not linked to any province.
		/// </summary>
		public int provinceIndex;

		/// <summary>
		/// The location of the mount point on the sphere.
		/// </summary>
		public Vector2 unity2DLocation;

		/// <summary>
		/// The custom tags stored in a dictionary object (both keys and values are strings). These are optionally, user-defined values.
		/// </summary>
		public Dictionary<string,string>customTags;

		public MountPoint (string name, int countryIndex, int provinceIndex, Vector2 unity2DLocation, int type, Dictionary<string, string>tags) {
			this.name = name;
			this.countryIndex = countryIndex;
			this.provinceIndex = provinceIndex;
			this.unity2DLocation = unity2DLocation;
			this.type = type;
			this.customTags = tags;
		}

		public MountPoint (string name, int countryIndex, int provinceIndex, Vector2 unity2DLocation, int type): this(name, countryIndex, provinceIndex, unity2DLocation, type, new Dictionary<string,string>()) {
		}

		public MountPoint (string name, int countryIndex, int provinceIndex, Vector3 location): this(name, countryIndex, provinceIndex, location, 0, new Dictionary<string,string>()) {
		}

		public MountPoint Clone() {
			// Clone dictionary
			Dictionary<string, string> tags = new Dictionary<string, string>(customTags.Count, customTags.Comparer);
			foreach (KeyValuePair<string, string> entry in customTags)
			{
				tags.Add(entry.Key, (string) entry.Value.Clone());
			}
			MountPoint c = new MountPoint(name, countryIndex, provinceIndex, unity2DLocation, type, tags);
			return c;
		}
	}
}