# ClientAlarms — Follow and handle alarms, read the history

**Used for:** connecting another system to the alarms of AppVision: a ticketing or maintenance tool that opens a
ticket for each alarm and acknowledges it, a PSIM that takes over the treatment, an external system that raises
its own faults in AppVision, a statistics job that reads the alarm and event history.

**Audience:** C# developers who know the basics of AppVision alarms (severity, acknowledgement, report).

A console application: commands typed on the keyboard, changes printed as they arrive.

```
Connecting to localhost as ADM...
Connected. Type 'help' for the commands.
1 current alarm(s):
  #3     [ 95] 2026-10-08 02:27  Simple driver demo - Fault  (to acknowledge, condition active)
> raise 40 Chiller 2 - compressor fault
Alarm #4 raised
  >> Start       #4 [40] Chiller 2 - compressor fault - External
> report 4 Technician called, compressor restarted
Report written on alarm #4
  >> Report      #4 [40] Chiller 2 - compressor fault - External
> ack 4
Alarm #4 acknowledged
  >> Acknowledge #4 [40] Chiller 2 - compressor fault - External
  >> Delete      #4 [40] Chiller 2 - compressor fault - External
> close 3
Error: Alarm #3 cannot be closed yet (to acknowledge, condition active)
> history 1
  2026-10-08 02:18  #2     [  1] ExecuteProcess DriverSimple.exe DriverSimple ... - Error  ack by ADM 02:33
  2026-10-08 02:39  #4     [ 40] Chiller 2 - compressor fault - External  ack by ADM 02:39
2 alarm(s) since 2026-10-08 01:39
```

---

## How it works

1. `AppServer.Open` / `Login` open a session as an AppVision user (an SDK client licence is used).
2. `LoadDatabase(true)` fills the local cache (areas, groups, users, variables) that the alarm lists use.
3. `AddFilterNotifications("NotificationType=AlarmRow")` + `StartNotifications(true, false)`: the server sends the
   current alarms, then each change, to `AlarmManager.AlarmChanged`.
4. The operator actions (`AcknowledgeAlarm`, `ReportAlarm`, `CloseCompletedAlarm`...) are server calls; their
   effect comes back as a notification.
5. The history is read by pages (`GetHistoAlarmsByPage`, `GetHistoEventsByPage`).

| File | Role |
|---|---|
| `Program.cs` | Connection, notifications, commands |

**References:** `appSDK.dll`, `appData.dll`, `appCommon.dll` from the AppVision `Bin` folder, copied next to the exe
(a client application runs outside the AppVision folder). **Output:** `bin\Debug\ClientAlarms.exe`.

**Documentation:** *Alarm model* in the API help served by your server,
`http://<server>/doccsharp/html/6e5e199a-5dd5-4048-84ad-5975fe79add9.htm` (URL relative to your server, see
[Documentation](../README.md#documentation)).

---

## Run

```
ClientAlarms.exe [host] [user] [password]        (defaults: localhost ADM 123)
```

| Command | SDK call |
|---|---|
| `list [severity]` | `AlarmManager.GetCurrentAlarms("Severity >= 50")` |
| `ack <id>` / `ack all` | `AcknowledgeAlarm(id)` / `AcknowledgeAllAlarms()` |
| `report <id> <text>` | `ReportAlarm(id, text, alarmWaiting: false)` |
| `pause <id> <text>` | `ReportAlarm(id, text, alarmWaiting: true)` |
| `close <id>` | `CloseCompletedAlarm(id)` |
| `raise <severity> <text>` | `AddAlarmExt(severity, state, description, info)` |
| `history [hours]` | `GetHistoAlarmsByPage(page, 100, false, since, ...)` |
| `events [hours]` | `EventManager.GetHistoEventsByPage(page, 100, false, since, ...)` |

Any configuration works. With the one of [DriverSimple](../DriverSimple/README.md), its protocol alarm and the
inputs give alarms to play with; `raise` creates some in an empty configuration.

---

## The alarm life cycle

An alarm moves on **two independent axes**, and leaves the list of current alarms only when both are over:

| Axis | Fields of `AlarmRow` | Who moves it |
|---|---|---|
| Condition | `WaitEnd`: the source is still in alarm | the system only |
| Treatment | `WaitAcknowledge`, `WaitReport` (report or workflow expected), `WaitProcess` (paused), `Status` | the operators |

`Status` sums up the treatment: `AcknowledgeWaiting` (10) → `Processing` (20) / `ProcessWaiting` (30, paused) →
`Completed` (70) → `Closed` (100).

- **Acknowledging does not end an alarm**: it takes it in charge. An acknowledged alarm whose condition is still
  active stays in the list (`Completed`, `condition active`).
- **A report written before the acknowledgement is kept.** With nothing else pending, the acknowledgement then
  completes the alarm, and the server closes it at once (`Acknowledge` then `Delete` notifications).
- **Pause** = a report with `alarmWaiting: true`: the treatment stays open (`ProcessWaiting`) until resumed
  (`ResumeAlarm`) or reported again.
- **Close** applies to a `Completed` alarm whose condition is over. The server closes these alarms by itself,
  unless the configuration requires a manual close. A close request on another alarm is silently ignored, hence
  the check in the sample. `IsReadyToClose` means "report or workflow completed", **not** "condition over".
- **An alarm raised by `AddAlarmExt`** (not by a variable) has no condition: it ends once treated. Raised again
  while it is current (same description and state), it is not duplicated: the server increments its `Count` and
  sends an `Update`.
- **Masking** (`MaskAlarm`) suppresses the future alarms of a variable; it does not treat the current one.

`AlarmChanged` reports each step with `AlarmInfo.Event`: `Start`, `End` (condition over), `Acknowledge`,
`Report`, `Resume`, `Transfer`, `Update`, `Delete` (left the current list).

---

## Key points

**Notifications arrive on a background thread.** In a WPF application, go back to the UI thread
(`Dispatcher.InvokeAsync`) before touching the window. Here a lock keeps the console lines whole.

**Subscribe before `StartNotifications`**, otherwise the first changes are lost. `StartNotifications(true, ...)`
also sends the current alarms, which fill the cache read by `GetCurrentAlarms` and `GetCurrentAlarmById`.

**Polling or web service.** `StartNotifications(..., useWebService: false)` receives the notifications by polling
the server. With `true` (the default), the SDK opens a small HTTP listener in your process for the server to push
to: faster, but the port must be reachable from the server and the URL reserved for a non-administrator account
(`netsh http add urlacl`).

**Filters.** `GetCurrentAlarms` and the history accept `$A.Area`, `$G.Group`, `$V.Node.*` (several values joined
by `|`), `Severity >= x`, `Status=10`. The notification filter can be narrowed the same way, e.g.
`$A.Building1` to receive only the alarms of an area.

**History by pages.** `GetHistoAlarmsByPage` / `GetHistoEventsByPage` return the rows from a date, oldest first,
`pageSize` at a time: ask for the next page while the current one is full, and keep the time window reasonable.
The alarm history lists the alarms that have ended. The `GetHistoAlarms(minId, ...)` overloads are obsolete.

**Close the session** (`Close()`) when leaving: it frees the SDK client licence at once, otherwise the server
keeps it until its timeout.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `Error: Login:Invalid user or password` | Wrong user or password. |
| `No connection could be made because the target machine actively refused it` | Server not running, or wrong host / port. |
| Licence error at login | No SDK client left in the licence: close the other SDK sessions. |
| `list` is empty while the client shows alarms | Notifications not started with `sendInitialValues: true`, or the user's profile does not see those areas. |
| `close` refuses | The alarm is not `Completed` or its condition is still active (see the life cycle). |
| `ack` answers "not acknowledged" | Unknown id, alarm already acknowledged, or the user is not allowed to acknowledge it. |
| No live change with `useWebService: true` | The server cannot reach the listener of the client (firewall, URL reservation). Use polling. |

---

## Learning path

```
ClientSimple  ->  ClientAlarms                 ->  ClientAdvanced
subscriptions     alarm life cycle, history        Sdk.Server, theme, options
```

The same operations exist in the JavaScript SDK: see [NodeClient](../NodeClient/README.md).
