# WizardSample â€” AppVision Configuration Wizard

## Overview

A multi-page WPF wizard that guides an operator through **discovering,
connecting, and importing a new device** into the AppVision configuration.

Use this as a starting template for a driver-specific setup wizard.
Replace every `<todo>` placeholder with device-specific logic.

**References:** `appCommon.dll`, `appData.dll`, `appSDK.dll`  
**Output:** `WizardSample.exe` â€” standalone WPF application

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | Shell: hosts the current page `UserControl` |
| `ViewModel.cs` | Navigation commands and shared state |
| `ViewModel.Import.cs` | Import logic: create protocol, variables, groups |
| `Device.cs` | Model: discovered device data (IP, name, entities) |
| `Entity.cs` | Model: one device entity (camera, reader, sensor) |
| `Pages\1 WelcomePage` | Startup / splash page |
| `Pages\2 AppLoginPage` | Connect to AppVision server |
| `Pages\3 DiscoveryPage` | Discover devices on the network *(optional)* |
| `Pages\4 DeviceLoginPage` | Collect device address and credentials |
| `Pages\5 DeviceDetailsPage` | Scan device; list all entities to import |
| `Pages\6 SettingsPage` | Configure import options (node name, symbol, etc.) |
| `Pages\7 SummaryPage` | Review before import |
| `Pages\8 ImportPage` | Execute import and show progress |
| `Pages\9 CompletedPage` | Show results and offer to open AppVision |

---

## Wizard Flow

```
WelcomePage
    â”‚
AppLoginPage    â† skip if credentials are in the command line (http://user:pwd@host)
    â”‚
DiscoveryPage   â† remove if the device type has no discovery mechanism
    â”‚
DeviceLoginPage â† remove if the device requires no authentication
    â”‚
DeviceDetailsPage
    â”‚
SettingsPage
    â”‚
SummaryPage
    â”‚
ImportPage
    â”‚
CompletedPage
```

---

## Key Concepts

### 1. MVVM navigation

`MainWindow.xaml` binds its content to `ViewModel.CurrentPage`.  
Navigation commands set `CurrentPage` to the next `UserControl` instance:

```csharp
public ICommand DiscoveryCommand => new RelayCommand(GotoDiscoveryPage);

private void GotoDiscoveryPage()
{
    CurrentPage = new DiscoveryPage();
}
```

### 2. Shared state via `ViewModel`

Device details discovered in early pages are stored as `ViewModel` properties
(`DeviceHostname`, `DeviceUsername`, `SelectedEntities`, â€¦) and read by
later pages without explicit parameter passing.

### 3. `AppServer` in wizard context

The wizard uses its **own** `AppServer` instance (not `Sdk.Server`) â€” it is a
standalone tool, not a control hosted by the AppVision client:

```csharp
appServer.Open(hostUrl);
appServer.Login(user, password);
await appServer.LoadDatabase(true);
```

### 4. Creating configuration objects

```csharp
// Protocol
var proto = new ProtocolRow { Name = ProtocolName, Manager = ManagerExe };
appServer.ProtocolManager.Add(proto);

// Device node variable
var varDevice = new VariableRow
{
    Name       = NodeName,
    Type       = VariableType.Node,
    Expression = "$P." + ProtocolName,
    Address    = $"{DeviceUsername}:{DevicePassword}@{DeviceHostname}"
};
appServer.VariableManager.Add(varDevice);
appServer.SaveChanges();
```

### 5. Progress reporting

Import runs on a background thread (`ConfigureAwait(false)`).  
Progress is reported via `ViewModel` properties bound to a `ProgressBar`.

### 6. Command-line login skip

If the wizard is launched with a URL containing credentials
(`http://ADM:123@myserver`), `AppLoginPage` is skipped and the connection
is established silently.

---

## AppVision SDK Reference

### `AppServer` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(hostUrl)` | Connect |
| `.Login(user, pwd)` | Authenticate |
| `.LoadDatabase(force)` | Download config cache |

### `VariableManager` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.GetRowByName(name)` | Look up an existing variable |
| `.Add(row)` | Create a new variable |
| `.SaveChanges()` | Persist all pending changes |

### `ProtocolManager` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Add(row)` | Create a new protocol entry |

### `VariableRow` / `VariableType` (`appData.dll`)

```csharp
new VariableRow
{
    Name        = "Device1",
    Description = "My device",
    Type        = VariableType.Node,
    Expression  = "$P.MyProtocol",
    Address     = "user:pwd@192.168.1.10"
}
```

---

## How to Build and Run

1. Build in **Release** configuration.  
2. Run:

```
WizardSample.exe [http://user:pwd@hostname]
```

The optional argument pre-fills and skips the login page.

---

## Debugging

Set a breakpoint in `ViewModel.Import.cs / Import()` to trace created objects.  
Check the AppVision Configurator after a successful import to verify the
protocol and variables were created.

---

## See Also

- [DriverAdvanced](../DriverAdvanced/README.md) â€” the driver the wizard would typically configure
- [ClientAdvanced](../ClientAdvanced/README.md) â€” uses `Sdk.Server` (contrast with wizard `AppServer`)
