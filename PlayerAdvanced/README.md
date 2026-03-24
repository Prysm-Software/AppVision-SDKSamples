# PlayerAdvanced â€” Full AppVision Video Player

## Overview

A full-featured WPF video player control demonstrating all four player
interfaces a production player may implement:

| Interface | Capability |
|---|---|
| `IPlayerLive` | Live streaming |
| `IPlayerPlayback` | Seek, speed control, progress events |
| `IPlayerPtz` | Pan / tilt / zoom commands |
| `IPlayerExport` | Export a clip to a local file |

The player renders a **clock face** (no real video decoding) so the framework
mechanics can be demonstrated without a camera dependency.
Every tick emits a new frame via `FireFrameDecoded` so the AppVision pipeline
(recording, overlays, motion detection) receives the simulated stream.

**References:** `appSDK.dll`, `appVideo.dll`, `appCommon.dll`  
**Output:** `PlayerClock.dll` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `PlayerClockControl.xaml.cs` | All player logic: live, playback, PTZ, export |

---

## Key Concepts

### 1. Implementing multiple interfaces

Implement only the interfaces that the real device supports.
AppVision automatically enables or disables the corresponding UI elements:

```csharp
public class PlayerClockControl : PlayerBaseControl,
    IPlayerLive, IPlayerPlayback, IPlayerPtz, IPlayerExport
```

### 2. `IPlayerPlayback`

```csharp
Task Play(DateTime start, TimeSpan duration, float speed);
Task Pause();
Task Resume();
Task Stop();
Task SeekTo(DateTime position);
float PlaybackSpeed { get; set; }  // set by AppVision
DateTime PlaybackPosition { get; } // current position
event EventHandler PlaybackEnded;  // raise when clip is finished
```

### 3. `IPlayerPtz`

```csharp
Task PtzMove(float pan, float tilt, float zoom);  // -1..+1 per axis
Task PtzStop();
Task PtzPreset(int presetId);
Task PtzHome();
```

### 4. `IPlayerExport`

```csharp
Task Export(string outputPath, DateTime start, TimeSpan duration);
event EventHandler<int> ExportProgress;  // 0..100 percent
```

### 5. `PlaybackSpeed`

AppVision sets `PlaybackSpeed` before calling `Play()`.
Use a `DispatcherTimer` interval inversely proportional to the speed to
simulate correct timing:

```csharp
_timer.Interval = TimeSpan.FromMilliseconds(40 / PlaybackSpeed);
```

### 6. `DebugWriteLine()`

Extension method (`Prysm.AppVision.Common`) that writes the caller's method
name to the debug output â€” useful for tracing lifecycle calls:

```csharp
this.DebugWriteLine("connecting to " + address);
```

---

## AppVision SDK Reference

### `PlayerBaseControl` (`appVideo.dll`)

| Member | Description |
|---|---|
| `.FireFrameDecoded(date, bmp)` | Forward frame to AppVision pipeline |
| `.SetError(message)` | Report an error |
| `.Mode` | Current mode: Live / Playback / Idle |
| `.Status` | Current status string |

### `IPlayerLive` / `IPlayerPlayback` / `IPlayerPtz` / `IPlayerExport`

See [AppVision video SDK documentation] for the full interface contracts.

---

## Configuration in AppVision

```
Configurator â€º Variables â€º Add node
    Player: PlayerClock.dll#PlayerClock.PlayerClockControl
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `PlayerClock.dll` â†’ `AppVision 4\Bin\`.

---

## Debugging

Use `DebugWriteLine()` in each override to trace lifecycle calls.  
`tbBottom` in the XAML displays **Mode + Status** at runtime.

---

## Learning Path

```
PlayerSimple  â†’  PlayerAdvanced  â†’  PlayerMetadata
Live only         Full interface set   Metadata overlay (YOLO)
```
