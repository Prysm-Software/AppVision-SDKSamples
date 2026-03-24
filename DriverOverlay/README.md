# DriverOverlay â€” GIS Overlay Driver

## Overview

A WPF driver that creates and animates **geo-referenced overlays** on AppVision
geographical synoptics (maps).

Every 500 ms the driver moves a target icon along a **Lissajous figure** centred
on a fixed GPS coordinate and draws a 20-dot trail of its previous positions.

Use this as a starting point for any driver that needs to push **live position
data** (vehicles, people, assets) onto a map in the AppVision client.

**References:** `appCommon.dll` (`Helper`), `appSDK.dll` (`AppServerForDriver`, `ClientManager`)  
**Output:** `DriverOverlay.exe` + `target.xaml` + `map.xaml` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | Connection, timer, overlay push |
| `target.xaml` | WPF symbol displayed as the moving target |
| `map.xaml` | WPF symbol used for trail dots |

---

## Key Concepts

### 1. Overlay lifecycle

```csharp
// Create or move
ClientManager.UpdateOverlay(id, symbolPath, synopticTarget, location);

// Delete by exact id
ClientManager.RemoveOverlay("item_0");

// Delete by wildcard
ClientManager.RemoveOverlay("tail_*");
```

### 2. `@geo` synoptic target

Passing `"@geo"` displays the overlay on **all** geographical synoptics
currently open in any connected client, regardless of synoptic name.  
Use a specific path (e.g. `"DriverOverlay\map.xaml"`) to restrict to one synoptic.

### 3. Coordinate helpers (`Prysm.AppVision.Common.Helper`)

WGS-84 coordinates are expressed in decimal degrees.  
`Helper` converts metric offsets to degree deltas:

```csharp
double dLon = Helper.GetOffsetLongitude(xMeters, latitudeDegrees);
double dLat = Helper.GetOffsetLatitude(yMeters);

// Location is System.Windows.Point: X = longitude, Y = latitude
var loc = new Point(baseLon + dLon, baseLat + dLat);
```

### 4. Overlay id naming

| Pattern | Purpose |
|---|---|
| `"item_0"` | Single target â€” same id every tick, just moves |
| `"tail_" + t` | Trail dot â€” predictable id allows selective deletion |

### 5. Timer-driven update loop

A `DispatcherTimer` fires every 500 ms on the WPF UI thread â€” overlay
updates are serialised with no locking required.

---

## AppVision SDK Reference

### `AppServerForDriver` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(host)` | Connect to AppVision |
| `.Login(protocolName)` | Register as driver |
| `.StartNotifications(false)` | Start event loop |
| `.ProtocolSynchronized()` | Signal readiness |
| `.ControllerManager.Closed` | Event: server requests shutdown |

### `ClientManager` (via `AppServerForDriver.ClientManager`)

| Member | Description |
|---|---|
| `.UpdateOverlay(id, symbol, target, loc)` | Upsert an overlay |
| `.RemoveOverlay(id)` | Delete by exact id or wildcard mask |

### `Helper` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.GetOffsetLongitude(xMeters, latDeg)` | East/West metres â†’ longitude delta |
| `.GetOffsetLatitude(yMeters)` | North/South metres â†’ latitude delta |

---

## Configuration in AppVision

```
Configurator â€º Protocols â€º Add
    Name:    DriverOverlay
    Manager: DriverOverlay.exe
```

> No variables are needed â€” the protocol node is for lifecycle only.  
> A geographical synoptic must be open in a client to see the overlays.

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `DriverOverlay.exe`, `target.xaml`, `map.xaml` â†’ `AppVision 4\Bin\`.

---

## Debugging

**Command line arguments:** `DriverOverlay[@localhost]`  
Open a geographical synoptic in an AppVision client to see the moving target.

---

## See Also

- [DriverSimple](../DriverSimple/README.md) â€” variable I/O without overlays
- [OverlaySample](../OverlaySample/README.md) â€” standalone overlay CRUD tool (client mode)
