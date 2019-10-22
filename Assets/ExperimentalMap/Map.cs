using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;

namespace ExperimentalMap {

    class Area {
        public readonly string name;
        public readonly string counterpart;
        public Area(string name, string counterpart) {
            this.name = name;
            this.counterpart = counterpart;
        }

        public Vector2[] border;
        public Vector2 center;
        public Rect borderRect; 

        public void SetBordersData(string data) {
            string[] regions = data.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
            float maxVol = float.MinValue;
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
            border = new Vector2[coordsCount];
            for (int i1 = 0; i1 < coordsCount; i1++) {
                Vector2 point = ExperimentalMap.MapUtility.PointFromStringData(coordinates[i1]);
                border[i1] = point;
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
    }

    public class Map : MonoBehaviour {

        public static readonly float MapPrecision = 5000000f;
        public GameObject areaBackgroundObject;
        public Material areaBackgroundMaterial;
        List<Area> areas;

        void Start() {
            areas = ReadAreasData();
            CreateAreas();
        }

        void Update() {

        }

        List<Area> ReadAreasData() {
            TextAsset ta = Resources.Load<TextAsset>("Geodata/areas");
            string s = ta.text;
            string[] areasData = s.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            char[] separatorProvinces = new char[] { '$' };
            int areasCount = areasData.Length;
            List<Area> areas = new List<Area>();
            for (int i1 = 0; i1 < areasCount; i1++) {
                string[] areaInfo = areasData[i1].Split(separatorProvinces, StringSplitOptions.RemoveEmptyEntries);
                if (areaInfo.Length <= 2)
                    continue;
                string name = areaInfo[0];
                string counterpartName = areaInfo[1];
                Area area = new Area(name, counterpartName);
                area.SetBordersData(areaInfo[2]);
                areas.Add(area);
            }
            return areas;
        }


        void CreateAreas() {
            Debug.Log("sus");
            List<Vector3> frontiersPoints = new List<Vector3>();
            for (int i1 = 0; i1 < areas.Count; i1++) {
                Area area = areas[i1];
                //region.neighbours.Clear();
                int pointsCount = area.border.Length;
                for (int i2 = 0; i2 < pointsCount - 1; i2++) {
                    Vector3 p0 = area.border[i2];
                    Vector3 p1 = area.border[i2 + 1];
                    double v = (p0.x + p1.x) + MapPrecision * (p0.y + p1.y);
                    frontiersPoints.Add(p0);
                    frontiersPoints.Add(p1);
                }
                frontiersPoints.Add(area.border[pointsCount - 1]);
                frontiersPoints.Add(area.border[0]);
            }
            Debug.Log(areas.Count);
            int meshGroups = (frontiersPoints.Count / 65000) + 1;
            int meshIndex = -1;
            int[][] provincesIndices = new int[meshGroups][];
            Vector3[][] provincesBorders = new Vector3[meshGroups][];
            for (int k = 0; k < frontiersPoints.Count; k += 65000) {
                int max = Mathf.Min(frontiersPoints.Count - k, 65000);
                provincesBorders[++meshIndex] = new Vector3[max];
                provincesIndices[meshIndex] = new int[max];
                for (int j = k; j < k + max; j++) {
                    provincesBorders[meshIndex][j - k] = frontiersPoints[j];
                    provincesIndices[meshIndex][j - k] = j - k;
                }
            }

            for (int k = 0; k < provincesBorders.Length; k++) {
                GameObject flayer = new GameObject("flayer");
                flayer.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                flayer.transform.SetParent(areaBackgroundObject.transform, false);
                flayer.transform.localPosition = Vector3.zero;
                flayer.transform.localRotation = Quaternion.Euler(Vector3.zero);
                flayer.layer = areaBackgroundObject.layer;
                Debug.Log("pep");
                Mesh mesh = new Mesh();
                mesh.vertices = provincesBorders[k];
                mesh.SetIndices(provincesIndices[k], MeshTopology.Lines, 0);
                mesh.RecalculateBounds();
                mesh.hideFlags = HideFlags.DontSave;

                MeshFilter mf = flayer.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;

                MeshRenderer mr = flayer.AddComponent<MeshRenderer>();
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                //				mr.useLightProbes = false;
                mr.sharedMaterial = areaBackgroundMaterial;
            }
        }
    }

}

