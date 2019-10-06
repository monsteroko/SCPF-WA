// World Political Map - 2D Edition for Unity - Main Script
// Copyright 2015-2017 Kronnect
// Don't modify this script - changes could be lost if you upgrade to a more recent version of WPM

//#define TRACE_CTL
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WPMF
{
    public delegate void OnProvinceEvent(int provinceIndex, int regionIndex);

    public delegate void OnProvinceHighlightEvent(int provinceIndex, int regionIndex, ref bool allowHighlight);

    public partial class WorldMap2D : MonoBehaviour
    {

        #region Public properties

        Province[] _provinces;

        /// <summary>
        /// Complete array of states and provinces and the country name they belong to.
        /// </summary>
        public Province[] provinces {
            get {
                if (_provinces == null)
                    ReadProvincesPackedString();
                return _provinces;
            }
            set {
                _provinces = value;
                lastProvinceLookupCount = -1;
            }
        }

        Province _provinceHighlighted;

        /// <summary>
        /// Returns Province under mouse position or null if none.
        /// </summary>
        public Province provinceHighlighted { get { return _provinceHighlighted; } }

        int _provinceHighlightedIndex = -1;

        /// <summary>
        /// Returns current highlighted province index.
        /// </summary>
        public int provinceHighlightedIndex { get { return _provinceHighlightedIndex; } }

        Region _provinceRegionHighlighted;

        /// <summary>
        /// Returns currently highlightd province's region.
        /// </summary>
        /// <value>The country region highlighted.</value>
        public Region provinceRegionHighlighted { get { return _provinceRegionHighlighted; } }

        int _provinceRegionHighlightedIndex = -1;

        /// <summary>
        /// Returns current highlighted province's region index.
        /// </summary>
        public int provinceRegionHighlightedIndex { get { return _provinceRegionHighlightedIndex; } }

        int _provinceLastClicked = -1;

        /// <summary>
        /// Returns the last clicked province index.
        /// </summary>
        public int provinceLastClicked { get { return _provinceLastClicked; } }

        int _provinceRegionLastClicked = -1;

        /// <summary>
        /// Returns the last clicked province region index.
        /// </summary>
        public int provinceRegionLastClicked { get { return _provinceRegionLastClicked; } }

        public event OnProvinceEvent OnProvinceEnter;
        public event OnProvinceEvent OnProvinceExit;
        public event OnProvinceEvent OnProvinceClick;
        public event OnProvinceHighlightEvent OnProvinceHighlight;

        [SerializeField]
        bool
                        _showProvinces = false;

        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showProvinces {
            get {
                return _showProvinces;
            }
            set {
                if (value != _showProvinces)
                {
                    _showProvinces = value;
                    isDirty = true;

                    if (_showProvinces)
                    {
                        if (provinces == null)
                        {
                            ReadProvincesPackedString();
                        }
                        if (_drawAllProvinces)
                        {
                            DrawAllProvinceBorders(true);
                        }
                    }
                    else
                    {
                        HideProvinces();
                    }
                }
            }
        }

        [SerializeField]
        bool
                        _enableProvinceHighlight = true;

        /// <summary>
        /// Enable/disable province highlight when mouse is over and ShowProvinces is true.
        /// </summary>
        public bool enableProvinceHighlight {
            get {
                return _enableProvinceHighlight;
            }
            set {
                if (_enableProvinceHighlight != value)
                {
                    _enableProvinceHighlight = value;
                    isDirty = true;
                }
            }
        }

        [SerializeField]
        bool
                        _drawAllProvinces = false;

        /// <summary>
        /// Forces drawing of all provinces and not only thouse of currently selected country.
        /// </summary>
        public bool drawAllProvinces {
            get {
                return _drawAllProvinces;
            }
            set {
                if (value != _drawAllProvinces)
                {
                    _drawAllProvinces = value;
                    isDirty = true;
                    DrawAllProvinceBorders(true);
                }
            }
        }

        /// <summary>
        /// Fill color to use when the mouse hovers a country's region.
        /// </summary>
        [SerializeField]
        Color
                        _provincesFillColor = new Color(0, 0, 1, 0.7f);

        public Color provincesFillColor {
            get {
                if (hudMatProvince != null)
                {
                    return hudMatProvince.color;
                }
                else
                {
                    return _provincesFillColor;
                }
            }
            set {
                if (value != _provincesFillColor)
                {
                    _provincesFillColor = value;
                    isDirty = true;
                    if (hudMatProvince != null && _provincesFillColor != hudMatProvince.color)
                    {
                        hudMatProvince.color = _provincesFillColor;
                    }
                }
            }
        }

        /// <summary>
        /// Global color for provinces.
        /// </summary>
        [SerializeField]
        Color
                        _provincesColor = Color.white;

        public Color provincesColor {
            get {
                if (provincesMat != null)
                {
                    return provincesMat.color;
                }
                else
                {
                    return _provincesColor;
                }
            }
            set {
                if (value != _provincesColor)
                {
                    _provincesColor = value;
                    isDirty = true;

                    if (provincesMat != null && _provincesColor != provincesMat.color)
                    {
                        provincesMat.color = _provincesColor;
                    }
                }
            }
        }

        #endregion

        #region Public API area

        /// <summary>
        /// Draws the borders of the provinces/states a country by its id. Returns true is country is found, false otherwise.
        /// </summary>
        public bool DrawProvinces(int countryIndex, bool includeNeighbours, bool forceRefresh)
        {
            if (countryIndex >= 0)
            {
                return mDrawProvinces(countryIndex, includeNeighbours, forceRefresh);
            }
            return false;
        }

        /// <summary>
        /// Hides the borders of all provinces/states.
        /// </summary>
        public void HideProvinces()
        {
            if (provincesObj != null)
                DestroyImmediate(provincesObj);
            countryProvincesDrawnIndex = -1;
            HideProvinceRegionHighlight();
        }


        /// <summary>
        /// Returns the index of a province in the provinces array by its reference.
        /// </summary>
        public int GetProvinceIndex(Province province)
        {
            if (provinceLookup.ContainsKey(province))
                return _provinceLookup[province];
            else
                return -1;
        }

        /// <summary>
        /// Returns the index of a province in the global provinces array.
        /// </summary>
        public int GetProvinceIndex(int countryIndex, string provinceName)
        {
            Country country = countries[countryIndex];
            if (country.provinces == null)
            {
                ReadProvincesPackedString();
            }
            if (country.provinces == null)
                return -1;
            for (int k = 0; k < country.provinces.Length; k++)
            {
                if (country.provinces[k].name.Equals(provinceName))
                {
                    return GetProvinceIndex(country.provinces[k]);
                }
            }
            return -1;
        }


        /// <summary>
        /// Gets the index of the province that contains the provided map coordinates. This will ignore hidden countries.
        /// </summary>
        /// <returns>The province index.</returns>
        /// <param name="localPosition">Map coordinates in the range of (-0.5 .. 0.5)</param>
        public int GetProvinceIndex(Vector2 localPosition)
        {
            // verify if hitPos is inside any country polygon
            int provinceIndex, provinceRegionIndex;
            if (GetProvinceRegionIndex(localPosition, out provinceIndex, out provinceRegionIndex))
            {
                return provinceIndex;
            }
            return -1;
        }

        /// <summary>
        /// Gets the province that contains a given map coordinate or the province whose center is nearest to that coordinate.
        /// </summary>
        public int GetProvinceNearPoint(Vector2 localPosition)
        {
            int provinceIndex = GetProvinceIndex(localPosition);
            if (provinceIndex >= 0)
                return provinceIndex;
            float minDist = float.MaxValue;
            for (int k = 0; k < _provinces.Length; k++)
            {
                float dist = Vector2.SqrMagnitude(_provinces[k].center - localPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    provinceIndex = k;
                }
            }
            return provinceIndex;
        }

        /// <summary>
        /// Gets the index of the province and region that contains the provided map coordinates. This will ignore hidden countries.
        /// </summary>
        public bool GetProvinceRegionIndex(Vector2 localPosition, out int provinceIndex, out int provinceRegionIndex)
        {
            provinceIndex = -1;
            provinceRegionIndex = -1;
            int countryIndex = GetCountryIndex(localPosition);
            if (countryIndex >= 0)
            {
                Country country = countries[countryIndex];
                if (country.provinces == null)
                {
                    ReadProvincesPackedString();
                    if (country.provinces == null)
                        return false;
                }
                for (int p = 0; p < country.provinces.Length; p++)
                {
                    Province province = country.provinces[p];
                    if (province.regions == null)
                        ReadProvincePackedString(province);
                    if (!province.regionsRect2D.Contains(localPosition))
                        continue;
                    for (int pr = 0; pr < province.regions.Count; pr++)
                    {
                        if (province.regions[pr].Contains(localPosition))
                        {
                            provinceIndex = GetProvinceIndex(province);
                            provinceRegionIndex = pr;
                            return true;
                        }
                    }
                }
                // Look for rogue provinces (province that is surrounded by another country)
                for (int p = 0; p < provinces.Length; p++)
                {
                    Province province = provinces[p];
                    if (province.regions == null)
                        ReadProvincePackedString(province);
                    if (!province.regionsRect2D.Contains(localPosition))
                        continue;
                    for (int pr = 0; pr < province.regions.Count; pr++)
                    {
                        if (province.regions[pr].Contains(localPosition))
                        {
                            provinceIndex = p;
                            provinceRegionIndex = pr;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the province index by screen position.
        /// </summary>
        public bool GetProvinceIndex(Ray ray, out int provinceIndex, out int regionIndex)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, 500, layerMask);
            if (provinces != null && hits.Length > 0)
            {
                for (int k = 0; k < hits.Length; k++)
                {
                    if (hits[k].collider.gameObject == gameObject)
                    {
                        Vector2 localHit = transform.InverseTransformPoint(hits[k].point);
                        for (int c = 0; c < provinces.Length; c++)
                        {
                            if (provinces[c].regions != null)
                            {
                                for (int cr = 0; cr < provinces[c].regions.Count; cr++)
                                {
                                    Region region = provinces[c].regions[cr];
                                    if (region.Contains(localHit))
                                    {
                                        provinceIndex = c;
                                        regionIndex = cr;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            provinceIndex = -1;
            regionIndex = -1;
            return false;
        }


        /// <summary>
        /// Returns all neighbour provinces
        /// </summary>
        public List<Province> ProvinceNeighbours(int provinceIndex)
        {

            List<Province> provinceNeighbours = new List<Province>();

            // Get country object
            Province province = provinces[provinceIndex];
            if (province.regions == null)
                ReadProvincePackedString(province);

            // Iterate for all regions (a country can have several separated regions)
            for (int provinceRegionIndex = 0; provinceRegionIndex < province.regions.Count; provinceRegionIndex++)
            {
                Region provinceRegion = province.regions[provinceRegionIndex];

                // Get the neighbours for this region
                for (int neighbourIndex = 0; neighbourIndex < provinceRegion.neighbours.Count; neighbourIndex++)
                {
                    Region neighbour = provinceRegion.neighbours[neighbourIndex];
                    Province neighbourProvince = (Province)neighbour.entity;
                    if (!provinceNeighbours.Contains(neighbourProvince))
                    {
                        provinceNeighbours.Add(neighbourProvince);
                    }
                }
            }

            return provinceNeighbours;
        }


        /// <summary>
        /// Get neighbours of the main region of a province
        /// </summary>
        public List<Province> ProvinceNeighboursOfMainRegion(int provinceIndex)
        {

            List<Province> provinceNeighbours = new List<Province>();

            // Get main region
            Province province = provinces[provinceIndex];
            Region provinceRegion = province.regions[province.mainRegionIndex];

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < provinceRegion.neighbours.Count; neighbourIndex++)
            {
                Region neighbour = provinceRegion.neighbours[neighbourIndex];
                Province neighbourProvince = (Province)neighbour.entity;
                if (!provinceNeighbours.Contains(neighbourProvince))
                {
                    provinceNeighbours.Add(neighbourProvince);
                }
            }
            return provinceNeighbours;
        }


        /// <summary>
        /// Get neighbours of the currently selected region
        /// </summary>
        public List<Province> ProvinceNeighboursOfCurrentRegion()
        {

            List<Province> provinceNeighbours = new List<Province>();

            // Get main region
            Region selectedRegion = provinceRegionHighlighted;
            if (selectedRegion == null)
                return provinceNeighbours;

            // Get the neighbours for this region
            for (int neighbourIndex = 0; neighbourIndex < selectedRegion.neighbours.Count; neighbourIndex++)
            {
                Region neighbour = selectedRegion.neighbours[neighbourIndex];
                Province neighbourProvince = (Province)neighbour.entity;
                if (!provinceNeighbours.Contains(neighbourProvince))
                {
                    provinceNeighbours.Add(neighbourProvince);
                }
            }
            return provinceNeighbours;
        }

        /// <summary>
        /// Renames the province. Name must be unique, different from current and one letter minimum.
        /// </summary>
        /// <returns><c>true</c> if country was renamed, <c>false</c> otherwise.</returns>
        public bool ProvinceRename(int countryIndex, string oldName, string newName)
        {
            if (newName == null || newName.Length == 0)
                return false;
            int provinceIndex = GetProvinceIndex(countryIndex, oldName);
            int newProvinceIndex = GetProvinceIndex(countryIndex, newName);
            if (provinceIndex < 0 || newProvinceIndex >= 0)
                return false;
            provinces[provinceIndex].name = newName;
            lastProvinceLookupCount = -1;
            return true;

        }

        /// <summary>
        /// Adds a new province which has been properly initialized. Used by the Map Editor. Name must be unique.
        /// </summary>
        /// <returns><c>true</c> if province was added, <c>false</c> otherwise.</returns>
        public bool ProvinceAdd(Province province)
        {
            if (province.countryIndex < 0 || province.countryIndex >= countries.Length)
                return false;
            Province[] newProvinces = new Province[provinces.Length + 1];
            for (int k = 0; k < provinces.Length; k++)
            {
                newProvinces[k] = provinces[k];
            }
            newProvinces[newProvinces.Length - 1] = province;
            provinces = newProvinces;
            lastProvinceLookupCount = -1;
            // add the new province to the country internal list
            Country country = countries[province.countryIndex];
            if (country.provinces == null)
                country.provinces = new Province[0];
            Province[] newCountryProvinces = new Province[country.provinces.Length + 1];
            for (int k = 0; k < country.provinces.Length; k++)
            {
                newCountryProvinces[k] = country.provinces[k];
            }
            newCountryProvinces[newCountryProvinces.Length - 1] = province;
            country.provinces = newCountryProvinces;
            return true;
        }



        /// <summary>
        /// Flashes specified province by index in the global province array.
        /// </summary>
        public void BlinkProvince(int provinceIndex, Color color1, Color color2, float duration, float blinkingSpeed)
        {
            int mainRegionIndex = provinces[provinceIndex].mainRegionIndex;
            BlinkProvince(provinceIndex, mainRegionIndex, color1, color2, duration, blinkingSpeed);
        }

        /// <summary>
        /// Flashes specified province's region.
        /// </summary>
        public void BlinkProvince(int provinceIndex, int regionIndex, Color color1, Color color2, float duration, float blinkingSpeed)
        {
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            GameObject surf;
            bool disableAtEnd;
            if (surfaces.ContainsKey(cacheIndex))
            {
                surf = surfaces[cacheIndex];
                disableAtEnd = !surf.activeSelf;
            }
            else
            {
                surf = GenerateProvinceRegionSurface(provinceIndex, regionIndex, hudMatProvince);
                disableAtEnd = true;
            }
            SurfaceBlinker sb = surf.AddComponent<SurfaceBlinker>();
            sb.blinkMaterial = hudMatCountry;
            sb.color1 = color1;
            sb.color2 = color2;
            sb.duration = duration;
            sb.speed = blinkingSpeed;
            sb.disableAtEnd = disableAtEnd;
            sb.customizableSurface = provinces[provinceIndex].regions[regionIndex];
            surf.SetActive(true);
        }

        /// <summary>
        /// Starts navigation to target province/state by index in the provinces collection and duration in seconds.
        /// </summary>
        public bool FlyToProvince(int provinceIndex, float duration = -1, float zoomLevel = -1)
        {
            if (provinces == null || provinceIndex < 0 || provinceIndex >= provinces.Length)
                return false;
            SetDestination(provinces[provinceIndex].center, duration, zoomLevel);
            return true;
        }

        /// <summary>
        /// Starts navigation to target province/state. Returns false if not found.
        /// </summary>
        public bool FlyToProvince(string name, float duration = -1, float zoomLevel = -1)
        {
            for (int k = 0; k < provinces.Length; k++)
            {
                if (name.Equals(provinces[k].name))
                {
                    FlyToProvince(k, duration, zoomLevel);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Starts navigation to target province. with specified duration and zoom level, ignoring NavigationTime property.
        /// Set duration to zero to go instantly.
        /// Returns false if country is not found. 
        /// </summary>
        public bool FlyToProvince(string countryName, string provinceName, float duration = -1, float zoomLevel = -1)
        {
            int countryIndex = GetCountryIndex(countryName);
            if (countryIndex < 0)
                return false;
            int provinceIndex = GetProvinceIndex(countryIndex, provinceName);
            if (provinceIndex < 0)
                return false;
            return FlyToProvince(provinceIndex, duration, zoomLevel);
        }

        /// <summary>
        /// Colorize all regions of specified province/state. Returns false if not found.
        /// </summary>
        public bool ToggleProvinceSurface(string name, bool visible, Color color)
        {
            for (int c = 0; c < provinces.Length; c++)
            {
                if (provinces[c].name.Equals(name))
                {
                    ToggleProvinceSurface(c, visible, color);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Colorize all regions of specified province/state by index in the global provinces collection.
        /// </summary>
        public void ToggleProvinceSurface(int provinceIndex, bool visible, Color color)
        {
            if (!visible)
            {
                HideProvinceSurfaces(provinceIndex);
                return;
            }
            if (provinces[provinceIndex].regions == null)
                ReadProvincePackedString(provinces[provinceIndex]);
            for (int r = 0; r < provinces[provinceIndex].regions.Count; r++)
                ToggleProvinceRegionSurface(provinceIndex, r, visible, color);
        }


        /// <summary>
        /// Highlights the province region specified.
        /// Internally used by the Editor component, but you can use it as well to temporarily mark a country region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        public GameObject ToggleProvinceRegionSurfaceHighlight(int provinceIndex, int regionIndex, Color color)
        {
            GameObject surf;
            Material mat = Instantiate(hudMatProvince);
            mat.hideFlags = HideFlags.DontSave;
            mat.color = color;
            mat.renderQueue--;
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            bool existsInCache = surfaces.ContainsKey(cacheIndex);
            if (existsInCache)
            {
                surf = surfaces[cacheIndex];
                if (surf == null)
                {
                    surfaces.Remove(cacheIndex);
                }
                else
                {
                    surf.SetActive(true);
                    surf.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
            else
            {
                surf = GenerateProvinceRegionSurface(provinceIndex, regionIndex, mat);
            }
            return surf;
        }

        /// <summary>
        /// Disables all province regions highlights. This doesn't destroy custom materials.
        /// </summary>
        public void HideProvinceRegionHighlights(bool destroyCachedSurfaces)
        {
            HideProvinceRegionHighlight();
            if (provinces == null)
                return;
            for (int c = 0; c < provinces.Length; c++)
            {
                Province province = provinces[c];
                if (province == null || province.regions == null)
                    continue;
                for (int cr = 0; cr < province.regions.Count; cr++)
                {
                    Region region = province.regions[cr];
                    int cacheIndex = GetCacheIndexForProvinceRegion(c, cr);
                    if (surfaces.ContainsKey(cacheIndex))
                    {
                        GameObject surf = surfaces[cacheIndex];
                        if (surf == null)
                        {
                            surfaces.Remove(cacheIndex);
                        }
                        else
                        {
                            if (destroyCachedSurfaces)
                            {
                                surfaces.Remove(cacheIndex);
                                DestroyImmediate(surf);
                            }
                            else
                            {
                                if (region.customMaterial == null)
                                {
                                    surf.SetActive(false);
                                }
                                else
                                {
                                    ApplyMaterialToSurface(surf, region.customMaterial);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Colorize a region of specified province/state by index in the provinces collection.
        /// </summary>
        public void ToggleProvinceRegionSurface(int provinceIndex, int regionIndex, bool visible, Color color)
        {
            if (!visible)
            {
                HideProvinceRegionSurface(provinceIndex, regionIndex);
                return;
            }
            Material coloredMat = lockedMat;
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            if (surfaces.ContainsKey(cacheIndex) && surfaces[cacheIndex] != null)
            {
                surfaces[cacheIndex].SetActive(visible);
                // don't colorize while it's highlighted - it will revert to colorize when finish the highlight
                if (_provinceHighlightedIndex != provinceIndex || _provinceRegionHighlightedIndex != regionIndex || !_enableProvinceHighlight)
                {
                    surfaces[cacheIndex].GetComponent<Renderer>().sharedMaterial = coloredMat;
                }
            }
            else
            {
                GenerateProvinceRegionSurface(provinceIndex, regionIndex, coloredMat);
            }
            provinces[provinceIndex].regions[regionIndex].customMaterial = coloredMat;
        }

        /// <summary>
        /// Hides all colorized regions of all provinces/states.
        /// </summary>
        public void HideProvinceSurfaces()
        {
            if (provinces == null)
                return;
            for (int p = 0; p < provinces.Length; p++)
            {
                HideProvinceSurfaces(p);
            }
        }


        /// <summary>
        /// Hides all colorized regions of one province/state.
        /// </summary>
        public void HideProvinceSurfaces(int provinceIndex)
        {
            if (provinces[provinceIndex].regions == null)
                return;
            for (int r = 0; r < provinces[provinceIndex].regions.Count; r++)
            {
                HideProvinceRegionSurface(provinceIndex, r);
            }
        }

        /// <summary>
        /// Hides all regions of one province.
        /// </summary>
        public void HideProvinceRegionSurface(int provinceIndex, int regionIndex)
        {
            int cacheIndex = GetCacheIndexForProvinceRegion(provinceIndex, regionIndex);
            if (surfaces.ContainsKey(cacheIndex))
            {
                surfaces[cacheIndex].SetActive(false);
            }
        }

        /// <summary>
        /// Returns an array of province names. The returning list can be grouped by country.
        /// </summary>
        public string[] GetProvinceNames(bool groupByCountry)
        {
            List<string> c = new List<string>(provinces.Length + countries.Length);
            if (provinces == null)
                return c.ToArray();
            bool[] countriesAdded = new bool[countries.Length];
            for (int k = 0; k < provinces.Length; k++)
            {
                Province province = provinces[k];
                if (province != null)
                { // could be null if country doesn't exist in this level of quality
                    if (groupByCountry)
                    {
                        if (!countriesAdded[province.countryIndex])
                        {
                            countriesAdded[province.countryIndex] = true;
                            c.Add(countries[province.countryIndex].name);
                        }
                        c.Add(countries[province.countryIndex].name + "|" + province.name + " (" + k + ")");
                    }
                    else
                    {
                        c.Add(province.name + " (" + k + ")");

                    }
                }
            }
            c.Sort();

            if (groupByCountry)
            {
                int k = -1;
                while (++k < c.Count)
                {
                    int i = c[k].IndexOf('|');
                    if (i > 0)
                    {
                        c[k] = "  " + c[k].Substring(i + 1);
                    }
                }
            }
            return c.ToArray();
        }


        /// <summary>
        /// Returns an array of province names for the specified country.
        /// </summary>
        public string[] GetProvinceNames(int countryIndex)
        {
            List<string> c = new List<string>(100);
            if (countryIndex < 0 || countryIndex >= countries.Length || countries[countryIndex].provinces == null)
                return c.ToArray();
            for (int k = 0; k < countries[countryIndex].provinces.Length; k++)
            {
                Province province = countries[countryIndex].provinces[k];
                c.Add(province.name + " (" + GetProvinceIndex(province) + ")");
            }
            c.Sort();
            return c.ToArray();
        }

        /// <summary>
        /// Returns an array of province objects for the specified country.
        /// </summary>
        public Province[] GetProvinces(int countryIndex)
        {
            List<Province> c = new List<Province>(100);
            if (provinces == null || countryIndex < 0 || countryIndex >= countries.Length)
                return c.ToArray();
            for (int k = 0; k < provinces.Length; k++)
            {
                Province province = provinces[k];
                if (province.countryIndex == countryIndex)
                {
                    c.Add(province);
                }
            }
            return c.ToArray();
        }



        /// <summary>
        /// Returns a list of provinces whose center is contained in a given region
        /// </summary>
        public List<Province> GetProvinces(Region region)
        {
            int provCount = provinces.Length;
            List<Province> cc = new List<Province>();
            for (int k = 0; k < provCount; k++)
            {
                if (region.Contains(_provinces[k].center))
                    cc.Add(_provinces[k]);
            }
            return cc;
        }

        /// <summary>
        /// Delete all provinces from specified continent.
        /// </summary>
        public void ProvincesDeleteOfSameContinent(string continentName)
        {
            HideProvinceRegionHighlights(true);
            if (provinces == null)
                return;
            int numProvinces = _provinces.Length;
            List<Province> newProvinces = new List<Province>(numProvinces);
            for (int k = 0; k < numProvinces; k++)
            {
                if (_provinces[k] != null)
                {
                    int c = _provinces[k].countryIndex;
                    if (!countries[c].continent.Equals(continentName))
                    {
                        newProvinces.Add(_provinces[k]);
                    }
                }
            }
            provinces = newProvinces.ToArray();
        }

        /// <summary>
        /// Delete all provinces from speficied country.
        /// </summary>
        public void ProvincesDelete(int countryIndex)
        {
            if (provinces == null)
                return;
            int numProvinces = provinces.Length;
            List<Province> newProvinces = new List<Province>(numProvinces);
            for (int k = 0; k < numProvinces; k++)
            {
                if (provinces[k] != null && provinces[k].countryIndex != countryIndex)
                {
                    newProvinces.Add(provinces[k]);
                }
            }
            provinces = newProvinces.ToArray();
        }


        /// <summary>
        /// Returns the zoom level required to show the entire province region on screen
        /// </summary>
        /// <returns>The province zoom level of -1 if error.</returns>
        /// <param name="provinceIndex">Province index.</param>
        public float GetProvinceRegionZoomExtents(int provinceIndex)
        {
            if (provinceIndex < 0 || provinces == null || provinceIndex >= provinces.Length)
                return -1;
            return GetProvinceRegionZoomExtents(provinceIndex, provinces[provinceIndex].mainRegionIndex);
        }

        /// <summary>
        /// Returns the zoom level required to show the entire province region on screen
        /// </summary>
        /// <returns>The province zoom level of -1 if error.</returns>
        /// <param name="provinceIndex">Country index.</param>
        /// <param name="regionIndex">Region index of the country.</param>
        public float GetProvinceRegionZoomExtents(int provinceIndex, int regionIndex)
        {
            if (provinceIndex < 0 || provinces == null || provinceIndex >= provinces.Length)
                return -1;
            Province province = provinces[provinceIndex];
            if (regionIndex < 0 || regionIndex >= province.regions.Count)
                return -1;
            Region region = province.regions[regionIndex];
            return GetFrustumZoomLevel(region.rect2D.width * mapWidth, region.rect2D.height * mapHeight);
        }


        #endregion

    }

}