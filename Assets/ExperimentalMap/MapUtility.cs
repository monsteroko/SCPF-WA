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

    public partial class Triangulator {
        public int[] TriangulateOriented() {
            int[] indeces = Triangulate();
            Vector3 v0 = m_points[indeces[0]];
            Vector3 v1 = m_points[indeces[1]];
            Vector3 v2 = m_points[indeces[2]];
            Vector3 vf = Vector3.Cross(v1 - v0, v2 - v0);
            if (Vector3.Dot(new Vector3(0, 1, 0), vf) > 0) {
                int indicesLength = indeces.Length;
                for (int k = 0; k < indicesLength; k += 3) {
                    int a = indeces[k];
                    indeces[k] = indeces[k + 1];
                    indeces[k + 1] = a;
                }
            }
            return indeces;
        }
    }
}
