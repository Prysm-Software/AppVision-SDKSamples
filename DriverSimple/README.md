# DriverSimple â€” Simple AppVision Driver

## Overview

A WPF driver that simulates a physical I/O device with **8 digital inputs,
8 command outputs, and one buffer variable**.

| Direction | Mechanism | Description |
|---|---|---|
| **Input** â†’ AppVision | `VariableManager.Set()` | CheckBox click simulates an external event |
| AppVision â†’ **Output** | `VariableManager.StateChanged` | RadioButton shows the last received command |
| **Buffer** | `VariableManager.Set()` | Push a sequence of integers to AppVision |
| **Event** | `EventManager.AddEvent()` | Raise a custom event in AppVision |
| **Alarm** | `AlarmManager.AddAlarmExt()` | Raise a custom alarm in AppVision |

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`  
**Output:** `DriverSimple.exe` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | All driver logic: connection, UI events, AppVision interaction |

---

## Key Concepts

### 1. `@init` operation flag

Reset input variables at startup **without** recording a database-change event:

```csharp
_server.VariableManager.Set("DrvSampleVar.Input1", 0, operation: "@init");
```

This prevents spurious alarms or rule triggers on driver startup.

### 2. Restart support

`ControllerManager.Restart` fires when AppVision requests a driver restart
(e.g. after a configuration reload):

```csharp
Process.Start(Environment.GetCommandLineArgs()[0],
              string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
Application.Current.Shutdown();
```

### 3. `GetVariablesByProtocol`

Returns all variables whose Source is set to this protocol:

```csharp
var node = _server.GetVariablesByProtocol(protocolName).FirstOrDefault();
// node.Name == "DrvSampleVar"
```

### 4. Buffer variable

A `Buffer`-type variable stores a space-separated list of integers:

```csharp
_server.VariableManager.Set("DrvSampleVar.Buffer", "12 35 78 456 -85 0");
```

### 5. Error reporting

```csharp
_server.AlarmManager.AddAlarmExt(code, "Short message", "Full description");
```

---

## AppVision SDK Reference

### `AppServerForDriver` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.GetVariablesByProtocol(name)` | List variables linked to this protocol |
| `.Log(text)` | Write to the AppVision server log |
| `.VariableManager.Set(name, v)` | Push a new value to AppVision |
| `.VariableManager.Set(name, v, operation:"@init")` | Push initial value (no DB event) |
| `.VariableManager.StateChanged` | Event: command received from AppVision |
| `.EventManager.AddEvent(type, desc)` | Raise a custom event |
| `.AlarmManager.AddAlarmExt(code, msg, desc)` | Raise a custom alarm |

---

## Configuration in AppVision

```
Configurator â€º Protocols â€º Add
    Name:    DrvSample
    Manager: DriverSimple.exe

Variables:
    DrvSampleVar            Source: $P.DrvSample
    DrvSampleVar.Input1     Type: Logical
    ...
    DrvSampleVar.Input8     Type: Logical
    DrvSampleVar.Output1    Type: Logical   Flag Command âœ“   Source: =$parent.Input1
    ...
    DrvSampleVar.Output8    Type: Logical   Flag Command âœ“
    DrvSampleVar.Buffer     Type: Buffer    Buffer type: Int32
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `DriverSimple.exe` â†’ `AppVision 4\Bin\`.

---

## Debugging

**Command line arguments:** `DrvSample[@localhost]`  
The trace text area in the window displays a live log.  
Errors are forwarded to the AppVision server log.

---

## Learning Path

```
DriverMinimal  â†’  DriverSimple  â†’  DriverAdvanced
No GUI             WPF + 8 I/O      Device pattern, alarms, caching
```
