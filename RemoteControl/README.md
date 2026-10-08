# RemoteControl — Drive the screens of the operator clients

**Used for:** an external application that decides what the operators see: a PSIM or an alarm-handling system
that brings up the camera or the plan of an event on the right workstation, a video wall controller, a button
in another product that opens an AppVision view, an "on alarm, show it to the supervisors" rule.

**Audience:** C# developers who know the AppVision client (windows, containers, synoptics, built-in views).

```
Connecting to SERVER01 as ADM...
Connected. Type 'help' for the commands.
3 client(s) connected to SERVER01:
  OPERATOR01                   user ADM        appClient        since 02:21
  OPERATOR01                   user ADM        RemoteControl    since 02:45
  SERVER01                     user ADM        App.MCP          since 02:18
> load OPERATOR01 @AlarmsList ColumnSort=Severity,Descending
Sent: load '@AlarmsList' on OPERATOR01
> show OPERATOR01
<layout type="Prysm.AppVision.Client.x1">
  <main>
    <document name="@AlarmsList">
      ...
      <arg>ColumnSort=Severity,Descending</arg>
    </document>
  </main>
</layout>
> follow OPERATOR01
Each new alarm will be shown on OPERATOR01
  >> alarm #6 Access denied 5 times at the main entrance: @AlarmsList loaded on OPERATOR01
```

---

## How it works

1. The program logs in as an AppVision user (an SDK client licence).
2. `Sdk.Server.ClientManager` (`IRemoteClientManager`) sends **client commands** through the server: `@Load`,
   `@Open`, `@CloseWindow`, `@LoadContext`, `@LoadEnv`... The server forwards them to the clients matching the
   target.
3. Each client executes the command as if the operator had done it, **with the rights of its own user**.
4. The program can read back what a workstation displays (`GetLayout`), and follow the alarms to react to them.

| File | Role |
|---|---|
| `Program.cs` | Connection, commands, read-back, follow mode |

**References:** `appSDK.dll`, `appData.dll`, `appCommon.dll`, copied next to the exe. **Output:**
`bin\Debug\RemoteControl.exe`. Start an AppVision client (`appClient.exe`) to have something to drive.

---

## Run

```
RemoteControl.exe [host] [user] [password]        (defaults: localhost ADM 123)
```

| Command | SDK call |
|---|---|
| `clients` | `ClientManager.GetClientInfos()` |
| `load <target> <document> [args]` | `ClientManager.Load(target, windowNum: 0, container: "", document, args)` |
| `open <target> <document> [args]` | `ClientManager.Open(target, document, args)` |
| `clear <target>` | `ClientManager.Clear(target, 0, "")` |
| `show <host>` | `ClientManager.GetLayout(host, 1)` |
| `follow <target>` / `follow off` | `AlarmManager.AlarmChanged` → `ClientManager.Load(target, 0, "", "@alarm_<id>")` |

### Targets

| Target | Reaches |
|---|---|
| `PC-01` | the clients of that workstation (host name exactly as listed by `clients`) |
| `@User.JDOE` | wherever that user is logged in: follows the operator, not the machine |
| `@Profile.Supervisors` | every user of a profile |
| `@All` | every connected client |
| `PC-01; @Profile.Supervisors` | a list |

### Documents

| Document | Shows |
|---|---|
| `Building/Floor1.xaml` | a synoptic (path under `Param\Client`); args `Zoom=Auto`, `$V.Node.Var` (centre on that variable), `Parameters=...` |
| `$V.Site.Cam1.Video` | a camera; args `DateStart=-10m`, `Seconds=60` for a playback |
| `$V.Site.Door1` | a variable: its synoptic centred on it, or its chart, or its timetable |
| `$A.Hall`, `$G.Fire` | the synoptic of an area or group, or its alarm list |
| `@AlarmsList`, `@EventsHistory`, `@Cameras`... | a built-in view; list args `ColumnSort=Severity,Descending`, `ColumnFilter=...` |
| `VideoX4` | a layout (then load each container `c1`..`c4`) |
| `Night.env`, `Desk.ctx` | an environment (several windows) or a window context |
| `@alarm_<id>` | the video of an alarm, or the synoptic of its source |
| `""` | clears the container |

`open` also accepts window options: `Size=1200,800`, `Location=100,100`, `Maximized`, `Topmost`,
`WindowTitle=...`, `Unique=replace`.

---

## Key points

**Fire-and-forget.** A command never returns an error: a target that matches nobody, a document that does not
exist, a document the user is not allowed to open, and a success all look the same. Check the result when it
matters: `GetLayout(host, windowNum)` returns the XML of the window (document and arguments of each container).
Read-back needs a single workstation (host name), not a `@User`/`@Profile`/`@All` target.

**The client's rights apply.** The command runs with the user logged in on the target workstation, not with the
user of your program: a view gated by a right is silently refused.

**Window and container.** `windowNum`: `0` active window, `1` main window, `-1` last opened, `n` a window number.
Container `""`: the selected one, else `main`. A layout names its containers (`main`, `left`, `c1`..`cN`...):
read them back with `GetLayout`.

**`@alarm_<id>`** loads the video of the alarm (in playback around its start) or the synoptic of its source
variable centred on it. An alarm with neither (raised by a program, variable without synoptic) would change
nothing on screen: the sample then shows the alarm list. The client resolves the alarm in its own cache, so it
must be a current alarm.

**Same documents elsewhere.** A client script loads the same documents, with the same arguments, in its own
window with `Client.Load(windowNum, container, document, args)`.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| Nothing happens on the workstation | Host name not exactly the one listed by `clients` (short vs fully qualified name), user without the right for that document, wrong document name. Check with `show`. |
| `show` returns nothing | Not a single operator client on that host name: a configurator or this program runs there too and answered first, or the client is not connected. |
| `follow` reacts to some alarms only | It reacts to new alarms (`Start`). An alarm identical to a current one (same source, description, state) is merged into it: `Update` with `Count` + 1, not `Start`. |
| A camera stays black | No player declared for the camera (`CreateControlPlayer`), see [PlayerSimple](../PlayerSimple/README.md). |

---

## Learning path

```
ClientSimple  ->  ClientAlarms           ->  RemoteControl
subscriptions     alarm life cycle           drive the operators' screens
```
