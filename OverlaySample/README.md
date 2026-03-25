# OverlaySample — Overlay Manager Tool

## Overview

A standalone WPF tool that connects to AppVision as a client and demonstrates
the full **overlay CRUD API**:

| Operation | Method |
|---|---|
| List all overlays | `ClientManager.GetOverlays()` |
| Real-time updates | `ClientManager.OverlayUpdated` event |
| Create / move | `ClientManager.UpdateOverlay()` |
| Delete | `ClientManager.RemoveOverlay()` |

Overlays are geo-referenced or synoptic-positioned WPF symbols pushed to
AppVision clients in real time. Common use cases: moving vehicles, personnel
tracking, asset positioning on a GIS map.

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`  
**Output:** `OverlayManager.exe` → `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | Connection, overlay list, CRUD operations |

---

## Key Concepts

### 1. Overlay CRUD

```csharp
// List
var overlays = await server.ClientManager.GetOverlays();

// Create or move (upsert by id)
server.ClientManager.UpdateOverlay(id, symbolPath, "@geo", location);

// Delete by exact id
server.ClientManager.RemoveOverlay("item_0");

// Delete by wildcard
server.ClientManager.RemoveOverlay("tail_*");
```

### 2. Subscribe before listing (avoid race conditions)

```csharp
server.ClientManager.OverlayUpdated += OnOverlayUpdated;
server.AddFilterNotifications("NotificationType=Overlay");
server.StartNotifications(false);

// Now safe to list — any changes during the list will be caught by the event
var overlays = await server.ClientManager.GetOverlays();
```

### 3. `ModifType` enum

The `OverlayUpdated` event carries a `ModifType`:

| Value | Meaning |
|---|---|
| `Add` | New overlay created |
| `Modify` | Existing overlay moved or updated |
| `Delete` | Overlay removed |

Use it to keep a local `ObservableCollection` in sync with the server.

### 4. Symbol paths

Use `FileInfoManager.GetRows()` filtered by `FileInfoType.Symbol` to populate
a symbol picker. Symbol paths passed to `UpdateOverlay` must be accessible by
connected clients.

### 5. `WaitCursor` helper

```csharp
using (new Helper.WaitCursor())
{
    // cursor is automatically reset at end of block
}
```

### 6. Intercept DataGrid Delete key

Delete via the server (not the local collection) so all clients stay in sync:

```csharp
grid.CommandBindings.Add(new CommandBinding(
    ApplicationCommands.Delete,
    (s, e) => server.ClientManager.RemoveOverlay(selectedId)));
```

---

## AppVision SDK Reference

### `AppServer` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(hostUrl)` | Connect |
| `.Login(user, password)` | Authenticate |
| `.AddFilterNotifications(f)` | Select scope |
| `.StartNotifications(initial, queue)` | Begin callbacks |
| `.ControllerManager.ConnectionLost` | Event: connection dropped |

### `ClientManager` (via `AppServer.ClientManager`)

| Member | Description |
|---|---|
| `.GetOverlays()` | List all overlays (async) |
| `.UpdateOverlay(id, symbol, target, loc)` | Upsert overlay |
| `.RemoveOverlay(id)` | Delete by id or wildcard |
| `.OverlayUpdated` | Event: overlay changed |

### `OverlayItem` (`appData.dll`)

| Field | Description |
|---|---|
| `.Id` | Unique identifier |
| `.Symbol` | Path to WPF symbol file |
| `.Target` | Synoptic path or `"@geo"` |
| `.Location` | `System.Windows.Point` — X=longitude, Y=latitude |

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `OverlayManager.exe` → `AppVision 4\Bin\`.

---

## Debugging

Connection string format: `http://user:password@hostname`  
(parsed with `Helper.ParseUri`)

---

## See Also

- [DriverOverlay](../DriverOverlay/README.md) — push overlays from a driver
- [MapProvider](../MapProvider/README.md) — custom GIS map background
