OverlaySample - Overlay Manager Sample
=======================================

OVERVIEW
--------
A standalone WPF tool that connects to AppVision as a client and demonstrates
the full overlay CRUD API:

  - List all current overlays (GetOverlays).
  - Receive real-time overlay updates (OverlayUpdated event).
  - Create or move overlays (UpdateOverlay).
  - Delete overlays (RemoveOverlay).

Overlays are geo-referenced or synoptic-positioned WPF symbols pushed to
AppVision clients in real time. A common use case is displaying moving
vehicles, personnel, or asset positions on a GIS map synoptic.

Note: since this sample was created, overlay management has been integrated
directly into the AppVision client (Menu > Administration > Overlays).
This project remains useful as a reference for programmatic overlay control
from external tools or scripts.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - Helper, WaitCursor, AddRange, RemoveAll
  appData.dll    (Prysm.AppVision.Data)    - OverlayItem, FileInfoRow, ModifType
  appSDK.dll     (Prysm.AppVision.SDK)     - AppServer, ClientManager

The output 'OverlayManager.exe' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs   Connection, overlay list, CRUD operations.


KEY CONCEPTS
------------
1. Overlay CRUD
   ClientManager exposes the overlay management API:
       GetOverlays()                                  List all current overlays.
       UpdateOverlay(id, symbolPath, target, loc)      Create or move an overlay.
       RemoveOverlay(id)                               Delete by exact id.
       RemoveOverlay(mask)                             Delete by wildcard (e.g. "tail_*").

2. Real-time updates
   Subscribe before listing to avoid race conditions:
       server.ClientManager.OverlayUpdated += Overlay_Updated;
       server.AddFilterNotifications("NotificationType=Overlay");
       server.StartNotifications(false);
       var overlays = await server.ClientManager.GetOverlays();

3. ModifType enum
   The OverlayUpdated event delivers a ModifType that indicates the operation:
       ModifType.Add     - new overlay created.
       ModifType.Modify  - existing overlay moved or updated.
       ModifType.Delete  - overlay removed.
   Use it to maintain a local ObservableCollection in sync with the server state.

4. FileInfoRow and symbol paths
   FileInfoManager.GetRows() returns all server-side files (synoptics, symbols).
   Filter by FileInfoType.Synoptic or FileInfoType.Symbol to populate pickers.
   Symbol paths passed to UpdateOverlay must match a file accessible by clients.

5. WaitCursor helper
   Helper.WaitCursor (via using new WaitCursor()) sets the cursor to a wait
   cursor for the duration of a using block without manual try/finally.

6. DataGrid delete interception
   The sample intercepts DataGrid.DeleteCommand (Delete key) to call
   RemoveOverlay() instead of deleting the row from the local collection directly.
   Always delete via the server so all connected clients stay synchronised.


APPVISION SDK REFERENCE
-----------------------
AppServer  (appSDK.dll)
  .Open(hostUrl)                           Connect.
  .Login(user, password)                   Authenticate.
  .AddFilterNotifications(filter)          Select notification scope.
  .StartNotifications(initial, queue)      Begin receiving callbacks.
  .ControllerManager.ConnectionLost        Event: connection dropped.

ClientManager  (via AppServer.ClientManager)
  .GetOverlays()                                   List all overlays (async).
  .UpdateOverlay(id, symbolPath, target, loc)        Upsert overlay.
  .RemoveOverlay(id)                                Delete by id or wildcard mask.
  .OverlayUpdated                                   Event: overlay changed.

FileInfoManager  (via AppServer.FileInfoManager)
  .GetRows()                                       List server-side files.

OverlayItem  (appData.dll)
  .Id, .Symbol, .Target, .Location                 Overlay fields.


CONFIGURATION IN APPVISION
---------------------------
No protocol configuration is required. Connect as any user with client rights.
Default credentials: user "ADM", password "123".

Format the connection string as:  http://user:password@hostname
(the sample parses it with Helper.ParseUri)


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy OverlayManager.exe to:  AppVision 4\Bin\


SEE ALSO
--------
DriverOverlay   - Push overlays from a driver (AppServerForDriver).
MapProvider     - Custom GIS map background for geographical synoptics.
