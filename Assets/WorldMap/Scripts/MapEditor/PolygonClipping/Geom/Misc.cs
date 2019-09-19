using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using WPMF;

namespace WPMF.PolygonClipping {

	static class Misc {

		const float tolerance = 0.000000001f;

		public static bool PointEquals(Vector3 p1, Vector3 p2) {
			return (p1.x == p2.x && p1.y == p2.y) ||
				(Mathf.Abs(p1.x-p2.x)/Mathf.Abs(p1.x+p2.x) < tolerance &&
				 Mathf.Abs(p1.y-p2.y)/Mathf.Abs(p1.y+p2.y) < tolerance);
		}

	}

}