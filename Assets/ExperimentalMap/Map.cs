using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;
using UnityEditor;

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
        public Vector3[] border3d {
            get {
                Vector3[] border3 = new Vector3[border.Length];
                for (int i1 = 0; i1 < border.Length; i1++) {
                    border3[i1] = border[i1];
                }
                return border3;
            }
        }

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
        public Material areaBorderMaterial;
        public Material areaBackgroundMaterial;
        public Camera camera;
        List<Area> areas;

        void Start() {
            areas = ReadAreasData();
            CreateAreas();
        }

        float zoomAcceleration = 0;
        void Update() {
            if (!Application.isPlaying)
                return;
            bool isMouseOver = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                //TODO: Set isMouseOver
            }

            if (isMouseOver) {
                float currentAcceleration = Input.GetAxis("Mouse ScrollWheel");
                zoomAcceleration += currentAcceleration;
                // Touch Screen
                if (Input.touchSupported && Input.touchCount == 2) { 
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
                    zoomAcceleration += deltaMagnitudeDiff;
                }
            }

            if (zoomAcceleration != 0) {
                const float maxZoomSpeed = 0.1f;
                const float zoomSpeedMultiplier = 8f;
                float zoomSpeed = Mathf.Clamp(zoomAcceleration, -maxZoomSpeed, maxZoomSpeed);
                camera.transform.Translate(camera.transform.forward * zoomSpeed * zoomSpeedMultiplier);
                zoomAcceleration *= 0.9f;
                if (Mathf.Abs(zoomAcceleration) < maxZoomSpeed/10000.0f) {
                    zoomAcceleration = 0;
                }
            }
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
            foreach (Area area in areas) {
                CreateAreaSurface(area, areaBackgroundMaterial);
                CreateAreaBorder(area, areaBorderMaterial);
            }
            
        }

        GameObject CreateAreaBorder(Area area, Material material) {
            List<Vector3> frontiersPoints = new List<Vector3>();
            int pointsCount = area.border.Length;
            for (int i1 = 0; i1 < pointsCount - 1; i1++) {
                Vector3 p0 = area.border[i1];
                Vector3 p1 = area.border[i1 + 1];
                double v = (p0.x + p1.x) + MapPrecision * (p0.y + p1.y);
                frontiersPoints.Add(p0);
                frontiersPoints.Add(p1);
            }
            frontiersPoints.Add(area.border[pointsCount - 1]);
            frontiersPoints.Add(area.border[0]);

            int meshIndex = -1;
            int[] provincesIndices = new int[frontiersPoints.Count];
            Vector3[] provincesBorders = new Vector3[frontiersPoints.Count];
            for (int j = 0; j < frontiersPoints.Count; j++) {
                provincesBorders[j] = frontiersPoints[j];
                provincesIndices[j] = j;
            }

            GameObject borderSurface = new GameObject("borderSurface");
            borderSurface.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            borderSurface.transform.SetParent(areaBackgroundObject.transform, false);
            borderSurface.transform.localPosition = Vector3.zero;
            borderSurface.transform.localRotation = Quaternion.Euler(Vector3.zero);
            borderSurface.layer = areaBackgroundObject.layer;
            Mesh mesh = new Mesh();
            mesh.vertices = provincesBorders;
            mesh.SetIndices(provincesIndices, MeshTopology.Lines, 0);
            mesh.RecalculateBounds();
            mesh.hideFlags = HideFlags.DontSave;

            MeshFilter mf = borderSurface.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = borderSurface.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.sharedMaterial = material;
            return borderSurface;
        }

        GameObject CreateAreaSurface(Area area, Material material) {
            int[] surfaceIndices = new Triangulator(area.border).TriangulateOriented();
            // Make texture coordinates not depending of area position
            float chunkSize = 0.01f;
            float textureSizeKoef = chunkSize / area.borderRect.height;
            Vector2 textureScale = new Vector2(area.borderRect.height / area.borderRect.width * textureSizeKoef, textureSizeKoef);
            Vector2 surfaceOffset = new Vector2(area.borderRect.xMin - chunkSize * (int)(area.borderRect.xMin / chunkSize),
                area.borderRect.yMin - chunkSize * (int)(area.borderRect.yMin / chunkSize));
            Vector2 textureOffset = new Vector2(area.borderRect.xMin, area.borderRect.yMin);
            //Create Surface
            GameObject surface = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            surface.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            mesh.vertices = area.border3d;
            mesh.triangles = surfaceIndices;
            // uv mapping
            if (material.mainTexture != null) {
                Vector2[] uv = new Vector2[area.border.Length];
                for (int k = 0; k < uv.Length; k++) {
                    Vector2 coor = area.border[k];
                    coor.x /= textureScale.x;
                    coor.y /= textureScale.y;
                    coor += textureOffset;
                    Vector2 normCoor = new Vector2((coor.x - area.borderRect.xMin) / area.borderRect.width, (coor.y - area.borderRect.yMin) / area.borderRect.height);
                    uv[k] = normCoor;
                }
                mesh.uv = uv;
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshUtility.Optimize(mesh);

            MeshFilter meshFilter = surface.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            surface.GetComponent<Renderer>().sharedMaterial = material;
            surface.transform.SetParent(areaBackgroundObject.transform, false);
            surface.transform.localPosition = Vector3.zero;
            surface.transform.localRotation = Quaternion.Euler(Vector3.zero);
            surface.layer = gameObject.layer;
            return surface;
        }

    }

}

