using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace ExperimentalMap {
    public class ClipperUtility {

        private const int ClipperMultiplier = 1000000;

        public List<IntPoint> VectorPathToClipperPath(List<Vector2> border) {
            List<IntPoint> path = new List<IntPoint>();
            foreach (Vector2 realPoint in border) {
                path.Add(new IntPoint(realPoint.x * ClipperMultiplier, realPoint.y * ClipperMultiplier));
            }
            return path;
        }

        public List<Vector2> ClipperPathToVectorPath(List<IntPoint> path) {
            List<Vector2> border = new List<Vector2>();
            foreach (IntPoint point in path) {
                border.Add(new Vector2((float)point.X / ClipperMultiplier, (float)point.Y / ClipperMultiplier));
            }
            return border;
        }

        public bool IsPointInsideSurface(Vector2 point, MapSurface polygon) {
            int polygonLength = polygon.border.Count, i = 0;
            bool inside = false;
            float pointX = point.x, pointY = point.y;
            float startX, startY, endX, endY;
            Vector2 endPoint = polygon.border[polygonLength - 1];
            endX = endPoint.x;
            endY = endPoint.y;
            while (i < polygonLength) {
                startX = endX; startY = endY;
                endPoint = polygon.border[i++];
                endX = endPoint.x; endY = endPoint.y;
                inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
            }
            return inside;
        }

        public List<Vector2> UnionBorders(List<Vector2> border1, List<Vector2> border2) {
            List<List<IntPoint>> s1 = new List<List<IntPoint>>();
            List<List<IntPoint>> s2 = new List<List<IntPoint>>();
            List<List<IntPoint>> results = new List<List<IntPoint>>();
            s1.Add(VectorPathToClipperPath(border1));
            s1.Add(VectorPathToClipperPath(border2));
            Clipper c = new Clipper();
            c.AddPaths(s1, PolyType.ptSubject, true);
            c.AddPaths(s2, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, results, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            if (results.Count > 0) {
                return ClipperPathToVectorPath(results[0]);
            } else {
                Debug.Log("Invalid borders to union");
                return new List<Vector2>();
            }
        }

        public List<Vector2> ClipBorder(List<Vector2> border1, List<Vector2> border2) {
            List<List<IntPoint>> s1 = new List<List<IntPoint>>();
            List<List<IntPoint>> s2 = new List<List<IntPoint>>();
            List<List<IntPoint>> results = new List<List<IntPoint>>();
            s1.Add(VectorPathToClipperPath(border1));
            s2.Add(VectorPathToClipperPath(border2));
            Clipper c = new Clipper();
            c.AddPaths(s1, PolyType.ptSubject, true);
            c.AddPaths(s2, PolyType.ptClip, true);
            c.Execute(ClipType.ctDifference, results, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            if (results.Count > 0) {
                return ClipperPathToVectorPath(results[0]);
            } else {
                Debug.Log("Invalid borders to clip");
                return new List<Vector2>();
            }
        }
    }
}
