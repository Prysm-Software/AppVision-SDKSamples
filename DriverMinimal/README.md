# DriverMinimal — The smallest AppVision driver

**Used for:** understanding the connection sequence of a driver before writing a real one: a **console
application, no GUI, one file**, that logs in as a protocol, follows the variables linked to it and stops when
AppVision asks it to.

**Audience:** C# developers who start with AppVision drivers. Next steps: [DriverSimple](../DriverSimple/README.md)
(WPF, inputs/outputs, alarm, event) and [DriverAdvanced](../DriverAdvanced/README.md) (template for a real driver).

**References:** `appSDK.dll` (`AppServerForDriver`), `appData.dll` (`VariableState`), `appCommon.dll`.
**Output:** `DriverMinimal.exe`.

**Documentation:** *Writing a driver* in the API help served by your server,
`http://<server>/doccsharp/html/fa685819-4ff4-4c40-81ba-4172e43de86b.htm`, and *Notifications and filters*,
`http://<server>/doccsharp/html/72f3b891-cdf6-48e2-b5f1-015f8e023a0d.htm` (URLs relative to your server, see
[Documentation](../README.md#documentation)).

---

## How it works

A **driver** is a separate process that connects an external system to AppVision. AppVision knows it as a
**protocol** (Configurator › Protocols): when the protocol is started, the server launches the executable declared
in its *Manager* field with the protocol name on the command line. The driver then:

```csharp
var server = new AppServerForDriver();
server.Open(hostname);                               // open a session ("" = local server)
server.Login(protocolName);                          // log in as the protocol (throws if unknown)
server.ControllerManager.Closed  += ...;             // AppVision stops the protocol
server.ControllerManager.Restart += ...;             // connection to the server lost
server.VariableManager.StateChanged += ...;          // a subscribed variable changed
server.AddFilterNotifications("$V.MinimalNode.*");   // which variables to receive
server.StartNotifications(false);                    // start receiving notifications and commands
server.ProtocolSynchronized();                       // protocol state "Synchronized": the driver is ready
...
server.VariableManager.Set("MinimalNode.Value", 42); // push a value read from the device
...
server.Close();                                      // clean disconnect (logout)
```

| Step | What to know |
|---|---|
| Command line | `protocolName[@hostname]`. The server may append `-password=...` (protocol password), which `Login` reads by itself. When the server only listens in HTTPS, the host part is a URL: `MyProtocol@https://localhost:443`. |
| Variables of the driver | The variables whose **Source** is `$P.<protocolName>` (`GetVariablesByProtocol()`). Usually one node per device, its children carry the values. |
| Filter | `$V.` masks with `*`, several masks joined with `\|` **in one call**: a new `$V.` filter replaces the previous one. Without a `$V.` filter, the driver receives no variable change. |
| `StartNotifications` | Required even without a filter: it is also the channel of the AppVision commands (Close, Download, Refresh). |
| Threads | `StateChanged`, `Closed` and `Restart` are raised on SDK background threads. |
| `Closed` | AppVision stops the protocol (configurator, server shutdown, protocol restart): exit. |
| `Restart` | Raised by the SDK when the keep-alive with the server fails; the SDK then closes the session. Exit too: AppVision relaunches the protocols marked *Autostart* (since 4.8.5 a protocol restart is a close followed by a new process). |
| `StateChanged` | Receives every change of the subscribed variables, including the values written by the driver itself. See DriverAdvanced to receive the command variables only. |

This sample subscribes to the children of each node linked to the protocol and prints every change. `Ctrl+C`
stops it cleanly.

---

## Step 1 — Declare the protocol and a variable

In the **Configurator**:

1. **Protocols › Add**: *Name* `DriverMinimal`, *Manager* `DriverMinimal.exe` (path relative to the AppVision
   `Bin` folder, or an absolute path). Leave *Autostart* unchecked while you develop. The *Manager* field of the
   configurator is the `Executable` property of `ProtocolRow` in the SDK.
2. **Variables**: create a node `MinimalNode` and, in its *Parameters* tab, set *Source* to `$P.DriverMinimal`.
3. Create one or two children, for example `MinimalNode.Value` (Integer) and `MinimalNode.Cmd` (Logical, *Command*
   checked).

---

## Step 2 — Build and deploy

1. Build the project. The output goes to `bin\Debug\` (or `bin\Release\`) in the project folder, together with
   copies of the AppVision DLLs: it can run from there (e.g. from Visual Studio, with the protocol name as argument).
2. To let the server start it, copy `DriverMinimal.exe` (and `DriverMinimal.exe.config`) into the AppVision `Bin`
   folder, where the AppVision DLLs already are; or set the *Manager* of the protocol to the absolute path of the
   built exe.

> Production drivers are delivered in their own folder, `Bin\Drivers\<name>\appDriver<name>.exe`, with an
> `AssemblyResolve` handler that loads the AppVision DLLs from `Bin`: see [DriverAdvanced](../DriverAdvanced/README.md).

---

## Step 3 — Run

**Started by AppVision:** Configurator › Protocols › *Start* on the `DriverMinimal` line. A console window opens and
the protocol state becomes *Synchronized*.

**Started from Visual Studio** (debugging): make sure the protocol is stopped in AppVision, then set
*Project › Properties › Debug › Command line arguments*:

```
DriverMinimal
DriverMinimal@localhost
DriverMinimal@192.168.1.20 -password=xxx       (remote server, protocol with a password)
```

Without argument the sample uses `DriverMinimal` on the local server.

Change `MinimalNode.Cmd` from a client or the configurator: the console prints the new value.

```
Connecting to AppVision server '' using protocol 'DriverMinimal'
Ready. Press Ctrl+C to stop.
08/10/2026 18:26:23 +02:00: MinimalNode.Cmd = 1
08/10/2026 18:26:23 +02:00: MinimalNode.Value = 42
```

The server name is empty when the driver connects to the local server.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `Error: ...` then exit after 5 s | Server not running or not reachable (check the host and the port), or unknown protocol name. |
| "No variable has its Source set to $P.DriverMinimal" | No node is linked to the protocol (Step 1.2), or the protocol name differs. |
| Nothing is printed | The variable changed is not a child of a linked node, or the driver was started before the variable was created (restart it). |
| The protocol does not start from the configurator | `DriverMinimal.exe` is not where *Manager* points (relative to `Bin`); see the server window, *Errors* tab. |
| Two consoles | The protocol was started by AppVision and from Visual Studio at the same time: stop one. |
| Login refused | The protocol has a password (mandatory with a high security level): add `-password=...` to the command line. |

Log file of the driver (written by the SDK): `LogFiles\<year>\<product>\` under `%ProgramData%\<folder>\`, where
`<folder>` is the name of the folder that contains the `Bin` folder of the loaded `appCommon.dll`: the installation
folder when the driver runs from the AppVision `Bin` folder (folder opened by the *Log files* button of the server
window), `bin` when it runs from its own `bin\Debug` with copies of the DLLs.

---

## Learning path

```
DriverMinimal  ->  DriverSimple       ->  DriverAdvanced
console, no GUI    WPF, I/O, alarms       template: devices, cache, logging, deployment
```
