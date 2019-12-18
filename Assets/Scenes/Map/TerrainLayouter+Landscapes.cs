using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClipperLib;

namespace ExperimentalMap {

    public class LandscapesNoiseProvider: NoiseProvider {
        public LandscapesNoiseProvider(float seed) : base(seed, seed, 100, 100) {

        }
    }
    public class LandscapesResidualNoiseProvider: NoiseProvider {
        public List<float> noiseLevels { get; private set; }
        public int noiseLevelsCount {
            get {
                return noiseLevels.Count;
            }
        }
        public LandscapesResidualNoiseProvider(float seed) : base(seed, seed, 100, 100) {
            noiseLevels = new List<float>();
            noiseLevels.Add(0.0f);
            noiseLevels.Add(0.55f);
            noiseLevels.Add(0.70f);
        }

        public float getResidualNoise(Vector2 point, List<Terrain> terrains) {
            float height = getNoise(point);
            ClipperUtility utility = new ClipperUtility();
            int levels = 0;
            foreach (Terrain terrain in terrains) {
                if (utility.IsPointInsideSurface(point, terrain)) {
                    levels++;
                }
            }
            height -= noiseLevels[Mathf.Min(noiseLevels.Count - 1, levels)];
            return height;
        }
    }
    public partial class TerrainLayouter {

        private readonly LandscapesNoiseProvider landscapesNoiseProvider = new LandscapesNoiseProvider(10.5f);
        private readonly LandscapesResidualNoiseProvider landscapesResidualNoiseProvider = new LandscapesResidualNoiseProvider(10.5f);

        public List<Terrain> CreateLandscapesForAreas(ref List<Area> areas) {
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
            for (int i0 = 0; i0 < 10; i0++) {
                //while (true) {
                Vector2 bp = new Vector2(0, 0);
                float bh = 0;
                for (int i1 = 0; i1 < heightPoints; i1++) {
                    for (int i2 = 0; i2 < widthPoints; i2++) {
                        Vector2 p = new Vector2(minBound.x + i1 * PointSize, minBound.y + i2 * PointSize);
                        float h = landscapesResidualNoiseProvider.getResidualNoise(p, terrains);
                        if (h > bh) {
                            bp = p;
                            bh = h;
                        }
                    }
                }
                int nextLevel = landscapesResidualNoiseProvider.noiseLevelsCount - 1;
                for (int i1 = 1; i1 < landscapesResidualNoiseProvider.noiseLevelsCount; i1++) {
                    if (landscapesResidualNoiseProvider.noiseLevels[i1] > bh) {
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
                while (nextLevel > 0) {
                    bp.x += PointSize / 10.0f;
                    bp.y += PointSize / 50.0f;
                    if (landscapesNoiseProvider.getNoise(bp) < landscapesResidualNoiseProvider.noiseLevels[createdLevels + nextLevel]) {
                        List<Vector2> cont = ContourFromPoint(bp, landscapesNoiseProvider);
                        Terrain terrain = new Terrain(cont);
                        terrain.elevation = createdLevels + nextLevel;
                        terrain.terrainType = TerrainType.Landscape;
                        terrains.Add(terrain);
                        nextLevel--;
                    }
                }
            }
            //Clip to borders
            List<List<Vector2>> areaBorders = new List<List<Vector2>>();
            foreach (Area area in areas) {
                areaBorders.Add(area.border);
            }
            List<Vector2> summaryBorder = utility.UnionBorders(areaBorders);
            foreach (Terrain terrain in terrains) {
                terrain.IntersectBorder(summaryBorder);
            }
            return terrains;
        }

    }

}
