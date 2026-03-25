# WpfControlAdvanced — Advanced Embeddable WPF Control

## Overview

A fully-featured WPF `UserControl` that demonstrates the complete set of
AppVision integration interfaces available to embedded controls:

| Interface | Purpose |
|---|---|
| `IPersistable` | Save / restore control state across sessions |
| `ICommandable` | Receive commands from AppVision scripting rules |
| `IContainer` | Discover the parent synoptic panel by name |

The control also demonstrates:
- Async data loading (`FileInfoManager`, `AreaManager`, `VariableManager`)
- Real-time subscriptions (`StateChanged`, `AlarmChanged`, `EventChanged`)
- Constructor parameters (configuration tokens passed at load time)
- Interactive synoptic navigation

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`, `appUI.dll`  
**Output:** `WpfControlAdvanced.dll` → `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `UserControl1.xaml.cs` | All logic: data loading, subscriptions, interfaces |

---

## Key Concepts

### 1. `IPersistable`

AppVision calls `GetConfig()` / `SetConfig()` when saving and restoring the
synoptic layout. Serialise all user-adjustable settings to a compact string:

```csharp
public string GetConfig() => "mode=dark;filter=alarms";

public void SetConfig(string s)
{
    var mode   = Helper.GetParameter("mode",   s);
    var filter = Helper.GetParameter("filter", s);
}
```

### 2. `ICommandable`

AppVision scripts can target this control with named commands:

```csharp
public void Execute(string command, string[] args)
{
    switch (command)
    {
        case "Highlight": Highlight(args[0]); break;
        case "Reset":     Reset();            break;
    }
}
```

### 3. `IContainer` and `GetAncestor<T>()`

Walk the WPF visual tree upward to find the hosting synoptic panel:

```csharp
var container = this.GetAncestor<IContainer>();
string panelName = container?.Name;
```

### 4. Parameterised constructor

Embedding code can pass configuration tokens at load time:

```csharp
Sdk.Client.Load(0, "main",
    "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1",
    "arg1", "arg2");
```

AppVision instantiates the control using the public constructor that accepts
`params string[] args`.

### 5. Async loading in `Loaded` event

```csharp
private async void UserControl_Loaded(object sender, RoutedEventArgs e)
{
    var files = await Task.Run(() => Sdk.Server.FileInfoManager.GetRows());
    var areas = await Task.Run(() => Sdk.Server.AreaManager.GetRows());
}
```

### 6. Subscription cleanup in `Unloaded` event

Always remove handlers in `UserControl_Unloaded` to prevent memory leaks:

```csharp
private void UserControl_Unloaded(object sender, RoutedEventArgs e)
{
    Sdk.Server.VariableManager.StateChanged -= OnStateChanged;
    Sdk.Server.AlarmManager.AlarmChanged    -= OnAlarmChanged;
}
```

### 7. Server information API

```csharp
Sdk.Server.GetOption("App.Main.ProjectDescription")
Sdk.Server.ControllerManager.ServerState.Version
Sdk.Server.Hostname
Sdk.Server.GetLicenseInfo("SN")
Sdk.Server.UserManager.CurrentUser?.FullName
```

---

## AppVision SDK Reference

### `Sdk.Server` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.GetOption(key)` | Read a server-side option |
| `.GetLicenseInfo(key)` | Read a licence field |
| `.ControllerManager.ServerState` | Server version / status |
| `.FileInfoManager.GetRows()` | List synoptics, symbols, files |
| `.AreaManager.GetRows()` | List areas (site tree) |
| `.VariableManager.GetStates()` | All current variable values |
| `.VariableManager.StateChanged` | Event: value changed |
| `.AlarmManager.AlarmChanged` | Event: alarm changed |
| `.EventManager.EventChanged` | Event: custom event raised |

### `Sdk.Client` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Load(filename)` | Open a synoptic by filename |

### `IPersistable`, `ICommandable`, `IContainer` (`appUI.dll`)

| Interface | Methods |
|---|---|
| `IPersistable` | `GetConfig()` → `string`, `SetConfig(s)` |
| `ICommandable` | `Execute(cmd, args)` |
| `IContainer` | `.Name` property |

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `WpfControlAdvanced.dll` → `AppVision 4\Bin\`.

---

## Debugging

Attach to the AppVision client process in Visual Studio.  
The **Log()** rolling list in the control UI shows received events at runtime.

---

## Learning Path

```
WpfControlSimple  →  WpfControlAdvanced  →  PlayerSimple / PlayerAdvanced
Basic read/write      Full interface set        Video controls
```
