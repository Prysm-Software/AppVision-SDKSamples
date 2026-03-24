# PlayerSimple â€” Simple AppVision Video Player

## Overview

A minimal WPF video player control for AppVision that streams **live video**
using FFmpeg.

The control inherits from `PlayerBaseControl` and implements only `IPlayerLive`
(live streaming only â€” no playback, PTZ, or export).
See [PlayerAdvanced](../PlayerAdvanced/README.md) for a full-featured player.

**References:** `appSDK.dll`, `appVideo.dll`, `appCommon.dll`, `Prysm.FFmpeg.Next`  
**Output:** `PlayerSimple.dll` â†’ `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `PlayerSimple.xaml.cs` | Connect, Play, Stop, frame callback |

---

## Key Concepts

### 1. `PlayerBaseControl`

All AppVision video player controls **must inherit** `PlayerBaseControl`.  
The base class handles camera assignment, status reporting, `StretchVideo`, etc.
Override the lifecycle methods:

```csharp
public override async Task Connect(string cameraName, string address, string parameters)
public override async Task Play()
public override Task Stop()
public override Task Disconnect() => Stop();
```

### 2. `IPlayerLive`

Implement this interface to advertise live streaming support.  
AppVision enables / disables the camera widget controls accordingly.

### 3. `AVPlayerUri` and `PlayerOptions`

```csharp
await player.PlayAsync(
    new AVPlayerUri { Uri = url },
    new PlayerOptions
    {
        UseHardwareDecoding = false,
        Rendering = RenderingMethod.Software,
    });
```

Set `UseHardwareDecoding = true` for GPU-accelerated HEVC / H.264.

### 4. `SoftwareVideoFrame` and `FireFrameDecoded`

Every decoded frame fires `player.FrameRendered`:

```csharp
private void ImageDecoded(object sender, FrameEventArgs e)
{
    // e.Frame is null when Rendering != Software
    var bmp = (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource;
    FireFrameDecoded(e.Timestamp.Date, bmp);
}
```

`FireFrameDecoded` forwards the frame to AppVision (recording, metadata overlay,
motion detection, etc.).

### 5. `SetError(message)`

Reports an error to AppVision so the client can display it and trigger
automatic reconnection if configured:

```csharp
player.ErrorOccured += (err, ex) => SetError(err);
```

---

## AppVision SDK Reference

### `PlayerBaseControl` (`appVideo.dll`)

| Member | Description |
|---|---|
| `.Connect(name, address, param)` | Override: camera assigned |
| `.Disconnect()` | Override: camera removed |
| `.Play()` | Override: start live stream |
| `.Stop()` | Override: stop stream |
| `.FireFrameDecoded(date, bmp)` | Forward decoded frame to AppVision |
| `.SetError(message)` | Report an error |
| `.StretchVideo` | Fill vs uniform stretch |

### `AVPlayerUri` and `PlayerOptions` (`Prysm.FFmpeg.Next`)

| Member | Description |
|---|---|
| `AVPlayerUri.Uri` | Stream URL (RTSP, HTTP, file path, etc.) |
| `PlayerOptions.UseHardwareDecoding` | Enable GPU decoding |
| `PlayerOptions.Rendering` | `RenderingMethod.Software` or `Hardware` |

---

## Configuration in AppVision

```
Configurator â€º Variables â€º Add node
    Name:    MyCam
    Source:  rtsp://camera-ip/stream
    Player:  PlayerSimple.dll#PlayerSimple.PlayerSimple
```

Then add a **Camera** object on a synoptic and assign `MyCam` to it.

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `PlayerSimple.dll` â†’ `AppVision 4\Bin\`.  
3. Ensure FFmpeg native binaries (`avcodec-*.dll`, etc.) are present in `Bin\`.

---

## Debugging

Attach to the AppVision client process.  
Test `Play()` independently by assigning a local file path as the URL.

---

## Learning Path

```
PlayerSimple  â†’  PlayerAdvanced  â†’  PlayerMetadata
Live only         Playback+PTZ+Export  Overlay / YOLO metadata
```
