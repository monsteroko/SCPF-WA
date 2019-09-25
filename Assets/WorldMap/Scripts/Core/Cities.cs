using UnityEngine;
using System.Collections;

namespace WPMF {

	public enum CITY_CLASS {
		CITY = 1,
		REGION_CAPITAL = 2,
		COUNTRY_CAPITAL = 4
	}

	public class City {
		public string name;
		public string province;
		public int countryIndex;
		public Vector2 unity2DLocation;
		public int population;
		public CITY_CLASS cityClass;

		/// <summary>
		/// Set by DrawCities method.
		/// </summary>
		public bool isVisible; 

		public City (string name, string province, int countryIndex,  int population, Vector2 location, CITY_CLASS cityClass) {
			this.name = name;
			this.province = province;
			this.countryIndex = countryIndex;
			this.population = population;
			this.unity2DLocation = location;
			this.cityClass = cityClass;
		}

		public City Clone() {
			City c = new City(name, province, countryIndex, population, unity2DLocation, cityClass);
			return c;
		}
	}
}