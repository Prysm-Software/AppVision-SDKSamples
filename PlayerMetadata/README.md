# PlayerMetadata — Video Metadata / AI Overlay Provider

## Overview

A **metadata provider** that draws YOLO-detected object bounding boxes over
a video player in AppVision. It decodes a video stream off-screen with FFmpeg
and annotates each frame on a WPF `Canvas`.

> Use this as a reference for any analytics overlay: licence plate recognition,
> heatmaps, intrusion zones, people counting, etc.

**References:** `appSDK.dll`, `appVideo.dll`, `Prysm.AppVision.FFmpeg`,
`Alturos.Yolo` (NuGet)  
**Output:** `PlayerMetadata.dll` → `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `PlayerMetadata.cs` | `IPlayerOverlay` implementation: decode → detect → draw |

---

## Key Concepts

### 1. `IPlayerOverlay`

A metadata provider is a **plain class** (not a UserControl) that implements
`IPlayerOverlay`. AppVision instantiates it alongside the video player and
passes a WPF `Canvas` for drawing.

```csharp
Task Connect(string cameraName, string address, string parameters);
Task Disconnect();
Task Play(Canvas canvas);                  // live overlay
Task Play(Canvas canvas, DateTime start);  // playback overlay
DateTime PlaybackPosition { set; }         // seek notification
```

### 2. `FFmpegVideoPlayerCtrl`

An off-screen FFmpeg decoder that provides raw frames independently of the
visible player:

```csharp
media.NewImageDecoded += ImageDecoded;  // BitmapSource per frame
media.ErrorOccured    += (e, ex) => { };
media.EnableSynchro = true;             // decode at native frame rate
media.Play(url);
```

### 3. Canvas drawing

All drawing is done on the `Canvas` passed by `Play()`:

```csharp
var rect = new Rectangle
{
    Width  = item.Width,  Height = item.Height,
    Stroke = Brushes.Red, StrokeThickness = 2
};
Canvas.SetLeft(rect, item.X);
Canvas.SetTop(rect,  item.Y);
canvas.Children.Add(rect);

var label = new TextBlock { Text = $"{item.Type} {item.Confidence:P0}" };
Canvas.SetLeft(label, item.X);
Canvas.SetTop(label,  item.Y - 16);
canvas.Children.Add(label);
```

Always dispatch drawing to the UI thread if detection runs on a background thread.

### 4. `YoloWrapper` (`Alturos.Yolo`)

```csharp
var config = new ConfigurationDetector().Detect();  // auto-detect model files
using var yolo = new YoloWrapper(config);

IEnumerable<YoloItem> items = yolo.Detect(imageBytes);
// YoloItem: Type, Confidence, X, Y, Width, Height (pixel coords)
```

### 5. Server hostname for video URL

```csharp
string url = Sdk.Server.Hostname + "Config/Param/Client/" + address;
```

This convention lets the config store a relative path that works regardless
of which server the client is connected to.

---

## AppVision SDK Reference

### `IPlayerOverlay` (`appVideo.dll`)

| Member | Description |
|---|---|
| `.Connect(name, address, param)` | Camera assigned |
| `.Disconnect()` | Camera removed |
| `.Play(canvas)` | Start live overlay |
| `.Play(canvas, start)` | Start playback overlay |
| `.PlaybackPosition` | Property: current playback position |

### `Sdk.Server` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.Hostname` | Server base URL |

### `FFmpegVideoPlayerCtrl` (`Prysm.AppVision.FFmpeg`)

| Member | Description |
|---|---|
| `.Play(url)` | Start decoding |
| `.Stop()` | Stop decoding |
| `.NewImageDecoded` | Event: new frame decoded |
| `.EnableSynchro` | Throttle to native frame rate |

---

## Configuration in AppVision

```
Configurator › Variables › Add node
    Address:  relative/path/to/video.mp4   (under Config/Param/Client/)
    Metadata: PlayerMetadata.dll#PlayerMetadata.PlayerMetadata
```

> Ensure YOLO model files (`*.cfg`, `*.weights`, `*.names`) are present
> in `AppVision 4\Bin\` next to `PlayerMetadata.dll`.

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `PlayerMetadata.dll` → `AppVision 4\Bin\`.  
3. Copy YOLO model files → `AppVision 4\Bin\`.  
4. Ensure FFmpeg native binaries are present.

---

## See Also

- [PlayerSimple](../PlayerSimple/README.md) — simple live player
- [PlayerAdvanced](../PlayerAdvanced/README.md) — full player with playback, PTZ, export
