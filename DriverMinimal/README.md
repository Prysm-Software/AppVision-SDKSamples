# DriverMinimal â€” Minimal AppVision Driver

## Overview

The simplest possible AppVision driver: a **console application with no GUI**.

Use this sample to understand the basic SDK connection sequence before
moving on to [DriverSimple](../DriverSimple/README.md) or
[DriverAdvanced](../DriverAdvanced/README.md).

The driver logs every variable state change to the console.  
It does not manage device objects, alarms, or value caching.

**References:** `appSDK.dll` (`Prysm.AppVision.SDK`)  
**Output:** `DriverMinimal.exe` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `Program.cs` | Single-file driver: connection, notification loop, shutdown |

---

## Key Concepts

### 1. Minimal connection sequence

```csharp
var server = new AppServerForDriver();
server.Open(host);                               // connect (empty = localhost)
server.Login(protocolName);                      // register as driver
server.AddFilterNotifications("$V.*");           // subscribe to all variables
server.StartNotifications(false);                // begin receiving callbacks
server.ProtocolSynchronized();                   // signal readiness to AppVision
```

### 2. VariableState

`VariableManager.StateChanged` delivers a `VariableState` on every change:

| Field | Description |
|---|---|
| `.Name` | Full variable name |
| `.Value` | New value (`object`) |
| `.Date` | Timestamp of the change |

### 3. Lifecycle

`ControllerManager.Closed` fires when AppVision shuts down or removes the
protocol. The driver sets `_closing = true` and exits the polling loop.

### 4. Pushing values back to AppVision

```csharp
_server.VariableManager.Set("NodeName.VarName", value);
```

---

## AppVision SDK Reference

### `AppServerForDriver` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(host)` | Connect â€” empty string = localhost |
| `.Login(protocolName)` | Register as the protocol handler |
| `.AddFilterNotifications(filter)` | Subscribe to variable events |
| `.StartNotifications(false)` | Start receiving state-change callbacks |
| `.ProtocolSynchronized()` | Notify AppVision the driver is ready |
| `.VariableManager.StateChanged` | Event: variable value changed |
| `.VariableManager.Set(name, v)` | Push a value to AppVision |
| `.ControllerManager.Closed` | Event: AppVision requests shutdown |

---

## Configuration in AppVision

```
Configurator â€º Protocols â€º Add
    Name:    DriverMinimal
    Manager: DriverMinimal.exe
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `DriverMinimal.exe` â†’ `AppVision 4\Bin\`.  
3. Start the protocol from AppVision â€” the driver is launched automatically.

---

## Debugging

Set **Project Properties â€º Debug â€º Command line arguments** to:

```
DriverMinimal[@localhost]
```

Every received variable state change is printed to the console.

---

## Learning Path

```
DriverMinimal  â†’  DriverSimple  â†’  DriverAdvanced
No GUI             WPF + 8 I/O      Device pattern, alarms, caching
```
