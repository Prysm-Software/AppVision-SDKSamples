# ClientSimple — Simple AppVision Client

## Overview

A minimal WPF application that connects to AppVision as a **client** and
demonstrates the three real-time notification streams:

| Stream | Event | Description |
|---|---|---|
| Variables | `VariableManager.StateChanged` | Value change |
| Alarms | `AlarmManager.AlarmChanged` | Alarm status change |
| Events | `EventManager.EventChanged` | Custom event raised |

The sample also shows how to read all current variable values and set a
variable value interactively.

**References:** `appData.dll`, `appSDK.dll`  
**Output:** `ClientSimple.exe` — standalone WPF application

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | Connection, subscriptions, read/set variable |

---

## Key Concepts

### 1. `AppServer` vs `AppServerForDriver`

| Class | Used by |
|---|---|
| `AppServer` | Client applications (operators, dashboards, tools) |
| `AppServerForDriver` | Protocol drivers |

`AppServer` adds `LoadDatabase()`, full `UserManager`, `AlarmManager`, etc.

### 2. Connection sequence

```csharp
server.Open("http://localhost");             // connect
server.Login("ADM", "123");                  // authenticate
await server.LoadDatabase(true);             // download config into local cache
server.AddFilterNotifications("All");        // choose notification scope
server.StartNotifications(false, false);     // begin receiving callbacks
```

### 3. Notification filters

| Filter | Receives |
|---|---|
| `"All"` | Everything |
| `"$V.CR1.*"` | Variables under node CR1 only |
| `"$A.*"` | All alarms |
| `"NotificationType=VariableState"` | Variable changes only |

### 4. `GetStates()`

Snapshot of all current variable values — useful to populate a grid without
waiting for a change event:

```csharp
var states = server.VariableManager.GetStates();
```

### 5. Setting a variable

```csharp
server.VariableManager.Set("NodeName.VarName", newValue);
```

AppVision distributes the new value to all subscribers (including drivers)
according to the variable's command flag.

---

## AppVision SDK Reference

### `AppServer` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(hostUrl)` | Connect |
| `.Login(user, pwd)` | Authenticate |
| `.LoadDatabase(forceReload)` | Download config cache |
| `.AddFilterNotifications(f)` | Select notification scope |
| `.StartNotifications(initial, queue)` | Start callbacks |
| `.Close()` | Disconnect |
| `.IsConnected` | Connection state |
| `.VariableManager.StateChanged` | Event: value changed |
| `.VariableManager.GetStates()` | Snapshot of all values |
| `.VariableManager.Set(name, v)` | Push a new value |
| `.AlarmManager.AlarmChanged` | Event: alarm status changed |
| `.EventManager.EventChanged` | Event: custom event raised |

---

## How to Build and Run

1. Build in **Release** configuration.  
2. Run `ClientSimple.exe`.  
3. Enter the server address (e.g. `http://localhost`), user and password, then **Connect**.

> Default credentials: user `ADM`, password `123`.

---

## Debugging

- Set breakpoints in `StateChanged` / `AlarmChanged` / `EventChanged` callbacks.
- All received events are appended to the ListBox in the main window.

---

## Learning Path

```
ClientSimple  →  ClientAdvanced
Standalone        Sdk.Server singleton, RColor theme, embedded WPF controls
```
