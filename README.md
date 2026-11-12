# AppVision SDK Samples

Samples that show partners how to integrate third-party systems with **AppVision 4**, build custom client
applications, embed WPF controls and video players, drive the operator screens, and reach AppVision from other
platforms over HTTP or with the JavaScript SDK.

---

## Audience

Partner developers who already configure AppVision sites (protocols, variables, synoptics) and want to extend them.

| Topic | Level required |
|---|---|
| C# | Intermediate (for the .NET samples) |
| WPF | Basic (for the UI samples) |
| .NET Framework 4.8 | Basic |
| JavaScript / npm | Basic (for NodeClient, WebCustomClient, WebServiceREST) |
| HTTP, REST, OData | Basic (for the web service samples) |

---

## Documentation

These samples go with the API help, which every server serves from its installation (`Bin\Web`), in the version
and under the name of the installed product. Replace `<server>` by the name or address of your server (`localhost`
on the development machine), and `http` by `https` if the server only accepts HTTPS:

| Help | URL |
|---|---|
| .NET API (`appSDK`, `appData`, `appCommon`, `appVideo`, scripts...) | `http://<server>/doccsharp/index.html` |
| JavaScript SDK (`@prysm/appsdk`) | `http://<server>/docjs/index.html` |

The .NET help opens with concept pages, then the reference of each namespace. Start with the page that matches what
you build; each sample README also names its page. The page addresses below are relative to your server
(`http://<server>/doccsharp/html/<id>.htm`):

| You want to... | Concept page | `<id>` | Samples |
|---|---|---|---|
| Connect a program, read and write values | Getting started | `4d97183d-f664-4102-9eb7-e35df2a99dd8` | ClientSimple, ClientAdvanced |
| Choose what the server notifies, and on which thread | Notifications and filters | `72f3b891-cdf6-48e2-b5f1-015f8e023a0d` | ClientSimple, DriverMinimal |
| Read or write `$V.`, `$A.`, `$G.` operands and expressions | Operands and expressions | `17d6b229-9778-43f0-bfca-ef755524e4a8` | WpfControlSimple |
| Raise, acknowledge, report and close alarms | Alarm model | `6e5e199a-5dd5-4048-84ad-5975fe79add9` | ClientAlarms |
| Write a driver or its configuration wizard | Writing a driver | `fa685819-4ff4-4c40-81ba-4172e43de86b` | DriverMinimal, DriverSimple, DriverAdvanced, DriverOverlay, WizardSample |
| Write a video player | Video players | `241bd2a4-bd41-4dfa-aa02-ecbd33f83103` | PlayerSimple, PlayerAdvanced, PlayerMetadata |
| Write server scripts | Server scripts | `b274fa62-82c7-4e21-ab7b-099b658076e7` | OverlaySample (configuration) |
| Write client scripts, open documents on the operator screens | Client scripts and documents | `fba94a4b-ad72-4807-be00-2d747b51da85` | RemoteControl, WpfControlAdvanced |
| Put controls, widgets, maps or overlays on synoptics | Synoptics and symbols | `0d5e00d8-27bb-4b55-a9b4-bfcf867794da` | WpfControlSimple, WpfControlAdvanced, WidgetLibrary, MapProvider, OverlaySample |
| Manage people, badges and rights, or download them to controllers | Access control | `04bcc881-e1bf-4e10-bb05-ea4adf6f91ea` | DriverAppControl, WebServiceODataAppControl, AppControlSynchro |
| Reach the server over REST, OData or with the JavaScript SDK | Web services and JavaScript SDK | `11442ac1-c309-4c8f-8837-050c0cf5fe13` | WebServiceREST, WebServiceOData, NodeClient, WebCustomClient |
| Find the sample for a topic | SDK samples | `1f39c0dc-edd7-4d7c-9dd1-ddc315b457df` | all |

The help describes the version installed on that server; this repository follows the latest version (see
*Prerequisites* for the samples that need more than 4.10.0).

---

## Prerequisites

| Requirement | Details |
|---|---|
| **AppVision** | 4.10 or later, installed on the development machine (the .NET samples reference its DLLs) and running on `localhost` or a reachable host. Exceptions below. |
| **Visual Studio** | 2022 version 17.8 or later, with the *.NET desktop development* workload: [`Directory.Build.props`](Directory.Build.props) sets `LangVersion` to `latest` and some samples use C# 12 (primary constructors, collection expressions), which Visual Studio 2019 does not compile |
| **.NET Framework** | 4.8 |
| **Node.js** | 18 or later, for the JavaScript samples |

Samples that need more than AppVision 4.10.0:

| Sample | Requires |
|---|---|
| [WidgetLibrary](WidgetLibrary/README.md) | AppVision 4.10.2 (`Prysm.AppVision.SDK.Widgets`) |
| [PasskeyAuthentication](PasskeyAuthentication/README.md) | AppVision 4.10.2 (`IAuthClientInfo`, `IAuthProbe`, `IAuthAccountPlugin`, `AuthenticationMethodInfo`) |
| [NodeClient](NodeClient/README.md), [WebCustomClient](WebCustomClient/README.md) | The JavaScript SDK 4.9.79 and later needs an AppVision 4.10.2 server; the 4.9.76 package pinned by the samples works with 4.10.0 |

### Build

Open `SDK Samples.sln` and build. The AppVision assemblies (`appSDK.dll`, `appData.dll`...) are found in the AppVision
`Bin` folder through the `ReferencePath` of [`Directory.Build.props`](Directory.Build.props):
`C:\Program Files\AppVision 4\Bin\`. **Edit this file** if AppVision is installed elsewhere.

Each project builds into its own `bin\Debug\` or `bin\Release\` folder:

- **programs** (drivers, client applications, tools) get the AppVision DLLs copied next to them and run from there;
- **DLLs loaded by AppVision** (video players, WPF controls, map provider, authentication plugin) are built alone
  (*Copy Local = False*): copy the DLL into the AppVision `Bin` folder, as each README explains;
- [DriverAdvanced](DriverAdvanced/README.md) builds into `..\..\..\Bin\Drivers\DriverAdvanced\`, the layout of a
  delivered driver: this is the AppVision `Bin` folder only when the repository is cloned two levels below the
  installation folder (e.g. `<installation>\Public\AppVision-SDKSamples`). Elsewhere, change its *Output path* or
  copy the output folder into `Bin\Drivers\`.

### Sample configurations

Several samples ship a `Configuration\` folder: a ready-made AppVision configuration (`Param`, `Histo` databases,
synoptics, scripts) that declares the protocol, variables and synoptics the sample expects.

1. Copy the folder **out of the repository**, e.g. to `AppVision 4\Configurations\SDK_DriverSimple` (the server
   upgrades and writes the databases: keep the repository copy intact).
2. In the server window, click `...` next to *Configuration* and select that folder (or hold **Shift** while the
   server starts). The server restarts on it.
3. In the configurator, **generate** the client configuration.

Use a test server: switching the configuration disconnects every client.

---

## Samples

### Driver development

Write a **driver** to push the values of an external device into AppVision and to send it the commands of AppVision.
Drivers are standalone processes started and supervised by the AppVision server (one per protocol).

| Project | Description | Level |
|---|---|---|
| [DriverMinimal](DriverMinimal/README.md) | Bare-minimum console driver: connection sequence, subscriptions, clean stop | Beginner |
| [DriverSimple](DriverSimple/README.md) | WPF driver with 8 digital inputs/outputs, a buffer, a custom alarm and event | Beginner |
| [DriverAdvanced](DriverAdvanced/README.md) | Production structure: Driver/Device classes, cache, logging, `Bin\Drivers\<name>` layout | Advanced |
| [DriverAppControl](DriverAppControl/README.md) | Access control: download of people, badges, profiles and timetables to controllers | Advanced |
| [DriverOverlay](DriverOverlay/README.md) | Animate geo-referenced icons (overlays) on a GIS synoptic | Intermediate |

### Client applications

Build a **standalone application** that reads AppVision data, acts on it or drives the operators.

| Project | Description | Level |
|---|---|---|
| [ClientSimple](ClientSimple/README.md) | Subscribe to variables, alarms and events; write a value | Beginner |
| [ClientAlarms](ClientAlarms/README.md) | Alarm life cycle (acknowledge, report, close, raise) and history queries, console | Intermediate |
| [RemoteControl](RemoteControl/README.md) | Drive the operator screens: load a synoptic, a camera or a view on a workstation; show each new alarm | Intermediate |
| [ClientAdvanced](ClientAdvanced/README.md) | `Sdk.Server`, AppVision controls (areas, alarms, video container), theme, options | Intermediate |

### Embeddable WPF controls

Create a **WPF control** that operators place on a synoptic or load in a container, next to the built-in objects.

| Project | Description | Level |
|---|---|---|
| [WpfControlSimple](WpfControlSimple/README.md) | Read and write variables from a control placed in a synoptic | Intermediate |
| [WidgetLibrary](WidgetLibrary/README.md) | Widget DLL listed in the synoptic toolbox (`[AppWidget]`), live variables (AppVision 4.10.2) | Intermediate |
| [WpfControlAdvanced](WpfControlAdvanced/README.md) | `IPersistable`, `ICommandable` (container menu), alarms, async data | Advanced |

### Video players

Build a **video player** that AppVision uses for the cameras of a video system (live, playback, PTZ, export).

| Project | Description | Level |
|---|---|---|
| [PlayerSimple](PlayerSimple/README.md) | Live stream (file, RTSP, desktop) with the FFmpeg decoder | Intermediate |
| [PlayerAdvanced](PlayerAdvanced/README.md) | Complete player: live, playback, PTZ, export (simulated) | Advanced |
| [PlayerMetadata](PlayerMetadata/README.md) | Metadata overlay: object detection boxes drawn over the video | Advanced |

### GIS and overlays

| Project | Description | Level |
|---|---|---|
| [MapProvider](MapProvider/README.md) | Custom map background for GIS synoptics: any XYZ tile server, offline cache | Intermediate |
| [OverlaySample](OverlaySample/README.md) | Create, edit and delete overlays (icons placed by programs on synoptics) | Intermediate |

### Configuration and server plugins

| Project | Description | Level |
|---|---|---|
| [WizardSample](WizardSample/README.md) | Multi-page wizard that discovers devices and creates protocol and variables | Advanced |
| [PasskeyAuthentication](PasskeyAuthentication/README.md) | Authentication plugin, server and client: sign in with a passkey (WebAuthn, Windows Hello, security key) instead of a password | Advanced |

### JavaScript, web services and databases

Reach AppVision **without the .NET SDK**: JavaScript SDK, REST, OData, SQL.

| Project | Description | Level |
|---|---|---|
| [NodeClient](NodeClient/README.md) | JavaScript SDK (`@prysm/appsdk`) from Node.js: live values, alarms, commands, history | Beginner |
| [WebCustomClient](WebCustomClient/README.md) | React page using the JavaScript SDK, hosted by the AppVision web client | Intermediate |
| [WebServiceREST](WebServiceREST/README.md) | Plain HTML/JavaScript page calling the REST web service (login, notifications, read/write) | Beginner |
| [WebServiceOData](WebServiceOData/README.md) | OData CRUD on the configuration: options, timetable variables | Beginner |
| [WebServiceODataAppControl](WebServiceODataAppControl/README.md) | OData CRUD on AppControl people and badges | Beginner |
| [AppControlSynchro](AppControlSynchro/README.md) | Import of cardholders from a CSV file into AppControl | Intermediate |
| [DatabaseSchema](DatabaseSchema/README.md) | Database schemas (SQL scripts, diagrams) and a history query in PowerShell | Reference |

---

## Key SDK assemblies

| Assembly | Namespace | Key types |
|---|---|---|
| `appSDK.dll` | `Prysm.AppVision.SDK` | `AppServer`, `AppServerForDriver`, `Sdk.Server`, `Sdk.Client`, managers (`VariableManager`, `AlarmManager`, `EventManager`, `ClientManager`...), `IPersistable`, `ICommandable` |
| `appData.dll` | `Prysm.AppVision.Data` | `VariableRow`, `VariableState`, `AlarmRow`, `AlarmInfo`, `EventRow`, `OverlayItem`, `CommStatus` |
| `appCommon.dll` | `Prysm.AppVision.Common` | `Helper`, `RT` (translations), extension methods |
| `appUI.dll` | `Prysm.AppVision.UI` | AppVision WPF controls, `RColor` (theme) |
| `appVideo.dll` | `Prysm.AppVideo` | `PlayerBaseControl`, `IPlayerLive`, `IPlayerPlayback`, `IPlayerPtz`, `IPlayerExport`, `IPlayerOverlay` |
| `App.Logger.dll`, `App.Utility.dll` | `Prysm.Logger`, `Prysm.Utility` | logging (`Trace`, `LoggerBox`, `AppServerLogAppender`) |

`App.Logger` and `App.Utility` are not described in the API help: [DriverAdvanced](DriverAdvanced/README.md) shows
how to use them.

JavaScript: `@prysm/appsdk`, served by the MCP server extension of the AppVision server
(`http://<server>/mcp/sdk`), see [NodeClient](NodeClient/README.md).

---

## Learning path

```
Beginner
  +- DriverMinimal          ->  the driver connection sequence
  +- ClientSimple           ->  variable, alarm and event subscriptions
  \- NodeClient             ->  the same from JavaScript

Intermediate
  +- DriverSimple           ->  inputs, outputs, alarms, events in a driver
  +- ClientAlarms           ->  alarm life cycle, history
  +- RemoteControl          ->  drive the operators' screens
  +- WpfControlSimple       ->  a control in a synoptic
  \- WidgetLibrary          ->  controls in the synoptic toolbox

Advanced
  +- DriverAdvanced         ->  structure and delivery of a real driver
  +- WpfControlAdvanced     ->  IPersistable, ICommandable
  +- PlayerSimple           ->  live video in AppVision
  \- PlayerAdvanced         ->  playback, PTZ, export

Specialised
  +- PlayerMetadata         ->  metadata overlay on live video
  +- DriverAppControl       ->  access control download
  +- DriverOverlay, OverlaySample  ->  overlays on synoptics
  +- MapProvider            ->  GIS map background
  +- PasskeyAuthentication  ->  authentication plugin (passkeys, server + sign-in screen)
  +- WizardSample           ->  device discovery and configuration import
  \- WebServiceREST, WebServiceOData, WebCustomClient  ->  HTTP access without the .NET SDK
```

---

## Default AppVision credentials

| Field | Default value |
|---|---|
| Host | `http://localhost` |
| User | `ADM` |
| Password | `123` |

Use them on a test server only: change the password of `ADM` on a real site.
