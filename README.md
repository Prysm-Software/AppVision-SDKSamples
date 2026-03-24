# AppVision SDK Samples

A collection of C# samples that demonstrate how to integrate third-party systems,
build custom client applications, embed WPF controls, stream video, and automate
configuration using the **AppVision 4 SDK**.

---

## Audience

These samples target **C# developers** who want to build extensions for AppVision.  
No prior AppVision development experience is required, but the following background
is assumed:

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
| **.NET Framework** | 4.8 SDK |
| **Repositories** | `AppVision-APP` and `AppVision-SDK` must be cloned **in the same parent folder** |

### Folder layout expected by the build

```
parent-folder/
â”œâ”€â”€ AppVision-APP/          â† AppVision application binaries & config
â”œâ”€â”€ AppVision-SDK/          â† SDK assemblies referenced by all samples
â””â”€â”€ AppVision-SDKSamples/   â† this repository
```

---

## Repository Structure

```
AppVision-SDKSamples/
â”‚
â”œâ”€â”€ DriverMinimal/              Console driver â€” minimal connection sequence
â”œâ”€â”€ DriverSimple/               WPF driver â€” 8 I/O, alarms, custom events
â”œâ”€â”€ DriverAdvanced/             WPF driver â€” Device pattern, caching, logging
â”œâ”€â”€ DriverAppControl/           WPF driver â€” AppControl hardware bridge
â”œâ”€â”€ DriverOverlay/              WPF driver â€” animated geo-referenced overlays
â”‚
â”œâ”€â”€ ClientSimple/               WPF client â€” subscribe to variables/alarms/events
â”œâ”€â”€ ClientAdvanced/             WPF client â€” Sdk.Server singleton, theme, options
â”‚
â”œâ”€â”€ WpfControlSimple/           Embeddable control â€” OperandManager read/write
â”œâ”€â”€ WpfControlAdvanced/         Embeddable control â€” IPersistable, ICommandable
â”‚
â”œâ”€â”€ PlayerSimple/               Video player â€” IPlayerLive, FFmpeg software decode
â”œâ”€â”€ PlayerAdvanced/             Video player â€” IPlayerPlayback, PTZ, Export
â”œâ”€â”€ PlayerMetadata/             Metadata overlay â€” YOLO object detection on live video
â”‚
â”œâ”€â”€ MapProvider/                GIS map provider â€” Bing Maps tile background
â”œâ”€â”€ OverlaySample/              Standalone tool â€” overlay CRUD on a GIS synoptic
â”‚
â”œâ”€â”€ WizardSample/               Setup wizard â€” multi-page device onboarding
â”œâ”€â”€ TwoFactorAuthentication/    Server plugin â€” custom 2-factor login provider
â”‚
â”œâ”€â”€ AppControlSynchro/          CLI tool â€” CSV bulk import of people into AppControl
â”œâ”€â”€ WebServiceOData/            CLI tool â€” AppVision config OData CRUD (no SDK)
â””â”€â”€ WebServiceODataAppControl/  CLI tool â€” AppControl people/badges OData CRUD
```

---

## Samples by Category

### Driver development

Write a **driver** when you need to push values from an external device into
AppVision, or when AppVision needs to send commands to the device.
Drivers are standalone processes started and supervised by AppVision.

| Project | Description | Difficulty |
|---|---|---|
| [DriverMinimal](DriverMinimal/README.md) | Bare-minimum console driver | â­ |
| [DriverSimple](DriverSimple/README.md) | WPF driver with 8 digital I/Os, alarm, event | â­â­ |
| [DriverAdvanced](DriverAdvanced/README.md) | Full driver: Device pattern, caching, logging | â­â­â­ |
| [DriverAppControl](DriverAppControl/README.md) | Bridge to AppControl access controller hardware | â­â­â­ |
| [DriverOverlay](DriverOverlay/README.md) | Animate geo-referenced icons on a GIS synoptic | â­â­ |

### Client applications

Build a **standalone application** that reads or controls AppVision data.

| Project | Description | Difficulty |
|---|---|---|
| [ClientSimple](ClientSimple/README.md) | Subscribe to variables, alarms, events; set values | â­ |
| [ClientAdvanced](ClientAdvanced/README.md) | `Sdk.Server`, colour theme, `GetOption`, clean disconnect | â­â­ |

### Embeddable WPF controls

Create a custom **UserControl** that operators can place directly on an
AppVision synoptic, alongside the built-in widgets.

| Project | Description | Difficulty |
|---|---|---|
| [WpfControlSimple](WpfControlSimple/README.md) | Read/write variables from inside a synoptic | â­â­ |
| [WpfControlAdvanced](WpfControlAdvanced/README.md) | `IPersistable`, `ICommandable`, `IContainer`, async data | â­â­â­ |

### Video players

Build a custom **video player control** that AppVision manages (camera assignment,
recording, motion detection, metadata overlay).

| Project | Description | Difficulty |
|---|---|---|
| [PlayerSimple](PlayerSimple/README.md) | Live stream with FFmpeg software decoder | â­â­ |
| [PlayerAdvanced](PlayerAdvanced/README.md) | Full player: live, playback, PTZ, export | â­â­â­ |
| [PlayerMetadata](PlayerMetadata/README.md) | Draw YOLO bounding-box overlays on live video | â­â­â­ |

### GIS and overlays

| Project | Description | Difficulty |
|---|---|---|
| [MapProvider](MapProvider/README.md) | Custom Bing Maps tile background for GIS synoptics | â­â­ |
| [OverlaySample](OverlaySample/README.md) | Standalone CRUD tool for geo-referenced overlay icons | â­â­ |

### Configuration and server plugins

| Project | Description | Difficulty |
|---|---|---|
| [WizardSample](WizardSample/README.md) | Multi-page WPF wizard for device onboarding | â­â­â­ |
| [TwoFactorAuthentication](TwoFactorAuthentication/README.md) | Server-side 2FA login plugin | â­â­â­ |

### Web services / OData

Access AppVision and AppControl data via **HTTP without the AppVision SDK**.
Useful for integrations from non-.NET platforms (Python, Java, PowerShell, etc.).

| Project | Description | Difficulty |
|---|---|---|
| [WebServiceOData](WebServiceOData/README.md) | CRUD on AppVision options and timetables | â­ |
| [WebServiceODataAppControl](WebServiceODataAppControl/README.md) | CRUD on AppControl people and badges | â­ |
| [AppControlSynchro](AppControlSynchro/README.md) | Bulk import of cardholders from a CSV file | â­â­ |

---

## Key SDK Assemblies

| Assembly | Namespace | Key types |
|---|---|---|
| `appSDK.dll` | `Prysm.AppVision.SDK` | `AppServerForDriver`, `AppServer`, `Sdk.Server`, `Sdk.Client` |
| `appData.dll` | `Prysm.AppVision.Data` | `VariableRow`, `VariableState`, `AlarmInfo`, `EventRow`, `OverlayItem` |
| `appCommon.dll` | `Prysm.AppVision.Common` | `Helper`, `RColor`, `RT`, `CommStatus` |
| `appVideo.dll` | `Prysm.AppVision.Video` | `PlayerBaseControl`, `IPlayerLive`, `IPlayerPlayback`, `IPlayerPtz`, `IPlayerExport` |
| `appUI.dll` | `Prysm.AppVision.UI` | `IPersistable`, `ICommandable`, `IContainer` |
| `appControl.dll` | `Prysm.AppControl.SDK` | `ChannelService` |
| `appServer.dll` | `Prysm.AppVision.Server` | `AppUserLogin` |
| `App.Logger.dll` | `Prysm.Logger` | `Trace`, `LogLevel`, `LogBox` |
| `App.Utility.dll` | `Prysm.Utility.Logger` | `AppServerLogAppender` |

---

## Learning Path

```
Beginner
  â”œâ”€ DriverMinimal          â†’ understand the driver connection sequence
  â””â”€ ClientSimple           â†’ understand variable subscriptions

Intermediate
  â”œâ”€ DriverSimple           â†’ add I/O, alarms, events to a driver
  â”œâ”€ DriverAdvanced         â†’ Device pattern + caching + structured logging
  â”œâ”€ ClientAdvanced         â†’ Sdk.Server singleton + theme integration
  â””â”€ WpfControlSimple       â†’ embed a read/write control in a synoptic

Advanced
  â”œâ”€ WpfControlAdvanced     â†’ IPersistable, ICommandable, async data loading
  â”œâ”€ PlayerSimple           â†’ FFmpeg live video in AppVision
  â””â”€ PlayerAdvanced         â†’ full player (playback + PTZ + export)

Specialised
  â”œâ”€ PlayerMetadata         â†’ AI object-detection overlay on live video
  â”œâ”€ DriverAppControl       â†’ access controller hardware bridge
  â”œâ”€ DriverOverlay          â†’ GIS overlay animation
  â”œâ”€ MapProvider            â†’ custom tile-map background
  â”œâ”€ TwoFactorAuthentication â†’ custom server authentication plugin
  â”œâ”€ WizardSample           â†’ multi-page setup wizard template
  â””â”€ WebServiceOData*       â†’ HTTP/OData access without the SDK
```

---

## Getting Started

```bash
# 1. Clone the three related repositories into the same folder
git clone https://dev.azure.com/PrysmSoftware/Public/_git/AppVision-APP
git clone https://dev.azure.com/PrysmSoftware/Public/_git/AppVision-SDK
git clone https://dev.azure.com/PrysmSoftware/Public/_git/AppVision-SDKSamples

# 2. Open the solution
start AppVision-SDKSamples\AppVision-SDKSamples.sln
```

3. **Build once** (Ctrl+Shift+B) to restore NuGet packages.  
4. Open the `README.md` of the project you want to explore.  
5. Make sure **AppVision 4.x is running** on `http://localhost`.

---

## Default AppVision credentials

| Field | Default value |
|---|---|
| Host | `http://localhost` |
| User | `ADM` |
| Password | `123` |
