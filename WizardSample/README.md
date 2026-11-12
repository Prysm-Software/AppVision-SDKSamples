# WizardSample — Configuration wizard for your driver

**Used for:** delivering, with your driver, a wizard that creates its AppVision configuration: it logs in to the
AppVision server, discovers the devices on the network, reads what each device contains, then creates the protocol,
the variables and the groups in one go. Integrators then commission a site in minutes instead of typing hundreds of
variables in the configurator.

**Audience:** driver developers (see [DriverSimple](../DriverSimple/README.md) / [DriverAdvanced](../DriverAdvanced/README.md))
who know how their driver expects its variables (names, types, addresses).

The sample is a skeleton: every `<todo>` marks device-specific code to write, every `<demo>` marks placeholder code
to replace (the "discovery" pings the local subnet, the "entities" of a device are the files of the public
documents folder).

![Four of the nine pages, run from Bin\Drivers\WizardSample: AppVision login, entities of a device, summary, import result](preview.png)

**References:** `appSDK.dll`, `appData.dll`, `appCommon.dll`, Telerik UI for WPF (`Telerik.Windows.*`, shipped in
the AppVision `Bin` folder). **Output:** `appWizardSample.exe` — standalone WPF application.

**Documentation:** *Writing a driver* in the API help served by your server,
`http://<server>/doccsharp/html/fa685819-4ff4-4c40-81ba-4172e43de86b.htm` (URL relative to your server, see
[Documentation](../README.md#documentation)).

---

## How it works

| File | Role |
|---|---|
| `MainWindow.xaml(.cs)` | Shell: left banner + the current page (`ContentPresenter` bound to `CurrentPage`) |
| `ViewModel.cs` | Navigation commands, shared state, AppVision login, discovery, device scan |
| `ViewModel.Import.cs` | Import: protocol, group, variables |
| `Device.cs` / `Entity.cs` | A discovered device / an item of a device (input, camera, reader...) |
| `AddDeviceDialog.xaml` | Manual addition of a device that the discovery did not find |
| `App.xaml.cs` | Error handling, resolution of the AppVision DLLs when deployed in a sub-folder of `Bin` |
| `Properties\Settings.settings` | Last host, user names, node and protocol names (passwords are never stored) |
| `Pages\1..9` | The pages, in order |

```
1 Welcome
2 AppVision login       <- skipped when credentials are given on the command line
3 Discovery             <- remove if the device type cannot be discovered
4 Device login          <- remove if the device needs no credentials
5 Device details        <- entities found on the device, to select
6 Settings              <- node name, description, protocol name, group, symbol
7 Summary
8 Import                <- progress, can be cancelled until the variables are saved
9 Completed             <- "Import new device" goes back to the discovery
```

### 1. MVVM navigation

`MainWindow` displays `ViewModel.CurrentPage`. Each button of a page is bound to a command of the view model, which
does the work of the step (login, scan...) then sets the next page:

```csharp
public ICommand DiscoveryCommand => new RelayCommand(GotoDiscoveryPage, CanGotoDiscoveryPage);

async void GotoDiscoveryPage()
{
    using (new WaitCursor())
        await LoginApp();
    CurrentPage = new DiscoveryPage();
    _ = DiscoverDevices();
}
```

The pages share the view model (`DataContext`): what one page collects (`DeviceHostname`, `SelectedEntities`...) is
read by the next ones.

### 2. AppVision connection: a private `AppServer`, no database cache

The wizard is a standalone tool, not a control hosted by the AppVision client: it creates its own `AppServer`
(not `Sdk.Server`). It does **not** call `LoadDatabase()`: without the local cache, `GetRowByName()` queries the
server, so the wizard always sees the current configuration, even a very large one, without downloading it.

```csharp
appServer.Open(AppHostname);     // on a background thread (Task.Run): the UI stays responsive
appServer.Login(AppUsername, AppPassword);
var existing = appServer.VariableManager.GetRowByName("WizardSample");   // server request
```

The user must have the right to modify the configuration.

### 3. Creating the configuration

```csharp
// protocol: the driver started by the server
appServer.ProtocolManager.AddProtocol(new ProtocolRow
{
    Name = "WizardSample",
    Executable = @"Drivers\WizardSample\appDriverWizardSample.exe",   // the "Manager" field of the configurator, relative to the AppVision Bin folder, without leading '\'
    Icon = "Images/WizardSample.png",           // uploaded with FileManager.PutFile("Param/Client/Images/...")
});

// variables, all in one call
var variables = new List<VariableRow>
{
    new VariableRow { Name = "WizardSample", Type = VariableType.Node, Expression = "$P.WizardSample",
                      Address = "user:password@192.168.0.100" },        // node linked to the protocol
    new VariableRow { Name = "WizardSample.Comm", Type = VariableType.Enum, IsAlarm = true,
                      StateDescs = "0:Unknown\n1:Connected:d\n2:Disconnected\n3:Default:a" },
    new VariableRow { Name = "WizardSample.Door1.Cmd", Type = VariableType.Bool, StateType = 1 }, // 1 = command
};
if (!appServer.VariableManager.AddVariables(variables.ToArray()))
    throw new Exception("License limit reached");

// group membership
appServer.GroupManager.AddGroup(new GroupRow { Name = "WizardSample", Description = "..." });
appServer.VariableManager.AddVariableGroup("WizardSample", "WizardSample");
```

- The **node** (`VariableType.Node`) linked to the protocol by `Expression = "$P.<protocol>"` is what the driver
  receives; `Address` and `Parameters` carry the connection data of the device.
- `StateDescs` lists the states `value:label[:flag]`, one per line. For an alarm variable (`IsAlarm`), `a` starts the
  alarm (`a80`: with severity 80) and `d` ends it; a state without flag leaves the alarm as it is.
- A `.Comm` variable per device, member of the *Communication* group when it exists, follows the AppVision
  convention for communication status.
- `AddVariables()` returns `false` when the license does not allow so many variables. With thousands of variables,
  the server keeps working after the call returns: check the result in the configurator.

### 4. Background work and progress

`Import()` leaves the UI thread (`ConfigureAwait(false)`); it updates `Progress` and `Details` through
`InvokeUI()` / `Notify()`, which go back to the UI thread. Dialogs and `CommandManager` calls also go through the
dispatcher. Cancelling is possible until the variables are sent.

### 5. Command line

```
appWizardSample.exe http://ADM:123@myserver
```

With user and password in the URL, the login page is skipped. Avoid it on shared machines: the password is visible
in the process list.

### 6. Deployment next to a driver

Deploy the wizard with its driver, for example in `Bin\Drivers\WizardSample\` of the installation. `App.xaml.cs` resolves the
AppVision DLLs (`appSDK.dll`, Telerik...) one and two folders up, so they need not be copied next to the exe.

---

## Build and run

1. Build the project. The output goes to `bin\Debug\` (or `bin\Release\`) in the project folder, with the
   referenced AppVision and Telerik DLLs.
2. Start an AppVision server (an empty configuration is fine) and run `appWizardSample.exe`.
3. Log in (`ADM` / `123` on a new configuration). The discovery lists the hosts that answer a ping on your subnet;
   select one or use **Add**, then **Next** through the pages, select a few entities, **Import**.
4. In the configurator, check the protocol `WizardSample`, the node `WizardSample` with its `.Comm` variable and one
   node per entity with `.State` and `.Cmd`.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| Login fails | Wrong address or credentials; the user needs configuration rights. |
| Discovery finds nothing | ICMP blocked by firewalls, or no IPv4 address on the machine. Use **Add**. |
| *Your license does not allow to create N variables* | Variable count of the license reached. |
| The protocol exists but the driver does not start | `Executable` (*Manager* in the configurator) not found relative to the server `Bin` folder; with `App.Server.SecurityLevel` 2 or more, the protocol also needs a password. |
| *Could not load file or assembly 'appSDK'* when deployed in a sub-folder | The wizard is more than two levels below the AppVision `Bin` folder: adapt `CurrentDomain_AssemblyResolve`. |
| Entities already imported are skipped | By design (`<todo>` in `Import()`): update them if your driver needs it. |

**Debugging:** set breakpoints in `ViewModel.Import.cs` / `Import()`; errors are also written to the wizard's log
(`Helper.Log`, folder `%ProgramData%\<folder>\LogFiles\<year>\`, where `<folder>` is the name of the folder that
contains the `Bin` folder of the loaded `appCommon.dll`: the installation folder when the wizard is deployed under
`Bin\Drivers\`, `bin` when it runs from its own `bin\Debug`).

---

## See also

- [DriverAdvanced](../DriverAdvanced/README.md) — the kind of driver such a wizard configures
- [ClientAdvanced](../ClientAdvanced/README.md) — `Sdk.Server` in a client application (contrast with the private `AppServer` here)
