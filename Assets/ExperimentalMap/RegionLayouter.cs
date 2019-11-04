using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExperimentalMap {

    public class Terrain: MapSurface {
        public Vector2[] border { get; private set; }
        public Rect borderRect { get; private set; }
        public int type;

        public Terrain(Vector2[] border) {
            this.border = border;
            Vector2 min = Vector2.one * 10;
            Vector2 max = -min;
            for (int i1 = 0; i1 < border.Length; i1++) {
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

        public List<Terrain> CreateAreaLayout(Area area) {
            int heightPoints = (int)(area.borderRect.height / PointSize);
            int widthPoints = (int)(area.borderRect.height / PointSize);
            Debug.Log(heightPoints);
            Debug.Log(widthPoints);
            bool[,] field = new bool[heightPoints,widthPoints];
            for (int i1 = 0; i1 < heightPoints; i1++) {
                for (int i2 = 0; i2 < widthPoints; i2++) {
                    field[i1,i2] = false;
                }
            }
            List<Terrain> terrains = new List<Terrain>();
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
                    
                    Vector2[] border = new Vector2[4];
                    border[0] = new Vector2(area.borderRect.xMin + PointSize * (i1 + px), area.borderRect.yMin + PointSize * (i2 + py));
                    border[1] = new Vector2(area.borderRect.xMin + PointSize * (i1 + px + sx), area.borderRect.yMin + PointSize * (i2 + py));
                    border[2] = new Vector2(area.borderRect.xMin + PointSize * (i1 + px + sx), area.borderRect.yMin + PointSize * (i2 + py + sy));
                    border[3] = new Vector2(area.borderRect.xMin + PointSize * (i1 + px), area.borderRect.yMin + PointSize * (i2 + py + sy));
                    Terrain terrain = new Terrain(border);
                    terrain.type = Random.Range(1, 3);
                    terrains.Add(terrain);
                }
            }
            Debug.Log(terrains.Count);
            return terrains;
        }
    }

}
