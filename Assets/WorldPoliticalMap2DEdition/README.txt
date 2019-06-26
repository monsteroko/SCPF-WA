************************************
* WORLD POLITICAL MAP - 2D EDITION *
*           README FILE            *
************************************


How to use this asset
---------------------
Firstly, you should run the Demo Scene provided to get an idea of the overall functionality.
Later, you should read the documentation and experiment with the API/prefabs.


Demo Scene
----------
There're several demo scenes, located in "Demo" folder. Just go there from Unity, open and run them to get an overall idea of the asset main features.


Documentation/API reference
---------------------------
The PDF is located in the Doc folder. It contains instructions on how to use the prefab and the API so you can control it from your code.


Support
-------
Please read the documentation PDF and browse/play with the demo scene and sample source code included before contacting us for support :-)

* Support: contact@kronnect.me
* Website-Forum: http://kronnect.me
* Twitter: @KronnectGames


Version history
---------------

Version 5.9.3 Current version
  - Cosmetic changes to the inspector (also makes it more lightweight)
  - [Fix] Fixed harmless shader console warning
  - [Fix] Fixed outline mesh packing issue

Version 5.9.2 2018.03.05
  - WPM 2D Edition now requires Unity 5.5
  - [Fix] Fixed issue when texturing a country and include all regions option is enabled
  - [Fix] Map Editor: fixed regression issue with country/continent delete command

Version 5.9.1 
  - [Fix] Fixed text encoding when saving changes from Map Editor

Version 5.9 
  - Support for multiple viewports. New demo scene 5
  - Demo scene 1: example to show specific country provinces and names
  - API: country property: allowShowProvinces
  - API: added variations to FlyToProvince method (duration and zoomLevel now accepted as arguments)
  - Added internal check to preserve selections on map when pointer is over another game object
  
Version 5.8.1 2017.10.06
  - Added Drag Threshold option to prevent accidental drag on high Dpi touch screens
  - [Fix] Added CultureInfo.InvariantCulture to all float parsing instructions to support foreign languages
  
Version 5.8 2017.07.31
  - Decorators: option to apply texture to all regions
  - Some fixes and improvements related to NGUI integration
  - Render Viewport: added filtering mode (Bilinear / Trilinear)  
  - API: New FlyToCountry overloads which allow custom destination zoom level
  - API: New GetCountryRegionZoomExtents / GetProvinceRegionZoomExtents functions which return the zoom level required to show a country or province within screen borders
  - API: Added OnDragStart / OnDragEnd events
  - [Fix] Fixed camera position issue when distance to map is reduced below zoom minimum distance 
  - [Fix] Removed visual glitch when highlighting a decorated country
  
Version 5.7 2017.06.27
  - Added new City Class filter under Cities Settings
  - Support for NGUI clicks and highlighting in viewport mode
  - Map Editor: added "Keep Nearby Frontiers" option in Reshape command
  - Map Editor: added "Transfer Region" vs "Transfer Country" commands
  - Map Editor: new Regions group under Country and Province sections in inspector which allows to quickly select, remove and isolate regions
  - Added option to assign a different default camera
  - [Fix] Added Belgrade (Serbia capital)

Version 5.6 2017.05.23
  - Map Editor: deleting a province now removes the corresponding land portion at the country level
  - Map Editor: new option for separating provinces into a new country
  - [Fix] Map Editor: transfer province operation was not including cities nor mount points
  - [Fix] Map Editor: prevent save error when mount points file is missing
  
Version 5.5.3 2017.04.19
  - [Fix] Map Editor: fixed crash when transferring a province to a newly created country
  
Version 5.5.2 2017.04.10
  - [Fix] Fixed Map Editor console warnings in Unity 5.6

Version 5.5.1 2017.02.09
  - [Fix] Fixed cursor shader error during build

Version 5.5 - 2017.02.01
  - Redesigned custom inspector
  - Fixed conflict with PUN+ namespace
  - Changes in inspector now mark the scene as unsaved
  - [Fix] compatibility issues with Unity 5.5f3
  - [Fix] Map Editor: fixed provinces issue when renaming country
  - [Fix] iOS: fixed drag with constant move option enabled
  
Version 5.4 - 2016.12.02
  - Added OnCountryHighlight / OnProvinceHighlight events (which allows further control of highlighting)
  - 2 new high resolution 16K styles: Natural HighRes 16K and Scenic 16K.
  - Optimized outline drawing - meshes are now packed so less draw calls are required
  - Added zoomConstantSpeed option
  - Map Editor: added new hotkeys to country creation tool
  - Viewport camera is now disabled as well when WMSK object is disabled so it does not waste ms
  - [Fix] Map editor: transferring independent provinces was not removing corresponding country region
  - [Fix] jitter issue when enabling constant drag speed and static camera is set to false
  - [Fix] drag + zoom issue when static camera and constant drag speed were enabled
  - [Fix] country neighbours detection between Egypt and Israel
 
Version 5.3 - 2016.07.30
  - Experimental NGUI support using viewports
  - API: cursorLocation property now always return the location under mouse position even when cursor option is disabled
  - New option (staticCamera) to choose if it's the camera or the map what moves when user drags or zooms in/out
  - Decorators: new option to specify if all regions are included when coloring/texturing a country
  - [Fix] markers and lines were not being placed in the map layer
  
Version 5.2 - 2016.06.03
  - New demo scenes showing viewport mode for both perspective and orthographic camera.
  - New Screen Overlay mode for viewport for making it easier to position the map on the screen. Updated manual.
  - Added "Thin Lines" property to prevent frontiers lines getting thicker when zooming in (useful in VR or mobile)
  - New APIs: GetProvinceIndex (location), GetProvinceNearToPoint (location), GetCountryIndex (location), GetCountryNearTo (location)
  - [Fix] in high resolution mode, some countries had surfaced inverted when colorized
  - [Fix] enclaves (countries surrounded by bigger countries) can now be highlighted

Version 5.1 - 2016.05.24
  - New demo scene for orthographic camera.
  - Added City Hit Test Radius to allow customize selection area for cities (useful when changing map scale)
  - [Fix] issues with orthographic camera for small scales and orthographic sizes

Version 5.0 - 2016.02.24
  - Mount Points. User-defined interest points on map. Integrated editor now supports editing mount points (position, type and collection of attributes)
  - Options to set minimum and maximum zoom distance (API: zoomMinDistance, zoomMaxDistance)
  - Option to toggle province highlighting (API: enableProvinceHighlight in addition to enableCountryHighlight)
  - [Fix] fixed an error when erasing points of a region with less of 5 border points
  - [Fix] some regions failed to highlight due to a regression bug. Fixed.
  
Version 4.4 - 2016.01.16
  - Can hide completely individual countries using decorators, Editor or through API (country.hidden property)
  - Tickers: new overlay mode option for displaying ticker texts
  - Decorators now colorize all regions of a country (texturing still affects only main region)
  - Significant performance improvement in mouse over functionality
  - [Fix] Min population filter now returns to previous value when closing the map editor
  - [Fix] Removed Baikonur Cosmodrome and a degenerated region nearby from Kazakhstan
  - [Fix] Fixed issue in builds when changing default Font in map prefab
  - [Fix] Changing the layer of the prefab no longer stops click events
  - [Fix] Disabling highlighting no longer stop click/enter/exit events
  - [Fix] Disabling highlighting now works with decorators
  - [Fix] Country borders disappear while zooming in and two adjacent regions are colorized
  - [Fix] Some cities were not been highlighted because they were positioned outside a country frontier
  
Version 4.3 2015.12.24
  - Country and region capitals. Different icons + colors, new class filter (cityClassFilter), new "cityClass" property and editor support.
  - Added Hidden GameObjects Tool for dealing with hidden residual gameobjects (located under GameObject main menu)
  - New options for country decorator and for country object (now can click-select/hide/rotate/offset/font size any country label)
  - Added pinch to zoom in/out on mobile devices
  - API: Added new events OnCountryEnter, OnCountryExit, OnCountryClick, OnProvinceEnter, OnProvinceExit, OnProvinceClick, OnCityEnter, OnCityExit, OnCityClick
  - API: Added lastProvinceClicked, lastProvinceRegionClicked, cityHighlightedIndex, lastCityClicked
  - Editor: country's continent is displayed and can be renamed
  - Editor: continent can be destroyed, including countries, provinces and cities
  - Editor: deleting a country now deletes all cities belonging to that country as well
  - Editor: new options to delete a country, a province or all provinces belonging to a country
  - Reorganized/split public main API class to make it easier to find properties / methods
  - [Fix] Fixed wrong acceleration when releasing left mouse button and some drag options were disabled
  - [Fix] Fixed some import issues when mixing both Globe and 2D Editions in the same project 
  
Version 4.2 - 2015.12.01
  - Support for ortographic projection
  - Number of cities increased to 7144
  - Option to draw all provinces (API: drawAllProvinces)
  - Option to invert zoom direction (API: invertZoomDirection)
  - Option to drag without acceleration (API: dragConstantSpeed)
  - Option to allow auto-scroll when mouse is on screen edge (APIs: allowScrollOnScreenEdges & edgeThickness (pixels))
  - Option to highlight all regions of a country (API: highlightAllCountryRegions)
  - Reduced geodata loading & parsing time
  - Consolidated city catalogues in one file
  - Added province name to each city
  - Frontiers and borders now support transparency
  - Country frontiers thickness grow automatically when camera approach (based on LOD) and can choose inner/outer color
  - Country is highlighted now when provinces are shown
  - [Fix] Fixed geodata issues with Republic of Congo, South Sudan and Democratic Republic of the Congo
  - [Fix] Fixed see-through and scene positioning issues
  - [Fix] Fixed city scaling when map default scale is changed
  - [Fix] Fixed minimum font label size so it can accommodate very small texts 
  - [Fix] Changed z-order position of some elements to avoid z-fighting issues
  - [Fix] Fixed ticker texts showing always on top of everything
  - [Fix] Fixed runtime exception when hiding country names during collider's triggers events
  - [Fix] Fixed flicker when using assign a texture to a country and highlight it

Version 4.1 - 2015.11.05
  - New Scenic Earth Style
  - New markers and line drawing and animation support
  - Cursor cross maintains dash pattern irrespective of zoom
  - New APIs: CountryNeighbours, CountryNeighboursOfCurrentRegion, CountryNeighboursOfCurrentRegion
  - New APIs: ProvinceNeighbours, ProvinceNeighboursOfCurrentRegion, ProvinceNeighboursOfCurrentRegion
  - New APIs: SetZoomLevel/GetZoomLevel to programatically set the zoom level from closest (0) to farther (1)
  - New option to enable dragging the map using WASD keys
  - Thicker country outline and now it's shown in province mode as well
  - [Fix] Fixed bouncing issues when fit to window height/width and viewport rendering were enabled
  - [Fix] Interaction is now properly blocked when mouse is hovering an UI element (Canvas, ScrollRect, ...)
   
Version 4.0 - 2015.09.04
  - New component: Map Editor
  - Viewport mode (allows cropping of map)
  - City distance calculator
  - Improved highlighting implementation (>x3 speed)
  - Improved data resolution for high-def countries frontiers file
  - Can change default font for map labels
  - Can assign individual font for labels using Decorator component
  - Can assign individual font for tickers using Tickers component
  - Cities icons now rescale proportionally when zooming in/out (also new scale multiplier in inspector)
  - Additional font included: Corwell
  - Project structure overhaul
  - [Fix] Fixed Inspector dark background on Unity Personal
  
Version 3.0 - Initial launch 2015.08.14


Credits
-------

All code, data files and images, otherwise specified, is (C) Copyright 2015-2017 KronnectNon high-res Earth textures derived from NASA source (Visible Earth)
Flag images: Licensed under Public Domain via Wikipedia



