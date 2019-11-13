﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExperimentalMap;
using UnityEditor;

namespace ExperimentalMap {

    public interface MapSurface {
        List<Vector2> border { get; }
        Rect borderRect { get; }
    }

    public class Area: MapSurface {
        public readonly string name;
        public readonly string counterpart;
        public Area(string name, string counterpart) {
            this.name = name;
            this.counterpart = counterpart;
            this.terrains = new List<Terrain>();
        }

        public List<Vector2> border { get; private set; }
        public Vector2 center;
        public Rect borderRect { get; private set; }
        public List<Terrain> terrains { get; private set; }

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
            terrains.Add(terrain);
            border = (new ClipperUtility()).UnionBorders(border, terrain.border);
        }

        public void ClipTerrain(Terrain terrain) {
            border = (new ClipperUtility()).ClipBorder(border, terrain.border);
        }
    }

    public class Map : MonoBehaviour {

        public static readonly float MapPrecision = 5000000f;
        public GameObject terrainsObject;
        public GameObject areaBackgroundObject;
        public GameObject mapBackgroundObject;
        public GameObject mapObject;
        public Material areaBorderMaterial;
        public Material areaBackgroundMaterial;
        public Material biom1Material;
        public Material biom2Material;
        public Material biom3Material;
        public Material seaMaterial;
        public Camera camera;
        List<Area> areas;
        List<Material> biomMaterials = new List<Material>();

        void Start() {
            biomMaterials.Add(biom1Material);
            biomMaterials.Add(biom2Material);
            biomMaterials.Add(biom3Material);
            CreateBackground();
            areas = ReadAreasData();
            CreateAreas();
            FitCameraPosition();
        }

        float zoomAcceleration = 0;
        float dragAcceleration = 0;
        Vector3 dragDirection;
        Vector3? mouseDragLast, mouseDragCurrent;
        Vector3 cameraPositionLast;
        const float maxCameraDistance = 90.0f;
        const float minCameraDistance = 5.0f;
        const float minCameraAngle = 0.0f;
        const float maxCameraAngle = -20.0f;
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
                camera.transform.Translate(mapObject.transform.forward * zoomSpeed * zoomSpeedMultiplier);
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
                cameraPositionLast = camera.transform.position;
                mouseDragLast = mouseDragCurrent;
                mouseDragCurrent = GetMousePosition();
                if (mouseDragLast != null && mouseDragCurrent != null) {
                    Vector3? mapDragLast = GetRaycast((Vector3)mouseDragLast);
                    Vector3? mapDragCurrent = GetRaycast((Vector3)mouseDragCurrent);
                    Vector3 scrPointLast = camera.ScreenToWorldPoint(new Vector3(((Vector3)mouseDragLast).x, ((Vector3)mouseDragLast).y, camera.nearClipPlane));
                    Vector3 scrPointCurrent = camera.ScreenToWorldPoint(new Vector3(((Vector3)mouseDragCurrent).x, ((Vector3)mouseDragCurrent).y, camera.nearClipPlane));
                    if (mouseDragLast != null && mapDragCurrent != null) {
                        float koef = (camera.transform.position - (Vector3)mapDragLast).magnitude / (scrPointLast - (Vector3)mapDragLast).magnitude;
                        Vector3 cameraRay = (scrPointCurrent - (Vector3)mapDragCurrent) * koef;
                        float distanceKoef = ((camera.transform.position.z - ((Vector3)mapDragLast).z) / cameraRay.z);
                        Vector3 newCamera = (Vector3)mapDragLast + cameraRay * distanceKoef;
                        currentDragVector = newCamera - camera.transform.position;
                        dragDirection = currentDragVector.normalized;
                        currentDragAcceleration += currentDragVector.magnitude;
                    }
                } 
            } else {
                mouseDragCurrent = null;
                mouseDragLast = null;
            }
            dragAcceleration += currentDragAcceleration;
            if (dragAcceleration != 0) {
                if (currentDragAcceleration == 0) {
                    currentDragAcceleration = dragAcceleration;
                } else {
                    dragAcceleration = currentDragAcceleration;
                }
                camera.transform.position = camera.transform.position + dragDirection * currentDragAcceleration;
                dragAcceleration *= 0.9f;
                if (Mathf.Abs(dragAcceleration) < 1 / 1000.0f) {
                    dragAcceleration = 0;
                }
            }
            FitCameraPosition();
        }

        void FitCameraPosition() {
            float pureDistance = Mathf.Clamp(mapObject.transform.position.z - camera.transform.position.z, minCameraDistance, maxCameraDistance);
            float zPosition = mapObject.transform.position.z - pureDistance;
            camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y, zPosition);
            float angle = minCameraAngle + maxCameraAngle * (1 - Mathf.Pow((pureDistance - minCameraDistance) / (maxCameraDistance - minCameraDistance), 0.7f));
            camera.transform.rotation = Quaternion.Euler(angle, 0, 0);
            //TODO: Restrict x and y camera movement
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
            List<Area> europeAreas = new List<Area>();
            List<Area> otherAreas = new List<Area>();
            foreach (Area area in areas) {
                if (area.counterpart == "SLAV EMPIRE") {
                    europeAreas.Add(area);
                } else {
                    otherAreas.Add(area);
                }
            }
            new TerrainLayouter().CreateLayoutForAreas(ref europeAreas);
            foreach (Area area in europeAreas) {
                CreateAreaSurface(area, biom1Material, areaBackgroundObject);
                CreateAreaTerrainsSurfaces(area);
                CreateAreaBorder(area, areaBorderMaterial); 
            }
            foreach (Area area in otherAreas) {
                CreateAreaSurface(area, areaBackgroundMaterial, areaBackgroundObject);
                CreateAreaBorder(area, areaBorderMaterial);
            }
        }

        GameObject CreateAreaBorder(MapSurface surface, Material material) {
            List<Vector3> frontiersPoints = new List<Vector3>();
            int pointsCount = surface.border.Count;
            for (int i1 = 0; i1 < pointsCount - 1; i1++) {
                Vector3 p0 = surface.border[i1];
                Vector3 p1 = surface.border[i1 + 1];
                double v = (p0.x + p1.x) + MapPrecision * (p0.y + p1.y);
                frontiersPoints.Add(p0);
                frontiersPoints.Add(p1);
            }
            frontiersPoints.Add(surface.border[pointsCount - 1]);
            frontiersPoints.Add(surface.border[0]);

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

        GameObject CreateAreaSurface(MapSurface surface, Material material, GameObject parentObject) {
            int[] surfaceIndices = new Triangulator(surface.border).TriangulateOriented();
            Vector3[] border3 = new Vector3[surface.border.Count];
            for (int i1 = 0; i1 < surface.border.Count; i1++) {
                border3[i1] = surface.border[i1];
            }
            // Make texture coordinates not depending of area position
            float chunkSize = 0.01f;
            float textureSizeKoef = chunkSize / surface.borderRect.height;
            Vector2 textureScale = new Vector2(surface.borderRect.height / surface.borderRect.width * textureSizeKoef, textureSizeKoef);
            Vector2 surfaceOffset = new Vector2(surface.borderRect.xMin - chunkSize * (int)(surface.borderRect.xMin / chunkSize),
                surface.borderRect.yMin - chunkSize * (int)(surface.borderRect.yMin / chunkSize));
            Vector2 textureOffset = new Vector2(surface.borderRect.xMin, surface.borderRect.yMin);
            //Create Surface
            GameObject surfaceObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            surfaceObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            Mesh mesh = new Mesh();
            mesh.hideFlags = HideFlags.DontSave;
            mesh.vertices = border3;
            mesh.triangles = surfaceIndices;
            // uv mapping
            if (material.mainTexture != null) {
                Vector2[] uv = new Vector2[surface.border.Count];
                for (int k = 0; k < uv.Length; k++) {
                    Vector2 coor = surface.border[k];
                    coor.x /= textureScale.x;
                    coor.y /= textureScale.y;
                    coor += textureOffset;
                    Vector2 normCoor = new Vector2((coor.x - surface.borderRect.xMin) / surface.borderRect.width, (coor.y - surface.borderRect.yMin) / surface.borderRect.height);
                    uv[k] = normCoor;
                }
                mesh.uv = uv;
            }
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshUtility.Optimize(mesh);

            MeshFilter meshFilter = surfaceObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            surfaceObject.GetComponent<Renderer>().sharedMaterial = material;
            surfaceObject.transform.SetParent(parentObject.transform, false);
            surfaceObject.transform.localPosition = Vector3.zero;
            surfaceObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            surfaceObject.layer = gameObject.layer;
            return surfaceObject;
        }

        List<GameObject> CreateAreaTerrainsSurfaces(Area area) {
            List<GameObject> surfaces = new List<GameObject>();
            foreach (Terrain terrain in area.terrains) {
                surfaces.Add(CreateAreaSurface(terrain, biomMaterials[terrain.type], terrainsObject));
            }
            return surfaces;
        }

        GameObject CreateBackground() {
            List<Vector2> border = new List<Vector2>();
            border.Add(new Vector2(-1, -1));
            border.Add(new Vector2(1, -1));
            border.Add(new Vector2(1, 1));
            border.Add(new Vector2(-1, 1));
            Terrain sea = new Terrain(border);
            return CreateAreaSurface(sea, seaMaterial, mapBackgroundObject);
        }

    }

}

