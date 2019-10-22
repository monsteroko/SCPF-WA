using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ExperimentalMap {
    static public class MapUtility {
        static public Vector2 PointFromStringData(string s) {
            int j = s.IndexOf(",");
            string sx = s.Substring(0, j);
            string sy = s.Substring(j + 1);
            float x = float.Parse(sx) / Map.MapPrecision;
            float y = float.Parse(sy) / Map.MapPrecision;
            return new Vector2(x, y);
        }
    }
}
