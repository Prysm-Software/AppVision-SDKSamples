# DriverAppControl â€” AppControl Access Controller Driver

## Overview

An AppVision driver that bridges AppVision with an **AppControl** access control
system. It connects to AppControl hardware via the AppControl SDK, mirrors
reader states and access events into AppVision variables, and forwards
AppVision commands (door unlock, etc.) to the physical controllers.

Use this as a reference for drivers that communicate with **access controllers,
intrusion panels, or other security devices** through a device-specific SDK.

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`, `appControl.dll`  
**Output:** `DriverAppControl.exe` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `App.xaml.cs` | WPF entry point, lifecycle management |
| `MainWindow.xaml.cs` | Diagnostic window (log display) |
| `Driver.cs` | Protocol level: AppVision connection, device creation, main loop |
| `Device.cs` | Device level: AppControl lifecycle, event mirroring |
| `ChannelService.cs` | Low-level TCP/IP channel to one AppControl controller |

---

## Key Concepts

### 1. AppControl SDK (`appControl.dll`)

`ChannelService` manages the connection to one physical access controller.

| Event | Description |
|---|---|
| `.ConnectChanged` | Controller connected / disconnected |
| `.InputChanged` | Reader or digital input state changed |
| `.AccessControlEvent` | Badge swipe â€” grant / deny / alarm |

### 2. Download command

`AppServerForDriver.Download` fires when AppVision requests a configuration
push. The driver uploads timetables, access profiles, and badge lists to the
controller via the AppControl SDK.

### 3. NewModification event

`AppServerForDriver.NewModification` fires when the AppVision configuration
changes (e.g. a badge was added or a timetable was updated).  
The driver queues a download at the next polling cycle.

### 4. Command variable naming convention

Command variables follow the pattern `DeviceName.Reader01.Cmd`.  
Use a targeted filter to subscribe only to command variables:

```csharp
server.AddFilterNotifications("$V." + deviceName + ".*.Cmd");
```

### 5. Thread-based main loop

`Driver.Main()` runs on a dedicated background thread, polling device status
and processing the download queue â€” the WPF dispatcher is never blocked.

### 6. Per-device physical limits (`Device.cs`)

| Constant | Value | Description |
|---|---|---|
| `MAX_READER` | 4 | Readers per controller |
| `MAX_HOLIDAY` | 24 | Holiday slots |
| `MAX_TIMETABLE` | 48 | Timetable slots |
| `MAX_USER` | 10 000 | Cardholder capacity |
| `MAX_PROFILE` | 256 | Access profile slots |

---

## AppVision SDK Reference

### `AppServerForDriver` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Download` | Event: AppVision requests a configuration push |
| `.NewModification` | Event: AppVision configuration changed |
| `.VariableManager.Set(name, v)` | Mirror device state into AppVision |
| `.VariableManager.StateChanged` | Event: command received from AppVision |

---

## Configuration in AppVision

```
Configurator â€º Protocols â€º Add
    Name:    DriverAppControl
    Manager: DriverAppControl.exe

Variables (one set per controller):
    Device1             Source: $P.DriverAppControl
                        Address: <controller IP>
    Device1.Reader01    Type: Logical
    Device1.Reader02    Type: Logical
    Device1.Reader01.Cmd  Type: Logical   Flag Command âœ“
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `DriverAppControl.exe` â†’ `AppVision 4\Bin\`.

---

## Debugging

**Command line arguments:** `DriverAppControl[@localhost]`  
The diagnostic window shows traces from both the Driver and Device layers.

---

## See Also

- [DriverAdvanced](../DriverAdvanced/README.md) â€” driver template with the Device pattern
- [WebServiceODataAppControl](../WebServiceODataAppControl/README.md) â€” OData access to AppControl people/badges
- [AppControlSynchro](../AppControlSynchro/README.md) â€” CSV import of people into AppControl
