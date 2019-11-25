using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentalMap {

    public enum AreaState {
        Locked = 0,
        Unlocked = 1,
        Controlled = 2,
    }

    public class MapSurface {
        public List<Vector2> border { get; protected set; }
        public Rect borderRect { get; protected set; }

        public bool IsValid() {
            return border.Count >= 3;
        }

        public void ClipSurface(MapSurface surface) {
            border = (new ClipperUtility()).ClipBorder(border, surface.border);
        }
    }

    public class Area : MapSurface {
        public readonly string name;
        public readonly string counterpart;
        public Area(string name, string counterpart) {
            this.name = name;
            this.counterpart = counterpart;
            this.terrains = new List<Terrain>();
        }

        public Vector2 center;
        public List<Terrain> terrains { get; private set; }
        public GameObject lockedOverlay;
        private AreaState _state = AreaState.Locked;

        public AreaState state {
            get {
                return _state;
            }
            set {
                if (value == _state) return;
                lockedOverlay.SetActive(state != AreaState.Locked);
                _state = value;
            }
        }

        // Geometry

        public void SetBordersData(string data) {
            string[] regions = data.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            Vector2 minPoint = Vector2.one * 10;
            Vector2 maxPoint = -minPoint;
            char[] separatorRegions = new char[] { ';' };
            if (regions.Length == 0) {
            }
            string region = regions[0];
            string[] coordinates = region.Split(separatorRegions, StringSplitOptions.RemoveEmptyEntries);
            int coordsCount = coordinates.Length;
            if (coordsCount < 3) {
                Debug.Log("Incorrect area border data");
                return;
            }
            Vector3 min = Vector3.one * 10;
            Vector3 max = -min;
            border = new List<Vector2>();
            for (int i1 = 0; i1 < coordsCount; i1++) {
                Vector2 point = ExperimentalMap.MapUtility.PointFromStringData(coordinates[i1]);
                border.Add(point);
                if (point.x < min.x)
                    min.x = point.x;
                if (point.x > max.x)
                    max.x = point.x;
                if (point.y < min.y)
                    min.y = point.y;
                if (point.y > max.y)
                    max.y = point.y;
            }
            center = (min + max) * 0.5f;
            borderRect = new Rect(min.x, min.y, Math.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        public void SetTerrains(List<Terrain> terrains) {
            this.terrains = terrains;
            if (terrains.Count > 0) {
                this.border = new TerrainLayouter().CalculateSummaryBorder(border, terrains);
            }
        }

        public void AddTerrain(Terrain terrain) {
            border = (new ClipperUtility()).UnionBorders(border, terrain.border);
            foreach (Terrain oldTerrain in terrains) {
                oldTerrain.ClipSurface(terrain);
            }
            terrains.Add(terrain);
        }

        public void ClipTerrain(Terrain terrain) {
            ClipSurface(terrain);
            foreach (Terrain oldTerrain in terrains) {
                oldTerrain.ClipSurface(terrain);
            }
        }
    }

}