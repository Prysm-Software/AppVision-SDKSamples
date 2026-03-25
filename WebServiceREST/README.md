# WebServiceREST -- AppVision REST Web Service Sample

## Overview

A **pure HTML + JavaScript** demo that connects to the AppVision REST service
(`AppVisionService.svc`) directly from the browser -- no SDK, no build step,
no back-end server required.

Capabilities demonstrated:

1. Open a session and authenticate with username / password.
2. Subscribe to all server notifications (`AddFilterNotifications` / `StartNotifications`).
3. Receive a real-time stream of alarms, events and variable changes via polling
   (`KeepAlive` / `GetNotifications`).
4. Display the live event feed in the browser.
5. Close the session cleanly.

> Use this as a starting point for any browser-based integration that needs live
> AppVision data without the .NET SDK.

**Dependencies:** none -- vanilla JavaScript, Fetch API, modern browser  
**Output:** static HTML files served by IIS (or any web server)

---

## Project Structure

| File | Role |
|---|---|
| `login.html`   | Entry page -- collects host, username and password |
| `main.html`    | Main page -- opens the session and shows the live event feed |
| `appServer.js` | `AppServer` class -- wraps the entire REST API surface |
| `Web.config`   | ASP.NET / IIS hosting configuration |

---

## How It Works

```
Browser                          AppVision server
  |                                     |
  |-- GET /Open ----------------------->|  establish session (receives cookie)
  |<- 200 sessionId -------------------|
  |                                     |
  |-- POST /Login -------------------->|  authenticate
  |<- 200 -----------------------------|
  |                                     |
  |-- POST /AddFilterNotifications --->|  subscribe to all events
  |-- GET  /StartNotifications ------->|
  |                                     |
  |   every 500 ms:                     |
  |-- GET /KeepAlive ----------------->|  poll for pending notification count
  |<- count ---------------------------|
  |-- GET /GetNotifications?count=100->|  fetch up to 100 payloads (if count>0)
  |<- [ VariableState | AlarmInfo | EventRow | ServerState ] --|
  |                                     |
  |-- GET /Logout -------------------->|  close session
```

---

## Key Concepts

### 1. `AppServer` class (`appServer.js`)

A single class encapsulates all REST calls.  Instantiate it once, call
`login()`, then listen to the `#log` element for live updates.

```javascript
const srv = new AppServer();
await srv.login('http://localhost', 'ADM', '123');
// real-time events now appear in document.getElementById('log')
srv.logout();
```

After a successful `login()` the instance exposes:

| Property | Type | Description |
|---|---|---|
| `srv.user` | object | User record from `GetCurrentUser` |
| `srv.desc` | string | Project name from `GetOption(App.Main.ProjectDescription)` |
| `srv.serverState` | object | Latest `ServerState` notification |

---

### 2. `login(host, user, pwd)` -- session setup

```javascript
async login(host, user, pwd)
```

Performs the mandatory five-step connection sequence:

| Step | Endpoint | Purpose |
|---|---|---|
| 1 | `GET /Open` | Creates the HTTP session; the server sets a session cookie |
| 2 | `POST /Login` | Authenticates with `[{Name:"username",...},{Name:"password",...}]` |
| 3 | `GET /GetCurrentUser` | Background fetch -- populates `this.user` |
| 3 | `GET /GetOption?name=App.Main.ProjectDescription` | Background fetch -- populates `this.desc` |
| 4 | `POST /AddFilterNotifications` body `"All"` | Subscribes to every event type |
| 4 | `GET /StartNotifications?send=false` | Activates the notification queue |
| 5 | `setTimeout(keepAlive, 500)` | Starts the polling loop |

> **AD / LDAP:** when Windows or LDAP authentication is enabled, uncomment
> the `Authorization: Basic ...` header line in `login()`.

---

### 3. `keepAlive()` -- notification polling loop

```
GET /KeepAlive          --> returns pending count (integer)
GET /GetNotifications   --> returns array of notification wrappers
                            each wrapper has a Data field (object or array)
```

The loop reschedules itself 500 ms after every successful response.  
On network error the exception is written to the document and polling stops.

---

### 4. `gotEvent(notif)` -- notification dispatch

Each notification carries a `__type` discriminator (WCF JSON format):

| `__type` | Payload fields used | Display |
|---|---|---|
| `ServerState:#Prysm.AppVision.Data` | whole object | stored in `srv.serverState` |
| `AlarmInfo:#Prysm.AppVision.Data` | `notif.Event`, `notif.Alarm.Description` | `Alarm:<event>: <desc>` |
| `EventRow:#Prysm.AppVision.Data` | `notif.StateDesc`, `notif.Description` | `<stateDesc>: <desc>` |
| `VariableState:#Prysm.AppVision.Data` | `notif.Name`, `notif.Value` | `<name>=<value>` |

---

## Running the Sample

### Prerequisites

| Requirement | Details |
|---|---|
| **AppVision** | 4.x running and reachable at `http://localhost` |
| **IIS** | Site pointing at this folder, or use VS IIS Express |
| **Browser** | Any modern browser with Fetch API support (Chrome, Edge, Firefox) |

### Steps

1. Deploy the three files (`login.html`, `main.html`, `appServer.js`) to an IIS
   virtual directory, **or** open the project in Visual Studio and press **F5**
   to launch with IIS Express.
2. Navigate to `login.html`.
3. Enter the server URL, username and password (defaults: `http://localhost` /
   `ADM` / `123`).
4. Click **Login** -- you will be redirected to `main.html`.
5. The connection banner shows `<LastName> connected to <project> at <host>`.
6. Trigger alarms or change variable values in AppVision; they appear live in
   the log area.

### URL format

`login.html` submits the form via a standard GET, producing a URL like:

```
main.html?host=http://localhost&user=ADM&pwd=123
```

`main.html` reads these three parameters with `URLSearchParams` and passes
them directly to `AppServer.login()`.

---

## Extending the Sample

Any AppVision REST endpoint can be called with the same pattern.

**Read a variable value:**
```javascript
const resp = await fetch(
    `${host}/AppVisionService.svc/GetVariableState?name=$V.MyVar`,
    appServer.get);
const state = await resp.json();
console.log(state.Value);
```

**Set a variable value:**
```javascript
await fetch(`${host}/AppVisionService.svc/SetVariableState`, {
    ...appServer.post,
    body: JSON.stringify({ Name: '$V.MyVar', Value: 1 })
});
```

**Filter notifications by variable name instead of "All":**
```javascript
await fetch(`${host}/AppVisionService.svc/AddFilterNotifications`, {
    ...appServer.post,
    body: '"$V.MyVar"'   // only receive state changes for this variable
});
```

---

## AppVision REST API Reference

Base URL: `http://localhost/AppVisionService.svc/`

| Method | Endpoint | Description |
|---|---|---|
| GET | `Open` | Create session, returns session ID |
| POST | `Login` | Authenticate with `NameValue[]` credentials |
| GET | `GetCurrentUser` | Logged-in user record |
| GET | `GetOption?name=<key>` | Read a single configuration option |
| GET | `Logout` | Close session |
| POST | `AddFilterNotifications` | Subscribe to a filter (e.g. `"All"`, `"$V.*"`) |
| GET | `StartNotifications?send=false` | Start the notification queue |
| GET | `KeepAlive` | Returns count of buffered notifications |
| GET | `GetNotifications?count=<n>` | Dequeue up to `n` notifications |
| GET | `GetVariableState?name=<var>` | Read current value of a variable |
| POST | `SetVariableState` | Write a variable value |
