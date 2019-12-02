using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace ExperimentalMap {

    public class Terrain: MapSurface {
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

    public class TerrainLayouter: MonoBehaviour {

        public Material continental1Material;
        public Material continental2Material;
        public Material continental3Material;

        const float PointSize = 0.0003f;
        const float chunkSize = 0.003f;
        const int TerrainSizeMark = 4;

        ClipperUtility utility = new ClipperUtility();

        struct PriorityPath {
            public int priority;
            public List<IntPoint> path;

            public PriorityPath(int priority, List<IntPoint> path) {
                this.priority = priority;
                this.path = path;
            }
        }

        public Material MaterialForSurface(MapSurface surface) {
            switch (surface.elevation) {
                case 0:
                    return continental1Material;
                case 1:
                    return continental2Material;
                default:
                    return continental3Material;
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
                if (indeces[i1]>=0) {
                    for (int i2 = 0; i2 < areas.Count; i2++) {
                        if (i2 == indeces[i1]) {
                            areas[i2].AddTerrain(terrains[i1]);
                        } else if (utility.AreSurfacesNear(areas[i2], terrains[i1])) {
                            areas[i2].ClipTerrain(terrains[i1]);
                        }
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

        private float LandscapeHeight(Vector2 point) {
            float seedx = 111.5f;
            float seedy = 0.5f;
            Vector2 p = point * 100;
            float c1 = Mathf.PerlinNoise(seedx + p.x, seedy + p.y);
            float c2 = 0.5f * Mathf.PerlinNoise((seedx + p.x) * 2.0f, (seedy + p.y) * 2.0f) * c1;
            float c3 = 0.25f * Mathf.PerlinNoise((seedx + p.x) * 4.0f, (seedy + p.y) * 4.0f) * (c1 + c2);
            //return (c1 + c2 + c3) / 1.75f;
            return c1;
        }

        private float ResidualLandscapeHeight(Vector2 point, List<Terrain> terrains, List<float> heightLevels) {
            float height = LandscapeHeight(point);
            ClipperUtility utility = new ClipperUtility();
            int levels = 0;
            foreach (Terrain terrain in terrains) {
                if (utility.IsPointInsideSurface(point, terrain)) {
                    levels++;
                }
            }
            height -= heightLevels[Mathf.Min(heightLevels.Count-1, levels)];
            return height;
        }

        public List<Terrain> CreateLandscapesForAreas(ref List<Area> areas) {
            List<float> grassLevels = new List<float>();
            grassLevels.Add(0.0f);
            grassLevels.Add(0.55f);
            grassLevels.Add(0.70f);
            List<Terrain> terrains = new List<Terrain>();
            if (areas.Count == 0) {
                return terrains;
            }
            Vector2 minBound = areas[0].borderRect.min;
            Vector2 maxBound = areas[0].borderRect.max;
            List<Vector2> chunkCenters = new List<Vector2>();
            foreach (Area area in areas) {
                minBound.x = Mathf.Min(minBound.x, area.borderRect.min.x);
                minBound.y = Mathf.Min(minBound.y, area.borderRect.min.y);
                maxBound.x = Mathf.Max(maxBound.x, area.borderRect.max.x);
                maxBound.y = Mathf.Max(maxBound.y, area.borderRect.max.y);
            }
            int heightPoints = (int)((maxBound.y - minBound.y) / PointSize);
            int widthPoints = (int)((maxBound.x - minBound.x) / PointSize);
            Debug.Log(heightPoints);
            Debug.Log(widthPoints);
            for (int i0=0; i0<10; i0++) {
            //while (true) {
                Vector2 bp = new Vector2(0, 0);
                float bh = 0;
                for (int i1 = 0; i1 < heightPoints; i1++) {
                    for (int i2 = 0; i2 < widthPoints; i2++) {
                        Vector2 p = new Vector2(minBound.x + i1 * PointSize, minBound.y + i2 * PointSize);
                        float h = ResidualLandscapeHeight(p, terrains, grassLevels);
                        if (h > bh) {
                            bp = p;
                            bh = h;
                        }
                    }
                }
                int nextLevel = grassLevels.Count - 1;
                for (int i1=1; i1<grassLevels.Count; i1++) {
                    if (grassLevels[i1]>bh) {
                        nextLevel = i1 - 1;
                        break;
                    }
                }
                int createdLevels = 0;
                foreach (Terrain terrain in terrains) {
                    if (utility.IsPointInsideSurface(bp, terrain)) {
                        createdLevels++;
                    }
                }
                if (nextLevel == 0) {
                    break;
                }
                while (nextLevel>0) {
                    bp.x += PointSize / 10.0f;
                    bp.y += PointSize / 50.0f;
                    if (LandscapeHeight(bp) < grassLevels[createdLevels + nextLevel]) {
                        List<Vector2> cont = ContourFromPoint(bp);
                        Terrain terrain = new Terrain(cont);
                        terrain.elevation = createdLevels + nextLevel;
                        terrains.Add(terrain);
                        nextLevel--;
                    }
                }
            }
            return terrains;
        }

        private List<Vector2> ContourFromPoint(Vector2 p) {
            float step = PointSize;
            List<Vector2> res = new List<Vector2>();
            res.Add(p);
            float pHeight = LandscapeHeight(p);
            float cx = p.x;
            float cy = p.y;
            float cdir = 0;
            for (int i1 = 0; i1<10000; i1++) {
                float bx = cx;
                float by = cy;
                float bdir = cdir;
                float bhdiff = float.MaxValue;
                for (float dir = cdir + Mathf.PI / 2.0f; dir >= cdir - Mathf.PI / 2.0f; dir -= Mathf.PI/10.0f) {
                    float x = cx + step * Mathf.Cos(dir);
                    float y = cy + step * Mathf.Sin(dir);
                    float h = LandscapeHeight(new Vector2(x, y));
                    if (Mathf.Abs(h - pHeight) < bhdiff && h < pHeight) {
                        bx = x;
                        by = y;
                        bdir = dir;
                        bhdiff = Mathf.Abs(h - pHeight);
                    }
                }
                cx = bx;
                cy = by;
                cdir = bdir;
                Vector2 cv = new Vector2(cx, cy);
                res.Add(cv);
                if ((cv - p).magnitude < step && i1 > 3) {
                    return res;
                }
            }
            Debug.Log(pHeight);
            Debug.Log("Could not create valid contour");
            return res;
        }
    }

}
