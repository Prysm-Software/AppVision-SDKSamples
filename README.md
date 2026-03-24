# AppVision SDK Samples

A collection of C# samples that demonstrate how to integrate third-party systems,
build custom client applications, embed WPF controls, stream video, and automate
configuration using the **AppVision 4 SDK**.

---

## Audience

These samples target **C# developers** who want to build extensions for AppVision.
A basic AppVision experience is required; the following background is also assumed:

| Topic | Level required |
|---|---|
| C# | Intermediate |
| WPF | Basic (for UI samples) |
| .NET Framework 4.8 | Basic |
| TCP/IP networking | Basic (for driver and web-service samples) |

---

## Prerequisites

| Requirement | Details |
|---|---|
| **AppVision** | 4.x running on localhost (or a reachable host) |
| **Visual Studio** | 2019 or 2022 with .NET desktop workload |
| **.NET Framework** | 4.8 |

### Folder layout expected by the build

```
C:\Program Files\AppVision 4\Bin\
\AppVision 4\Public\AppVision-SDKSamples\   <- this repository
```

The SDK and application DLLs in `AppVision 4\Bin\` are referenced as
`<Private>False</Private>` in every project, so AppVision must be
installed before building.

---

## Repository Structure

```
AppVision-SDKSamples/
|
+-- DriverMinimal/              Console driver -- minimal connection sequence
+-- DriverSimple/               WPF driver    -- 8 I/O, alarms, custom events
+-- DriverAdvanced/             WPF driver    -- Device pattern, caching, logging
+-- DriverAppControl/           WPF driver    -- AppControl hardware bridge
+-- DriverOverlay/              WPF driver    -- animated geo-referenced overlays
|
+-- ClientSimple/               WPF client -- subscribe to variables / alarms / events
+-- ClientAdvanced/             WPF client -- Sdk.Server singleton, theme, options
|
+-- WpfControlSimple/           Embeddable control -- OperandManager read/write
+-- WpfControlAdvanced/         Embeddable control -- IPersistable, ICommandable
|
+-- PlayerSimple/               Video player -- IPlayerLive, FFmpeg software decode
+-- PlayerAdvanced/             Video player -- IPlayerPlayback, PTZ, Export
+-- PlayerMetadata/             Metadata overlay -- YOLO object detection on live video
|
+-- MapProvider/                GIS map provider -- Bing Maps tile background
+-- OverlaySample/              Standalone tool  -- overlay CRUD on a GIS synoptic
|
+-- WizardSample/               Setup wizard -- multi-page device onboarding
+-- TwoFactorAuthentication/    Server plugin -- custom 2-factor login provider
|
+-- AppControlSynchro/          CLI tool -- CSV bulk import of people into AppControl
+-- WebServiceOData/            CLI tool -- AppVision config OData CRUD (no SDK needed)
\-- WebServiceODataAppControl/  CLI tool -- AppControl people/badges OData CRUD
```

---

## Samples by Category

### Driver development

Write a **driver** when you need to push values from an external device into
AppVision, or when AppVision needs to send commands to the device.
Drivers are standalone processes started and supervised by AppVision.

| Project | Description | Level |
|---|---|---|
| [DriverMinimal](DriverMinimal/README.md) | Bare-minimum console driver | Beginner |
| [DriverSimple](DriverSimple/README.md) | WPF driver with 8 digital I/Os, alarm, event | Intermediate |
| [DriverAdvanced](DriverAdvanced/README.md) | Full driver: Device pattern, caching, logging | Advanced |
| [DriverAppControl](DriverAppControl/README.md) | Bridge to AppControl access controller hardware | Advanced |
| [DriverOverlay](DriverOverlay/README.md) | Animate geo-referenced icons on a GIS synoptic | Intermediate |

### Client applications

Build a **standalone application** that reads or controls AppVision data.

| Project | Description | Level |
|---|---|---|
| [ClientSimple](ClientSimple/README.md) | Subscribe to variables, alarms, events; set values | Beginner |
| [ClientAdvanced](ClientAdvanced/README.md) | `Sdk.Server`, colour theme, `GetOption`, clean disconnect | Intermediate |

### Embeddable WPF controls

Create a custom **UserControl** that operators can place directly on an
AppVision synoptic, alongside the built-in widgets.

| Project | Description | Level |
|---|---|---|
| [WpfControlSimple](WpfControlSimple/README.md) | Read/write variables from inside a synoptic | Intermediate |
| [WpfControlAdvanced](WpfControlAdvanced/README.md) | `IPersistable`, `ICommandable`, `IContainer`, async data | Advanced |

### Video players

Build a custom **video player control** that AppVision manages (camera assignment,
recording, motion detection, metadata overlay).

| Project | Description | Level |
|---|---|---|
| [PlayerSimple](PlayerSimple/README.md) | Live stream with FFmpeg software decoder | Intermediate |
| [PlayerAdvanced](PlayerAdvanced/README.md) | Full player: live, playback, PTZ, export | Advanced |
| [PlayerMetadata](PlayerMetadata/README.md) | Draw YOLO bounding-box overlays on live video | Advanced |

### GIS and overlays

| Project | Description | Level |
|---|---|---|
| [MapProvider](MapProvider/README.md) | Custom Bing Maps tile background for GIS synoptics | Intermediate |
| [OverlaySample](OverlaySample/README.md) | Standalone CRUD tool for geo-referenced overlay icons | Intermediate |

### Configuration and server plugins

| Project | Description | Level |
|---|---|---|
| [WizardSample](WizardSample/README.md) | Multi-page WPF wizard for device onboarding | Advanced |
| [TwoFactorAuthentication](TwoFactorAuthentication/README.md) | Server-side 2FA login plugin | Advanced |

### Web services / OData

Access AppVision and AppControl data via **HTTP without the AppVision SDK**.
Useful for integrations from non-.NET platforms (Python, Java, PowerShell, etc.).

| Project | Description | Level |
|---|---|---|
| [WebServiceOData](WebServiceOData/README.md) | CRUD on AppVision options and timetables | Beginner |
| [WebServiceODataAppControl](WebServiceODataAppControl/README.md) | CRUD on AppControl people and badges | Beginner |
| [AppControlSynchro](AppControlSynchro/README.md) | Bulk import of cardholders from a CSV file | Intermediate |

---

## Key SDK Assemblies

| Assembly | Namespace | Key types |
|---|---|---|
| `appSDK.dll` | `Prysm.AppVision.SDK` | `AppServerForDriver`, `AppServer`, `Sdk.Server`, `Sdk.Client` |
| `appData.dll` | `Prysm.AppVision.Data` | `VariableRow`, `VariableState`, `AlarmInfo`, `EventRow`, `OverlayItem` |
| `appCommon.dll` | `Prysm.AppVision.Common` | `Helper`, `RColor`, `RT`, `CommStatus` |
| `appVideo.dll` | `Prysm.AppVision.Video` | `PlayerBaseControl`, `IPlayerLive`, `IPlayerPlayback`, `IPlayerPtz`, `IPlayerExport` |
| `appUI.dll` | `Prysm.AppVision.UI` | `IPersistable`, `ICommandable`, `IContainer` |
| `App.Logger.dll` | `Prysm.Logger` | `Trace`, `LogLevel`, `LogBox` |
| `App.Utility.dll` | `Prysm.Utility.Logger` | `AppServerLogAppender` |

---

## Learning Path

```
Beginner
  +- DriverMinimal          ->  understand the driver connection sequence
  \- ClientSimple           ->  understand variable subscriptions

Intermediate
  +- DriverSimple           ->  add I/O, alarms, events to a driver
  +- DriverAdvanced         ->  Device pattern + caching + structured logging
  +- ClientAdvanced         ->  Sdk.Server singleton + theme integration
  \- WpfControlSimple       ->  embed a read/write control in a synoptic

Advanced
  +- WpfControlAdvanced     ->  IPersistable, ICommandable, async data loading
  +- PlayerSimple           ->  FFmpeg live video in AppVision
  \- PlayerAdvanced         ->  full player (playback + PTZ + export)

Specialised
  +- PlayerMetadata         ->  AI object-detection overlay on live video
  +- DriverAppControl       ->  access controller hardware bridge
  +- DriverOverlay          ->  GIS overlay animation
  +- MapProvider            ->  custom tile-map background
  +- TwoFactorAuthentication ->  custom server authentication plugin
  +- WizardSample           ->  multi-page setup wizard template
  \- WebServiceOData        ->  HTTP/OData access without the SDK
```

---

## Getting Started

1. Open `AppVision-SDKSamples.sln` in Visual Studio 2019 or 2022.
2. Build once (**Ctrl+Shift+B**) to restore NuGet packages.
3. Open the `README.md` of the project you want to explore.
4. Make sure **AppVision 4.x is running** on `http://localhost`.

---

## Default AppVision credentials

| Field | Default value |
|---|---|
| Host | `http://localhost` |
| User | `ADM` |
| Password | `123` |