using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace ExperimentalMap {

    public class Terrain: MapSurface {
        public List<Vector2> border { get; private set; }
        public Rect borderRect { get; private set; }
        public Vector2 center { get; private set; }
        public int type;

        public Terrain(List<Vector2> border) {
            this.border = border;
            Vector2 min = Vector2.one * 10;
            Vector2 max = -min;
            for (int i1 = 0; i1 < border.Count; i1++) {
                Vector2 point = border[i1];
                if (point.x < min.x)
                    min.x = point.x;
                if (point.x > max.x)
                    max.x = point.x;
                if (point.y < min.y)
                    min.y = point.y;
                if (point.y > max.y)
                    max.y = point.y;
            }
            borderRect = new Rect(min, max - min);
            center = (min + max) / 2.0f;
        }
    }

    public class TerrainLayouter {

        const float PointSize = 0.0004f;
        const float chunkSize = 0.008f;
        const int TerrainSizeMark = 5;

        struct PriorityPath {
            public int priority;
            public List<IntPoint> path;

            public PriorityPath(int priority, List<IntPoint> path) {
                this.priority = priority;
                this.path = path;
            }
        }

        public void CreateLayoutForAreas(ref List<Area> areas) {
            if (areas.Count == 0) {
                return;
            }
            Vector2 minBound = areas[0].borderRect.min;
            Vector2 maxBound = areas[0].borderRect.max;
            List<Vector2> chunkCenters = new List<Vector2>();
            List<Terrain> terrains = new List<Terrain>();
            foreach (Area area in areas) {
                minBound.x = Mathf.Min(minBound.x, area.borderRect.min.x);
                minBound.y = Mathf.Min(minBound.y, area.borderRect.min.y);
                maxBound.x = Mathf.Max(maxBound.x, area.borderRect.max.x);
                maxBound.y = Mathf.Max(maxBound.y, area.borderRect.max.y);
            }
            int counter = 0;
            ClipperUtility utility = new ClipperUtility();
            while (counter < 100) {
                counter++;
                Vector2 newCenter = new Vector2(Random.Range(minBound.x, maxBound.x), Random.Range(minBound.y, maxBound.y));
                bool isInside = false;
                int areaIndex = 0;
                foreach (Area area in areas) {
                    if (utility.IsPointInsideSurface(newCenter, area)) {
                        isInside = true;
                        break;
                    }
                    areaIndex++;
                }
                if (!isInside) continue;
                bool isTooClose = false;
                foreach (Vector2 center in chunkCenters) {
                    if ((newCenter - center).magnitude < 3*chunkSize) {
                        isTooClose = true;
                    }
                }
                if (isTooClose) continue;
                chunkCenters.Add(newCenter);
                terrains.AddRange(CreateTerrainChunk(newCenter));
                counter = 0;
            }
            List<int> indeces = new List<int>();
            foreach (Terrain terrain in terrains) {
                bool found = false;
                for (int i = 0; i < areas.Count; i++) {
                    if (utility.IsPointInsideSurface(terrain.center, areas[i])) {
                        indeces.Add(i);
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    indeces.Add(-1);
                }
            }
            for (int i1 = 0; i1 < terrains.Count; i1++) {
                for (int i2 = 0; i2 < areas.Count; i2++) {
                    if (i2 == indeces[i1]) {
                        areas[i2].AddTerrain(terrains[i1]);
                    } else {
                        areas[i2].ClipTerrain(terrains[i1]);
                    }
                }
            }

        }

        public List<Terrain> CreateTerrainChunk(Vector2 center) {
            int heightPoints = (int)(chunkSize / PointSize);
            int widthPoints = (int)(chunkSize / PointSize);
            bool[,] field = new bool[heightPoints, widthPoints];
            for (int i1 = 0; i1 < heightPoints; i1++) {
                for (int i2 = 0; i2 < widthPoints; i2++) {
                    field[i1, i2] = false;
                }
            }
            List<Terrain> terrains = new List<Terrain>();
            List<PriorityPath> paths = new List<PriorityPath>();
            for (int i1 = 0; i1 < heightPoints; i1++) {
                for (int i2 = 0; i2 < widthPoints; i2++) {
                    if (field[i1, i2]) {
                        continue;
                    }
                    int px = Random.Range(-TerrainSizeMark / 2, 0);
                    int py = Random.Range(-TerrainSizeMark / 2, 0);
                    int sx = Random.Range(TerrainSizeMark * 2, TerrainSizeMark * 4);
                    int sy = Random.Range(TerrainSizeMark * 2, TerrainSizeMark * 4);
                    for (int ix = Mathf.Max(0, i1 + px); ix < Mathf.Min(heightPoints, i1 + px + sx); ix++) {
                        for (int iy = Mathf.Max(0, i2 + py); iy < Mathf.Min(widthPoints, i2 + py + sy); iy++) {
                            field[ix, iy] = true;
                        }
                    }

                    List<IntPoint> path = new List<IntPoint>();
                    path.Add(new IntPoint(i1 + px, i2 + py));
                    path.Add(new IntPoint(i1 + px + sx, i2 + py));
                    path.Add(new IntPoint(i1 + px + sx, i2 + py + sy));
                    path.Add(new IntPoint(i1 + px, i2 + py + sy));
                    PriorityPath priorityPath = new PriorityPath(Random.Range(1, 100), path);
                    paths.Add(priorityPath);
                }
            }
            paths.Sort((p1, p2) => p1.priority.CompareTo(p2.priority));
            List<List<IntPoint>> topPaths = new List<List<IntPoint>>();
            List<List<IntPoint>> resultPaths = new List<List<IntPoint>>();
            for (int i1 = paths.Count - 1; i1 >= 0; i1--) {
                List<List<IntPoint>> subject = new List<List<IntPoint>>();
                List<List<IntPoint>> results = new List<List<IntPoint>>();
                subject.Add(paths[i1].path);
                Clipper c = new Clipper();
                c.AddPaths(subject, PolyType.ptSubject, true);
                c.AddPaths(topPaths, PolyType.ptClip, true);
                c.Execute(ClipType.ctDifference, results, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
                foreach (List<IntPoint> result in results) {
                    resultPaths.Add(result);
                }
                topPaths.Add(paths[i1].path);
            }
            foreach (List<IntPoint> path in resultPaths) {
                List<Vector2> border = new List<Vector2>();
                for (int i1 = 0; i1 < path.Count; i1++) {
                    border.Add(new Vector2(center.x - widthPoints / 2 * PointSize + PointSize * path[i1].X, center.y - widthPoints / 2 * PointSize + PointSize * path[i1].Y));
                }
                Terrain terrain = new Terrain(border);
                terrain.type = Random.Range(0, 3);
                terrains.Add(terrain);

            }
            return terrains;
        }

        public List<Vector2> CalculateSummaryBorder(List<Vector2> border, List<Terrain> terrains) {
            ClipperUtility utility = new ClipperUtility();
            List<List<IntPoint>> subject = new List<List<IntPoint>>();
            List<List<IntPoint>> addition = new List<List<IntPoint>>();
            List<List<IntPoint>> results = new List<List<IntPoint>>();
            foreach (Terrain terrain in terrains) {
                addition.Add(utility.VectorPathToClipperPath(terrain.border));
            }
            subject.Add(utility.VectorPathToClipperPath(border));
            Clipper c = new Clipper();
            c.AddPaths(subject, PolyType.ptSubject, true);
            c.AddPaths(addition, PolyType.ptClip, true);
            c.Execute(ClipType.ctUnion, results, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            if (results.Count > 0) {
                return utility.ClipperPathToVectorPath(results[0]);
            } else {
                Debug.Log("Invalid terrain set");
                return new List<Vector2>();
            }
        }
    }

}
