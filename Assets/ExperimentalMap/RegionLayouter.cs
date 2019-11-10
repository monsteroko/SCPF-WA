﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace ExperimentalMap {

    public class Terrain: MapSurface {
        public List<Vector2> border { get; private set; }
        public Rect borderRect { get; private set; }
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
        }
    }

    public class TerrainLayouter {

        const float PointSize = 0.001f;
        const int TerrainSizeMark = 5;

        struct PriorityPath {
            public int priority;
            public List<IntPoint> path;

            public PriorityPath(int priority, List<IntPoint> path) {
                this.priority = priority;
                this.path = path;
            }
        }

        public List<Terrain> CreateAreaLayout(Area area) {
            int heightPoints = (int)(area.borderRect.height / PointSize);
            int widthPoints = (int)(area.borderRect.height / PointSize);
            bool[,] field = new bool[heightPoints,widthPoints];
            for (int i1 = 0; i1 < heightPoints; i1++) {
                for (int i2 = 0; i2 < widthPoints; i2++) {
                    field[i1,i2] = false;
                }
            }
            List<Terrain> terrains = new List<Terrain>();
            List<PriorityPath> paths = new List<PriorityPath>();
            for (int i1 = 0; i1 < heightPoints; i1++) {
                for (int i2 = 0; i2 < widthPoints; i2++) {
                    if (field[i1, i2]) {
                        continue;
                    }
                    int px = Random.Range(-TerrainSizeMark/2, 0);
                    int py = Random.Range(-TerrainSizeMark/2, 0);
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
            for (int i1=paths.Count-1; i1>=0; i1--) {
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
                for (int i1=0; i1<path.Count; i1++) {
                    border.Add(new Vector2(area.borderRect.xMin + PointSize * path[i1].X, area.borderRect.yMin + PointSize * path[i1].Y));
                }
                Terrain terrain = new Terrain(border);
                terrain.type = Random.Range(0, 3);
                terrains.Add(terrain);

            }
            return terrains;
        }

        public List<Vector2> CalculateSummaryBorder(List<Terrain> terrains) {
            ClipperUtility utility = new ClipperUtility();
            List<List<IntPoint>> subject = new List<List<IntPoint>>();
            List<List<IntPoint>> results = new List<List<IntPoint>>();
            foreach (Terrain terrain in terrains) {
                subject.Add(utility.VectorPathToClipperPath(terrain.border));
            }
            Clipper c = new Clipper();
            c.AddPaths(subject, PolyType.ptSubject, true);
            c.AddPaths(new List<List<IntPoint>>(), PolyType.ptClip, true);
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
