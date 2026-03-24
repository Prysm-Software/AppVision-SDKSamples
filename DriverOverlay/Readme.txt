DriverOverlay - Overlay Driver Sample
======================================

OVERVIEW
--------
A WPF driver that demonstrates how to create and animate geo-referenced overlays
on AppVision geographical synoptics (maps).

Every 500 ms the driver moves a target icon along a Lissajous figure centred on a
fixed GPS coordinate, and draws a 20-dot trail of its previous positions.

Use this as a starting point for any driver that needs to push live position data
(vehicles, people, assets) onto a map displayed in the AppVision client.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - Helper coordinate utilities
  appSDK.dll     (Prysm.AppVision.SDK)     - AppServerForDriver, ClientManager

The output 'DriverOverlay.exe' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs   All driver logic: connection, timer, overlay push.
  target.xaml          XAML symbol displayed on the map as the moving target.
  map.xaml             XAML symbol used for the trail dots.


KEY CONCEPTS
------------
1. Overlay lifecycle
   An overlay is a named, positioned WPF symbol drawn over a synoptic.
   Create or move:   ClientManager.UpdateOverlay(id, symbolPath, synopticPath, location)
   Delete by id:     ClientManager.RemoveOverlay(id)
   Delete by mask:   ClientManager.RemoveOverlay("tail_*")   (* wildcard supported)

2. "@geo" synoptic target
   Passing "@geo" as the synoptic path displays the overlay on ALL geographical
   synoptics currently open in connected clients, regardless of synoptic name.
   Use a specific synoptic path (e.g. "DriverOverlay\map.xaml") to restrict
   the overlay to a single synoptic.

3. Coordinate helpers  (Prysm.AppVision.Common.Helper)
   WGS-84 GPS coordinates are expressed in decimal degrees (longitude, latitude).
   Helper converts metric offsets to degree offsets:
       Helper.GetOffsetLongitude(xMeters, latitudeDegrees)  ->  delta longitude
       Helper.GetOffsetLatitude(yMeters)                    ->  delta latitude
   The overlay location is System.Windows.Point where X = longitude, Y = latitude.

4. Overlay id naming convention
   Give each overlay a stable, unique id so UpdateOverlay acts as an upsert:
       "item_id"          single target â€” same id every tick, just moves.
       "tail_" + t        trail dots â€” predictable id allows timed deletion.

5. Timer-driven update loop
   A DispatcherTimer fires every 500 ms on the WPF UI thread, keeping overlay
   updates serialised without requiring locks.


APPVISION SDK REFERENCE
-----------------------
AppServerForDriver  (appSDK.dll)
  .Open(host)                                Connect to AppVision.
  .Login(protocolName)                       Register as driver.
  .StartNotifications(false)                 Start event loop.
  .ProtocolSynchronized()                    Signal readiness to AppVision.
  .ControllerManager.Closed                  Event: server requests shutdown.

ClientManager  (via AppServerForDriver.ClientManager)
  .UpdateOverlay(id, symbolPath, target, location)   Upsert an overlay.
  .RemoveOverlay(id)                                 Delete by exact id.
  .RemoveOverlay(mask)                               Delete by wildcard mask.

Helper  (appCommon.dll)
  .GetOffsetLongitude(xMeters, latDeg)   East/West meters to longitude delta.
  .GetOffsetLatitude(yMeters)            North/South meters to latitude delta.


CONFIGURATION IN APPVISION
---------------------------
1. Protocol:
       Configurator > Protocols > Add
           Name:    DriverOverlay
           Manager: DriverOverlay.exe

2. Variables: none required. Protocol node is for lifecycle only.

3. Geographical synoptic:
       A synoptic with a GIS map background must exist and be open in a client.
       The "@geo" target draws overlays on any such synoptic automatically.


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy DriverOverlay.exe, target.xaml, map.xaml to:  AppVision 4\Bin\


DEBUGGING
---------
- Project Properties > Debug > Command line arguments: DriverOverlay[@localhost]
- The driver window displays a live trace log with each overlay update.
- Open a geographical synoptic in an AppVision client to see the moving target.


SEE ALSO
--------
DriverSimple    - Variable I/O without overlay.
OverlaySample   - Standalone tool to manage overlays in client (AppServer) mode.
