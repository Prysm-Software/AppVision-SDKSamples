# NodeClient — AppVision JavaScript SDK from Node.js

**Used for:** connecting a server-side JavaScript program (a gateway, a scheduled job, a bridge to another system)
to AppVision with the JavaScript SDK `@prysm/appsdk`: read live values and alarms, follow them as they change,
write a command, acknowledge an alarm, read the event history. The same calls work in a browser application; see
[WebCustomClient](../WebCustomClient/README.md) for a React page.

**Audience:** developers who know JavaScript and npm; no .NET and no Visual Studio needed.

**Requirements:** Node.js 18 or later, an AppVision server reachable over HTTP, a user account (default
`ADM` / `123` on a test server). The SDK version must match the server: the package `4.9.76` pinned in
`package.json` works with AppVision 4.10.0 and 4.10.1; SDK `4.9.79` and later require an AppVision 4.10.2 server. Install the
version that your server serves (Step 1).

**Documentation:** *Web services and JavaScript SDK* in the API help served by your server,
`http://<server>/doccsharp/html/11442ac1-c309-4c8f-8837-050c0cf5fe13.htm`, and the reference of the JavaScript SDK,
`http://<server>/docjs/index.html` (URLs relative to your server, see [Documentation](../README.md#documentation)).

---

## How it works

1. The SDK is installed **from your AppVision server**, which serves the version that matches it.
2. `server.login()` opens a session and loads the configuration (variables, areas, groups, current alarms...) into
   memory. Reads of that cache (`getRows()`, `getStates()`, `getAlarms()`) are synchronous.
3. `server.startKeepAlive()` polls the server's notification channel and feeds the SDK's rxjs `Subject`s:
   `variableManager.statesChanged`, `alarmManager.alarmsChanged`, `eventManager.newEvents`...
4. Commands (`variableManager.set`, `alarmManager.acknowledgeById`...) and history queries are async HTTP calls.
5. `server.logout()` closes the session (and frees the client licence).

| File | Role |
|---|---|
| `index.js` | The whole sample: login, summary, live feed, `set`, `ack`, `history` |
| `package.json` | Dependency on the SDK tarball served by AppVision |

---

## Step 1 — Install the SDK from the AppVision server

The AppVision MCP server publishes the SDK package (no npm account, no token):

```bash
curl http://localhost/mcp/sdk
```

returns the available version and the exact install command, for example:

```json
{ "packages": [ { "name": "@prysm/appsdk", "version": "4.9.76",
    "install": "npm i http://localhost/mcp/sdk/prysm-appsdk-4.9.76.tgz" } ] }
```

In this folder, replace `localhost` in `package.json` by your server if needed, then:

```bash
npm install
```

`rxjs`, the only other dependency, comes from the public npm registry. The lock file is not committed because it
pins the tarball URL of one server.

---

## Step 2 — Run

```bash
node index.js
```

Connection settings come from environment variables: `APPVISION_HOST` (default `http://localhost`),
`APPVISION_USER` (`ADM`), `APPVISION_PASSWORD` (`123`). In PowerShell: `$env:APPVISION_HOST="http://10.0.0.5"`.

Output on the configuration of [DriverSimple](../DriverSimple/README.md), while a value is written and an alarm
acknowledged from a second terminal:

```
Connecting to http://localhost as ADM...
Connected.

18 variables, 15 areas, 0 groups

Variables with a value:
  InputOutput.Buffer                       0          (stale)
  InputOutput.Input1                       0
  ...
  InputOutput.Output8                      0          (stale)

2 current alarm(s):
  #2 [1] ExecuteProcess DriverSimple.exe DriverSimple The system cannot find the file specified - Error (to acknowledge)
  #3 [95] Simple driver demo - Fault (to acknowledge)

Following variables and alarms, Ctrl+C to quit...
02:33:01  InputOutput.Output1 = 1
02:33:10  alarm #2 Acknowledge: ExecuteProcess DriverSimple.exe DriverSimple The system cannot find the file specified
02:33:10  alarm #2 Delete: ExecuteProcess DriverSimple.exe DriverSimple The system cannot find the file specified
```

The other commands:

```bash
node index.js set InputOutput.Output1 1     # write a value
node index.js ack 2                         # acknowledge alarm #2
node index.js history 24                    # events of the last 24 hours
```

---

## Key points

**Node and SDK 4.9.76.** Up to 4.9.76, the SDK is written for the browser and needs two adjustments in Node:

```js
globalThis.window ??= globalThis;   // the SDK calls window.btoa / window.atob; Node has them as globals
...
await server.login({ host, username, password,
	productProcess: 'node' });      // otherwise login() reads navigator.userAgent, missing in Node < 21
```

From SDK 4.9.77 on, the SDK runs in Node.js 18+ as is: both lines become unnecessary (harmless if kept).

**One session per application.** `login()` loads the whole configuration: create one `AppServer` and share it.

**Nothing moves without `startKeepAlive()`.** Login succeeds and the cache is filled once, but no `Subject`
fires until the keep-alive runs. Subscribe to `connectionLost` to know when the session is gone (after
`keepAliveFailRetryNb` failed polls, 3 by default).

**Values are strings**, in both directions:

| Variable type | Value written |
|---|---|
| Logical | `'1'` / `'0'` |
| Enum | state index (`'2'`), not the label |
| Integer, Reader, Timetable | `'42'` |
| Double | `'21.5'` (always `.`) |
| String | the text |

`set()` does not update the cache: the new value comes back through `statesChanged`. A variable whose value is
calculated (Source `=...`) is recalculated by the server right after the write.

**Quality.** `255` = good, `0` = unknown or stale (driver stopped, no value yet): never display a quality-0 value
as a real reading.

**Alarms.** `alarmsChanged` sends `AlarmInfo { Alarm, Event }` (`Start`, `End`, `Acknowledge`, `Report`,
`Delete`...). Acknowledging takes an alarm in charge; it disappears only when its condition is over and, if the
alarm requires it, its report written (`report(id, text, false)`). Read `WaitAcknowledge`, `WaitEnd`,
`WaitReport` and `IsReadyToClose` on the `AlarmRow` to know what is still expected.

**History** (`eventManager.getHistoEvents`, `alarmManager.getHistoAlarms`, `variableManager.getHistoValues`) is
read from the server and limited by `maxCount`: query a time window, not the whole history, and never on each
notification.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| `window is not defined` / `navigator is not defined` | SDK 4.9.76 or earlier without the two Node adjustments above. |
| `Error: Unauthorized` | Wrong user or password. |
| `Error: fetch failed` | Server not reachable: check `APPVISION_HOST` (scheme and port). |
| Connected, but no live change | `startKeepAlive()` not called, or the notification filter (`notifFilter`) excludes the variables. |
| `npm install` fails on the SDK URL | The server does not serve the SDK (`/mcp/sdk` empty or 404): the AppVision MCP server is not installed or not running. |
| The session stays listed in *Users connected* | The program was killed without `logout()`; the server ends it after its timeout. |

---

## Learning path

```
NodeClient         ->  WebCustomClient       ->  WebServiceREST
JS SDK in Node         JS SDK in a React app     plain HTTP calls, no SDK
```
