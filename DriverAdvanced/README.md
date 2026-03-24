# DriverAdvanced â€” Reference AppVision Driver

## Overview

This project is a **reference implementation** for developing a custom AppVision driver.

A driver (also called a *connector*) is an executable that bridges AppVision with an
external system â€” a security controller (PSIM), a field device, or a standard
protocol (MQTT, OPC-UA, BACnet, SIA, Modbus, etc.).

The driver runs as a separate process and communicates with the AppVision server
through `appSDK.dll`. It is started, stopped, and restarted by AppVision through
the protocol configuration.

**References:**

| Assembly | Namespace | Purpose |
|---|---|---|
| `appCommon.dll` | `Prysm.AppVision.Common` | `Helper`, `CommStatus`, `RT` |
| `appData.dll` | `Prysm.AppVision.Data` | `VariableRow`, `VariableState`, `ProtocolRow` |
| `appSDK.dll` | `Prysm.AppVision.SDK` | `AppServerForDriver`, managers |
| `App.Logger.dll` | `Prysm.Logger` | `Trace`, `LogLevel`, `LogBox` |
| `App.Utility.dll` | `Prysm.Utility.Logger` | `AppServerLogAppender` |

**Output:** `DriverAdvanced.exe` â†’ `AppVision 4\Bin\`

---

## Architecture

```
AppVision Server
  â”‚
  â”œâ”€â”€ ProtocolManager  â”€â”€â–º  launches and monitors  DriverAdvanced.exe
  â”œâ”€â”€ VariableManager  â—„â”€â”€â–º Driver  (variable values and states)
  â”œâ”€â”€ AlarmManager     â—„â”€â”€  Driver  (raises alarms on errors)
  â””â”€â”€ ControllerManager â”€â”€â–º Driver  (restart / close events)
```

---

## Project Structure

| File | Role |
|---|---|
| `App.xaml.cs` | WPF entry point: lifecycle, error handling, graceful shutdown |
| `MainWindow.xaml.cs` | Diagnostic UI: live log with configurable level and max count |
| `Driver.cs` | Core driver: AppVision connection, device discovery, event routing |
| `Device.cs` | Per-device logic: connect one external device, handle commands |
| `BuildInfo.cs` | Auto-generated build date for the SDK warranty date check |

---

## Key Concepts

### 1. Protocol

AppVision launches the driver and passes the protocol name as the first argument:

```
DriverAdvanced.exe MyProtocolName[@hostname]
```

### 2. Linked variables ("owned variables")

Variables whose `Source` is `$P.<ProtocolName>` are owned by this driver.  
The driver discovers them at startup:

```csharp
var variables = server.GetVariablesByProtocol(protocolName);
// Each root variable (e.g. "Device1") represents one external device.
```

### 3. Comm variable (communication status)

Each device should expose a child variable `<Device>.Comm` of type Logical
with the **Command** flag checked.

```csharp
// Values from CommStatus enum:
SetVariable("Device1.Comm", (int)CommStatus.Connected);    // 1
SetVariable("Device1.Comm", (int)CommStatus.Fault);        // 3
```

| `CommStatus` | Value | Meaning |
|---|---|---|
| `Unknown` | 0 | Initial, not yet determined |
| `Connected` | 1 | Device is reachable |
| `Disconnected` | 2 | Intentionally offline |
| `Fault` | 3 | Communication error |

### 4. Variable state notifications

```csharp
// Subscribe only to Command variables under a specific device
server.AddFilterNotifications("$V.Device1.*");
server.AddFilterNotifications("Variable.StateType=1"); // Command flag only
```

### 5. Value caching

`SetVariable()` suppresses redundant updates â€” no message is sent when the
new value equals the cached value.  
Use `SetVariableNoCache()` to force-send regardless.

### 6. Alarm management

```csharp
_driver.AddAlarm(
    code:    "COMM_ERROR",
    message: "Connection lost",
    detail:  exception.ToString()
);
```

Each alarm carries: code, short message, variable description, exception detail,
area path, and group descriptions.

### 7. Download / Refresh

| Event | When raised | Driver action |
|---|---|---|
| `Download` | Operator pushes config to device | Upload timetables, profiles, badges |
| `Refresh` | Operator requests full resync | Re-read all device states |

---

## AppVision SDK Reference

### `AppServerForDriver` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(host)` | Connect â€” empty = local machine |
| `.Login(protocolName)` | Register as the protocol handler |
| `.GetVariablesByProtocol()` | Return all variables linked to this protocol |
| `.AddFilterNotifications(f)` | Subscribe to specific variable change events |
| `.StartNotifications(false)` | Start receiving callbacks |
| `.ProtocolSynchronized()` | Signal driver is ready |
| `.CurrentProtocol` | Protocol config row (address, parameters, â€¦) |
| `.CheckWarrantyDate(date)` | Verify build date is within SDK warranty |
| `.Restart` | Event: AppVision requests restart |
| `.Download` | Event: AppVision sends a download command |
| `.Refresh` | Event: AppVision sends a refresh command |
| `.IsConnected` | True while connection is open |
| `.Close()` | Clean disconnect |

### `VariableManager`

| Member | Description |
|---|---|
| `.StateChanged` | Event: variable value changed (command from AppVision) |
| `.RowUpdated` | Event: variable configuration row changed |
| `.Set(name, value, info, op)` | Push a new value to AppVision |

### `AlarmManager`

| Member | Description |
|---|---|
| `.AddAlarmExt(code, msg, â€¦)` | Raise a structured alarm |

### `Helper` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.ParseUri(address)` | Parse a URI from a variable's Address field |
| `.ParseEndPoint(address, port)` | Parse a TCP endpoint (`ip:port`) |
| `.GetParameter(name, params)` | Extract a named parameter from a parameter string |
| `.GetParameter<T>(name, params)` | Typed variant |

### Logging (`App.Logger.dll`)

| Type | Description |
|---|---|
| `Trace` | Static entry point â€” dispatches to all registered `ILogAppender` |
| `Trace.Info(msg)` | Write an informational message |
| `Trace.Error(msg/ex)` | Write an error or exception |
| `Trace.StatusText` | Update the status bar in MainWindow |
| `LogLevel` | `All \| Debug \| Info \| Warning \| Error \| Fatal \| None` |
| `LogBox` | WPF control that renders log entries in a scrollable, colour-coded list |

Register an appender:

```csharp
Trace.Logger.AddAppender(appender, LogLevel.Info, "tag");
```

### `AppServerLogAppender` (`App.Utility.dll`)

Forwards `Trace` entries to the **AppVision server log** â€” visible in the
AppVision Manager diagnostic console.

```csharp
Trace.Logger.AddAppender(
    new AppServerLogAppender(AppServer),
    LogLevel.All,
    "app"
);
```

---

## Configuration in AppVision

```
Configurator â€º Protocols â€º Add
    Name:    DriverAdvanced
    Manager: DriverAdvanced.exe

Configurator â€º Variables â€º Add
    Device1         Source: $P.DriverAdvanced
    Device1.Comm    Type: Logical   Flag Command âœ“

    Device2         Source: $P.DriverAdvanced
    Device2.Comm    Type: Logical   Flag Command âœ“
```

Additional sub-variables (e.g. `Device1.Temperature`) can be added under each
device as needed.

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `DriverAdvanced.exe` â†’ `AppVision 4\Bin\`  
   *(or a subfolder â€” the assembly resolver in `App.xaml.cs` searches parent folders)*.  
3. Start the protocol from AppVision Configurator or AppVision Manager.

---

## Debugging

**Command line arguments:** `DriverAdvanced[@localhost]`

- The MainWindow shows a live log. Right-click to change log level or max line count.  
- **Ctrl + Close** shuts down the driver. Closing without Ctrl is blocked intentionally
  to prevent accidental shutdown in production.  
- Log output is also visible in the AppVision Manager console via `AppServerLogAppender`.

---

## How to Adapt This Sample

1. Rename the project, assembly, and namespace.  
2. Update `BuildInfo.cs` with the correct build date.  
3. **`Driver.cs`** â€” parse protocol-level settings from `CurrentProtocol.Address` / `.Parameters`:

```csharp
Helper.ParseEndPoint(CurrentProtocol.Address, defaultPort);
Helper.GetParameter("timeout", CurrentProtocol.Parameters);
```

4. **`Device.cs`** â€” implement the device lifecycle:

| Override | Action |
|---|---|
| `Start()` | Connect to the device, begin monitoring |
| `Stop()` | Disconnect, release resources |
| `VariableChanged(state)` | Forward AppVision commands to the device |
| `variableUpdated(row)` | React to config-row changes |

5. Call `_driver.SetVariable()` to push device data into AppVision.  
6. Call `_driver.AddAlarm()` to report errors.  
7. Adjust the notification filter to cover only the variables you need.  
8. Implement or remove `Download()` / `Refresh()` as needed.

---

## Dependencies

All assemblies ship with AppVision and are marked `<Private>False</Private>` in the
`.csproj` â€” they are **not** copied to the driver output folder.  
The driver resolves them at runtime from `AppVision 4\Bin\` via `LookForAssemblyInParentDir`.

| Assembly | Namespace | Key types |
|---|---|---|
| `appCommon.dll` | `Prysm.AppVision.Common` | `Helper`, `CommStatus`, `RT`, `HidePasswords` |
| `appData.dll` | `Prysm.AppVision.Data` | `VariableRow`, `VariableState`, `ProtocolRow` |
| `appSDK.dll` | `Prysm.AppVision.SDK` | `AppServerForDriver`, `VariableManager`, `AlarmManager`, `ControllerManager` |
| `App.Logger.dll` | `Prysm.Logger` | `Trace`, `LogLevel`, `ILogAppender`, `LogBox` |
| `App.Utility.dll` | `Prysm.Utility.Logger` | `AppServerLogAppender` |

---

## Learning Path

```
DriverMinimal  â†’  DriverSimple  â†’  DriverAdvanced
No GUI             WPF + 8 I/O      Full reference implementation
```
