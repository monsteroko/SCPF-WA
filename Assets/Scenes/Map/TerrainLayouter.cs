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

    public class NoiseProvider {

        public float seedx { get; private set; }
        public float seedy { get; private set; }
        public float scalex { get; private set; }
        public float scaley { get; private set; }
        public NoiseProvider(Vector2 seed, Vector2 scale) : this(seed.x, seed.y, scale.x, scale.y) {

        }
        public NoiseProvider(float seedx, float seedy, float scalex, float scaley) {
            this.seedx = seedx;
            this.seedy = seedy;
            this.scalex = scalex;
            this.scaley = scaley;
        }

        public float getNoise(Vector2 point) {
            Vector2 p = new Vector2(point.x*scalex, point.y*scaley);
            float c1 = Mathf.PerlinNoise(seedx + p.x, seedy + p.y);
            float c2 = 0.5f * Mathf.PerlinNoise((seedx + p.x) * 2.0f, (seedy + p.y) * 2.0f) * c1;
            float c3 = 0.25f * Mathf.PerlinNoise((seedx + p.x) * 4.0f, (seedy + p.y) * 4.0f) * (c1 + c2);
            //return (c1 + c2 + c3) / 1.75f;
            return c1;
        }
    }

    public partial class TerrainLayouter: MonoBehaviour {

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

        private List<Vector2> ContourFromPoint(Vector2 p, NoiseProvider noiseProvider) {
            float step = PointSize;
            List<Vector2> res = new List<Vector2>();
            res.Add(p);
            float pHeight = noiseProvider.getNoise(p);
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
                    float h = noiseProvider.getNoise(new Vector2(x, y));
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
