# WebCustomClient ? AppVision Custom Web Client Panel

## Overview

A **React 18 + TypeScript** single-page application that runs as a custom
panel embedded inside the **AppVision Web Client**.

AppVision loads the built app in an `<iframe>` and injects its live
TypeScript SDK (`AppServer`) into the parent window. The panel reads that
reference ? **no separate login, no extra HTTP calls** ? and immediately
has access to the full AppVision data model.

Capabilities demonstrated:

1. Retrieve the shared `AppServer` instance from `window.parent.window.sdk.server`.
2. Read a snapshot of all variable states with `variableManager.getStates()`.
3. Subscribe to real-time alarm updates via `alarmManager.alarmsChanged`.
4. Display the connected user name and server URL from `AppServer`.

> Use this as a template for any panel that needs live AppVision data inside
> the Web Client without implementing its own authentication.

**Stack:** React 18, TypeScript 4, Create React App
**SDK:** `@prysm/appsdk` (local package in `AppSdk-TS/`)

---

## Project Structure

```
WebCustomClient/
+-- src/
|   +-- App.tsx        Main React component (SDK init + alarm subscription)
|   +-- App.css        Component styles
|   +-- index.tsx      React root mount point
|   \-- index.css      Global styles
+-- AppSdk-TS/         Local AppVision TypeScript SDK package (@prysm/appsdk)
+-- .env.production    Sets PUBLIC_URL=/CustomControl/ for the build output
+-- package.json       NPM dependencies and scripts
+-- tsconfig.json      TypeScript compiler configuration
\-- README.md
```

---

## How It Works

```
AppVision Web Client (browser tab)
+--------------------------------------------------+
|  window.sdk.server  <-- shared AppServer         |
|                                                  |
|  +------ <iframe src="/CustomControl/"> -------+ |
|  |                                             | |
|  |  App.tsx                                   | |
|  |   useEffect #1  --> initSdk()              | |
|  |    window.parent.window.sdk.server         | |
|  |          |  (stored in React state)        | |
|  |   useEffect #2  --> variableManager        | |
|  |                     alarmManager           | |
|  +---------------------------------------------+ |
+--------------------------------------------------+
```

The two `useEffect` hooks are chained:

| Hook | Trigger | Action |
|---|---|---|
| Effect 1 | component mount | Reads `sdk.server` from the parent window into React state |
| Effect 2 | `appServer` state change | Fetches variable snapshot, subscribes to alarms |

---

## Key Concepts

### 1. Accessing the shared SDK

```typescript
const appServer: AppServer =
  (window.parent.window as any).sdk?.server;
```

`sdk` is `undefined` when opened outside AppVision (local dev), so the
call is wrapped in `try/catch` ? the component renders without the
connection banner in that case.

---

### 2. Reading variable states (one-shot snapshot)

```typescript
const states: VariableState[] = await appServer.variableManager.getStates();
```

Returns the current value of every variable visible to the logged-in user.
To target a single variable use `getState(name)`.

---

### 3. Subscribing to real-time alarm changes (RxJS)

The SDK exposes data streams as **RxJS Subjects**. Every subscription
returns an object with an `unsubscribe()` method:

```typescript
const sub = appServer.alarmManager.alarmsChanged.subscribe(
  (alarms: AlarmInfo[]) => {
    alarms.forEach(a => console.log(a));
  }
);

// React useEffect cleanup ? prevents memory leaks on unmount
return () => sub.unsubscribe();
```

---

### 4. Available managers on `AppServer`

| Manager | Access | Description |
|---|---|---|
| `variableManager` | `appServer.variableManager` | Read/write values, subscribe to changes |
| `alarmManager` | `appServer.alarmManager` | Active alarms, acknowledge, subscribe |
| `eventManager` | `appServer.eventManager` | Historical and live event log |
| `userManager` | `appServer.userManager` | Current user, user list |
| `operandManager` | `appServer.operandManager` | Direct operand read/write |
| `groupManager` | `appServer.groupManager` | Variable groups |
| `fileManager` | `appServer.fileManager` | Server file upload/download |
| `scenarioManager` | `appServer.scenarioManager` | Scenario execution |
| `videoManager` | `appServer.videoManager` | Camera streams |
| `overlayManager` | `appServer.overlayManager` | GIS overlay items |
| `peopleManager` | `appServer.peopleManager` | AppControl people and badges |
| `http` | `appServer.http` | Raw HTTP service (exposes `host` URL) |

---

## Getting Started

### Prerequisites

| Requirement | Version |
|---|---|
| Node.js | 16 or later |
| npm | 8 or later |
| AppVision Web Client | 4.x |

### Install dependencies

```bash
cd WebCustomClient
npm install
```

### Run in development mode

```bash
npm start
```

Opens `http://localhost:3000`. The SDK is not available outside AppVision,
so the connection banner will not appear ? this is expected and handled by
the `try/catch` in `initSdk()`.

### Build for production

```bash
npm run build
```

Outputs the optimised bundle to `build/` with `PUBLIC_URL=/CustomControl/`
(set in `.env.production`).

---

## Deployment

1. Copy the contents of `build/` into the AppVision web folder:

   ```
   AppVision 4\Bin\web\CustomControl\
   ```

2. The panel is then accessible at:

   ```
   http://<appvision-host>/CustomControl/
   ```

3. In the AppVision **Configurator**, add a *Custom Web Control* element to a
   synoptic and point its URL to `/CustomControl/`.

4. Open the synoptic in the AppVision **Web Client** ? the panel loads inside
   its `<iframe>` with the shared SDK already connected.

> To rename the deployment folder, change `PUBLIC_URL` in `.env.production`
> and copy the build output to the matching subfolder.

---

## Extending the Sample

**Write a variable value:**
```typescript
await appServer.variableManager.set('$V.MyVar', '1');
```

**Subscribe to variable state changes:**
```typescript
const sub = appServer.variableManager.statesChanged.subscribe(
  (states: VariableState[]) => {
    states.forEach(s => console.log(`${s.name} = ${s.value}`));
  }
);
```

**Read the current user:**
```typescript
const { fullName, email } = appServer.userManager.currentUser;
```

**Execute a scenario:**
```typescript
await appServer.scenarioManager.execute('MyScenario');
```

---

## AppVision TypeScript SDK (`AppSdk-TS/`)

The local `@prysm/appsdk` package is a pre-compiled TypeScript SDK bundled
with this sample and declared as a local dependency in `package.json`:

```json
"@prysm/appsdk": "./AppSdk-TS/"
```

Type declaration files (`.d.ts`) are included, providing full IntelliSense
in VS Code and other IDEs without requiring SDK source access.
