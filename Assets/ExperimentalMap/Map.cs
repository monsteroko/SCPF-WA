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
        public GameObject mapObject;
        public Material areaBorderMaterial;
        public Material areaBackgroundMaterial;
        public Camera camera;
        List<Area> areas;

        void Start() {
            areas = ReadAreasData();
            CreateAreas();
        }

        float zoomAcceleration = 0;
        float dragAcceleration = 0;
        Vector3 dragDirection;
        Vector3? mouseDragFirst, mouseDragLast, mouseDragCurrent;
        Vector3 cameraPositionFirst;
        void Update() {
            if (!Application.isPlaying)
                return;
            bool isMouseOver = false;
            Vector3? mouseRaycastPoint = GetMouseRaycast();
            if (mouseRaycastPoint != null) {
                isMouseOver = true;
            }

            // Zoom
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
            // Drag
            bool pressed = false;
            Vector3 currentDragVector = Vector3.zero;
            float currentDragAcceleration = 0;
            if (Input.GetKey(KeyCode.W)) {
                currentDragVector += Vector3.up;
                pressed = true;
            }
            if (Input.GetKey(KeyCode.S)) {
                currentDragVector += Vector3.down;
                pressed = true;
            }
            if (Input.GetKey(KeyCode.A)) {
                currentDragVector += Vector3.left;
                pressed = true;
            }
            if (Input.GetKey(KeyCode.D)) {
                currentDragVector += Vector3.right;
                pressed = true;
            }
            if (pressed) {
                dragDirection = currentDragVector.normalized;
                const float keysDragSpeed = 0.05f;
                currentDragAcceleration += keysDragSpeed * Mathf.Sqrt(mapObject.transform.position.z - camera.transform.position.z);
            }
            if (isMouseOver && Input.GetMouseButton(0)) {
                if (mouseDragFirst == null) {
                    mouseDragFirst = mouseDragCurrent;
                    cameraPositionFirst = camera.transform.position;
                }
                mouseDragLast = mouseDragCurrent;
                mouseDragCurrent = GetMousePosition();
                if (mouseDragLast != null && mouseDragCurrent != null) {
                    Vector3? mapDragFirst = GetRaycast((Vector3)mouseDragFirst);
                    Vector3? mapDragCurrent = GetRaycast((Vector3)mouseDragCurrent);
                    Vector3 scrPointFirst = camera.ScreenToWorldPoint(new Vector3(((Vector3)mouseDragFirst).x, ((Vector3)mouseDragFirst).y, camera.nearClipPlane));
                    Vector3 scrPointCurrent = camera.ScreenToWorldPoint(new Vector3(((Vector3)mouseDragCurrent).x, ((Vector3)mouseDragCurrent).y, camera.nearClipPlane));
                    if (mapDragFirst != null && mapDragCurrent != null) {
                        float koef = (cameraPositionFirst - (Vector3)mapDragFirst).magnitude / (scrPointFirst - (Vector3)mapDragFirst).magnitude;
                        Debug.Log(koef);
                        Vector3 newCamera = (Vector3)mapDragFirst + (scrPointCurrent - (Vector3)mapDragFirst) * koef;
                        currentDragVector = newCamera - camera.transform.position;
                        dragDirection = currentDragVector.normalized;
                        currentDragAcceleration += currentDragVector.magnitude;
                    }
                } 
            } else {
                //if (mouseDragFirst != null && mouseDragLast != null) {
                //    dragDirection = ((Vector3)mouseDragFirst - (Vector3)mouseDragLast).normalized;
                //}
                mouseDragCurrent = null;
                mouseDragLast = null;
                mouseDragFirst = null;
            }
            dragAcceleration += currentDragAcceleration;
            if (dragAcceleration != 0) {
                if (currentDragAcceleration == 0) {
                    currentDragAcceleration = dragAcceleration;
                } else {
                    dragAcceleration = currentDragAcceleration;
                }
                camera.transform.Translate(dragDirection * currentDragAcceleration);
                dragAcceleration *= 0.9f;
                if (Mathf.Abs(dragAcceleration) < 1 / 1000.0f) {
                    dragAcceleration = 0;
                }
            }
        }

        Vector3 GetMousePosition() {
            Vector3 mousePos = Input.mousePosition;
            mousePos.x = Math.Max(Math.Min(mousePos.x, Screen.width), 0);
            mousePos.y = Math.Max(Math.Min(mousePos.y, Screen.height), 0);
            return mousePos;
        }

        Vector3? GetMouseRaycast() {
            return GetRaycast(GetMousePosition());
        }

        Vector3? GetRaycast(Vector3 point) {
            Ray ray = camera.ScreenPointToRay(point);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            if (hits.Length > 0) {
                for (int k = 0; k < hits.Length; k++) {
                    if (hits[k].collider.gameObject == mapObject) {
                        return hits[k].point;
                    }
                }
            }
            return null;
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

