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
    }
}
