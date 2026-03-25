# ClientAdvanced — Advanced AppVision Client

## Overview

A WPF client application that demonstrates the advanced SDK patterns used when
building **rich dashboards** that embed AppVision WPF controls.

Compared to [ClientSimple](../ClientSimple/README.md), this sample:

- Uses the `Sdk.Server` **singleton** instead of a local `AppServer` instance.
- Loads the AppVision colour theme with `RColor.Load()`.
- Connects with `StartNotifications(true, false)` to receive the **initial state**
  of all variables immediately.
- Reads a server-side project option with `GetOption()`.
- Provides a clean disconnect flow including unsubscribing from events.

**References:** `appCommon.dll`, `appSDK.dll`, `appUI.dll`  
**Output:** `ClientAdvanced.exe` — standalone WPF application

---

## Project Structure

| File | Role |
|---|---|
| `MainWindow.xaml.cs` | Connection, subscription, and disconnect logic |
| `Properties\Settings.settings` | Persisted host / user / password |

---

## Key Concepts

### 1. `Sdk.Server` singleton

`Sdk.Server` is a static `AppServer` instance shared across the entire process.  
All AppVision WPF controls reference `Sdk.Server` internally — always connect it
**before** loading any embedded controls.

```csharp
Sdk.Server.Open("http://localhost");
Sdk.Server.Login("ADM", "123");
await Sdk.Server.LoadDatabase(true);
```

### 2. `RColor.Load(true)`

Called in the **constructor** before `InitializeComponent()` — loads the AppVision
colour resource dictionary so embedded controls use the correct palette:

```csharp
public MainWindow()
{
    RColor.Load(true);   // true = dark theme
    InitializeComponent();
}
```

### 3. `StartNotifications(initialValues, queue)`

```csharp
// Receive current value of every subscribed variable immediately:
Sdk.Server.StartNotifications(true, false);
```

| Parameter | `true` | `false` |
|---|---|---|
| `initialValues` | Server sends current value of all variables on connect | Wait for next change |
| `queue` | Notifications queued | Notifications dispatched directly |

### 4. `GetOption(key)`

Read a named option from the server configuration:

```csharp
string desc = Sdk.Server.GetOption("App.Main.ProjectDescription");
```

### 5. Unsubscribe before `Close()`

Always remove event handlers before disconnecting to avoid callbacks on a
partially torn-down object:

```csharp
Sdk.Server.AlarmManager.AlarmChanged -= OnAlarmChanged;
Sdk.Server.Close();
```

### 6. Persisted connection settings

Host, user, and password are stored in `Properties.Settings` and reloaded on
the next launch.

---

## AppVision SDK Reference

### `Sdk.Server` (static `AppServer` — `appSDK.dll`)

| Member | Description |
|---|---|
| `.Open(hostUrl)` | Connect |
| `.Login(user, pwd)` | Authenticate |
| `.LoadDatabase(force)` | Download config cache |
| `.AddFilterNotifications(f)` | Select scope |
| `.StartNotifications(initial, queue)` | Begin callbacks |
| `.GetOption(key)` | Read a server-side option |
| `.GetLicenseInfo(key)` | Read a licence field |
| `.Close()` | Disconnect |
| `.UserManager.CurrentUser.FullName` | Logged-in user display name |
| `.AlarmManager.AlarmChanged` | Event: alarm status changed |

### `RColor` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.Load(darkTheme)` | Load AppVision colours into `Application.Current.Resources` |

### `Helper` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.GetInnerMessages(ex)` | Flatten nested exception messages into one string |

---

## How to Build and Run

1. Build in **Release** configuration.  
2. Run `ClientAdvanced.exe`.  
3. Fill in host / user / password and click **Connect**.

---

## Debugging

- Inspect `Sdk.Server.IsConnected` to verify the connection state.
- `txtStatus` displays the connected user name and project description.

---

## Learning Path

```
ClientSimple  →  ClientAdvanced  →  WpfControlSimple / WpfControlAdvanced
Standalone        Sdk singleton        Embedded custom controls
```
