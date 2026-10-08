# PlayerSimple — A minimal live video player

**Used for:** displaying in AppVision the live video of a camera that AppVision does not support out of the box,
when the camera (or its video system) provides a stream that FFmpeg can read: RTSP, HTTP/MJPEG, HLS, a video file...
This sample is the smallest possible player: live only, no playback, PTZ or export.
See [PlayerAdvanced](../PlayerAdvanced/README.md) for the complete set of features.

**Audience:** partner developers who already configure AppVision sites (variables, synoptics) and know C# and WPF.

![The camera PlayerSimple.Video of the sample configuration, a video file played by PlayerSimple.dll in the AppVision client](preview.png)

---

## How it works

1. A **player** is a WPF control that derives from `PlayerBaseControl` (`appVideo.dll`) and implements one or more
   player interfaces. `IPlayerLive` = live video.
2. The integrator declares the player on the **camera variable** (type *Camera*/*Video*), or on a parent node, with
   the parameter `CreateControlPlayer=PlayerSimple.dll`.
3. When an operator displays the camera, the AppVision client creates the control, places it in the camera tile and
   calls `Connect(cameraName, address, parameters)` with the *Address* and *Parameters* fields of the variable, then
   `Play()`. When the tile is closed or shows another camera: `Stop()` and `Disconnect()`.
4. The control decodes the stream itself. Here it hosts the AppVision FFmpeg player (`Prysm.FFmpeg.Next.Player`,
   `App.FFmpeg.dll`) and calls `FireFrameDecoded` for each frame, so that AppVision knows that images arrive.

| Project file | Role |
|---|---|
| `PlayerSimple.xaml` | The control: a `PlayerBaseControl` that contains an FFmpeg `Player` |
| `PlayerSimple.xaml.cs` | `Connect`, `Play`, `Stop`, `Disconnect`, `Snapshot`, `StretchVideo`, frame callback |
| `cctv.mp4` | Demo video, copied next to the DLL |
| `Configuration/` | Ready-made AppVision configuration with cameras declared (see below) |

**References:** `appVideo.dll`, `appSDK.dll`, `appCommon.dll`, `App.FFmpeg.dll` (AppVision `Bin` folder).
**Output:** `PlayerSimple.dll`.

---

## Step 1 — Write the player

Derive from `PlayerBaseControl`, implement `IPlayerLive`, and override the methods you support. `PlayerBaseControl`
already implements every member of the player interfaces with a default ("not available") behaviour.

```csharp
public partial class PlayerSimple : PlayerBaseControl, IPlayerLive
{
	string url;

	public override Task Connect(string cameraName, string address, string parameters)
	{
		url = address?.Trim();      // Address field of the camera variable, read like an FFmpeg command line
		return Task.CompletedTask;
	}

	public override Task Play() => player.PlayAsync(
		new AVPlayerUri { Uri = url },
		new PlayerOptions
		{
			Rendering = RenderingMethod.Software,   // pure WPF rendering
			UseHardwareDecoding = false,
			LoopOnEndOfFile = true,                 // a file plays in a loop, like a camera
			AutoReconnect = true,                   // stream lost: retry after 2 s, then up to every 30 s
		});

	public override Task Stop() { player.Stop(); return Task.CompletedTask; }
	public override Task Disconnect() => Stop();
}
```

**Tell AppVision that images arrive.** Call `FireFrameDecoded(timestamp, bitmap)` for each frame: the tile status
goes from *Connected* to *Playing* (the "Connecting" message disappears), and in playback the timestamp moves the
timeline.

```csharp
player.FrameRendered += (s, e) =>
	FireFrameDecoded(e.Timestamp.Date, (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource);
```

**Report errors** with `SetError(message, seconds)`: AppVision displays the message in red in the tile.
The FFmpeg player raises `ErrorOccured` on a decoding thread; `SetError` returns to the UI thread itself.

```csharp
player.ErrorOccured += (message, ex) => _ = SetError(message, 5);
```

**Optional members worth implementing**, even for a simple player:

| Member | Without it |
|---|---|
| `StretchVideo` (property) | The option *App.Client.Video.StretchVideo* (fill or keep the aspect ratio) is ignored. |
| `Snapshot()` | The snapshot command of the tile takes a screen capture of the tile instead of the decoded image. |
| `GetStreams()` / `Play(string stream)` | No stream choice (`@high`, `@low`, or names of your own) in the tile menu. |
| `CanAudio` / `Audio` | No audio button. |

Rendering: `RenderingMethod.Software` keeps a pure WPF control, so that AppVision can draw its overlays and apply the
digital zoom over the video. `RenderingMethod.D3D11_VP` with `UseHardwareDecoding = true` decodes and renders on the
GPU (better for many HD streams), but `FrameRendered` then carries a Direct3D frame, not a WPF bitmap.

---

## Step 2 — Build and deploy

1. Build in **Release** (.NET Framework 4.8, AnyCPU). The DLL is written to `bin\Release\` in the project folder
   (`OutputPath`), without the AppVision DLLs (*Copy Local = False*).
2. Copy `PlayerSimple.dll` (and `cctv.mp4` for the demo) into the AppVision `Bin` folder
   (`C:\Program Files\AppVision 4\Bin`) **of every client machine**: the player runs in the client, not in the server.
   Do not copy the AppVision DLLs that the build places next to it.
3. Close and restart the clients after replacing the DLL: a loaded DLL cannot be replaced.

FFmpeg itself is part of AppVision (`App.FFmpeg.dll` and the `FFmpeg` subfolder of `Bin`): nothing else to install.

---

## Step 3 — Declare a camera in the configurator

In the configurator, **Variables**, create a variable of type **Camera** (video), for example `Building.Camera1`:

| Field | Value |
|---|---|
| Address | Stream address, e.g. `rtsp://user:password@192.168.1.10/stream1`, or the full path of a file. It is read like an FFmpeg command line: put a path containing spaces **in quotes**, `"C:\Program Files\AppVision 4\Bin\cctv.mp4"`; FFmpeg options are accepted, e.g. `-f gdigrab -i desktop`. |
| Parameters | `CreateControlPlayer=PlayerSimple.dll` |

- `CreateControlPlayer` is a parameter like any other: set it on a parent node (the video system node) to apply it to
  all its cameras. The *Address* is also inherited from the parent when the camera has none.
- `PlayerSimple.dll` creates the first player type of the DLL; `PlayerSimple.dll#PlayerSimple.PlayerSimple` names the
  class explicitly (needed when a DLL contains several players).
- Save, then display the camera in the client: from the camera list, from a *Video* object of a synoptic, or by
  script with `Client.Load(0, "main", "$V.Building.Camera1")`.

See *docs/Video system integration/Video System Integration Specifications.pdf* for the variable tree that a video
system integration is expected to follow (`Cameras.Cam001.Video`, `.Comm`, `.RecStatus`...).

---

## Try it with the sample configuration

The `Configuration` folder is a complete AppVision configuration (databases in `Param` and `Histo`, synoptics in
`Param\Client`, server scripts in `Param\Server`), saved with AppVision 4.7 and upgraded when the server opens it.
It is shared by the video samples and declares these cameras:

| Variable | Address | Parameters |
|---|---|---|
| `PlayerSimple.Video` | full path of `cctv.mp4`, in quotes | `CreateControlPlayer=PlayerSimple.dll` |
| `PlayerSimple.LiveCam` | an HTTP video on the Internet | `CreateControlPlayer=PlayerSimple.dll` |
| `PlayerSimple.Desktop` | `-f gdigrab -i desktop` (capture of the Windows desktop by FFmpeg) | `CreateControlPlayer=PlayerSimple.dll` |
| `PlayerAdvanced.VMS` | `user:pwd@10.11.12.13` | `CreateControlPlayer=PlayerClock.dll` (see PlayerAdvanced) |
| `PlayerMetadata.Analytics` | `PlayerMetadata\intrusion.avi` | see PlayerMetadata |

1. Build the sample and copy `PlayerSimple.dll` and `cctv.mp4` into the AppVision `Bin` folder.
2. Copy the `Configuration` folder **out of the repository**, for example to
   `C:\Program Files\AppVision 4\Configurations\SDK_PlayerSimple`: the server writes into its databases.
3. Open it on the server: right-click the AppVision server icon in the notification area, *Change configuration*,
   and select the copied folder (the one that contains `Param` and `Histo`). The server restarts on it.
4. Open the configurator and **generate** the client configuration: the folder is delivered without the generated
   client (`GenBin`). Check the *Address* of `PlayerSimple.Video`: it must be the full path of `cctv.mp4` on the
   client machine, in quotes (the configuration was saved with `"C:\Program Files\AppVision 4\Bin\cctv.mp4"`).
5. Start the client. A warning icon on the home page means that `PlayerSimple.dll` is not in the `Bin` folder.
   Open a camera from the camera list on the left.

---

## Troubleshooting

| Symptom | Cause |
|---|---|
| *Missing parameter CREATECONTROLPLAYER* in the tile | The parameter is not set on the camera variable nor on its parents. |
| *Unknown file PlayerSimple.dll* | The DLL is not in the `Bin` folder of this client machine. |
| *Invalid type* | The class does not derive from a WPF control or does not implement `IPlayerLive`. |
| The tile stays on *Connecting* | `Play()` did not complete, or `FireFrameDecoded` is never called. |
| Red error message, then retries | FFmpeg cannot open the address (URL, credentials, firewall, RTSP over UDP blocked...). Test the address with `ffplay` or VLC. |
| The video stops at the end of a file | `LoopOnEndOfFile` is not set. |
| The new DLL is not taken into account | A client still has the old DLL loaded: close all clients, then replace it. |

**Debugging:** attach Visual Studio to `appClient.exe` (*Debug › Attach to Process*) and set breakpoints in
`Connect` and `Play`. The traces of `this.DebugWriteLine()` (`Prysm.AppVision.Common`) are visible in DebugView.

---

## Learning path

```
PlayerSimple   ->  PlayerAdvanced                    ->  PlayerMetadata
live only          live, playback, PTZ, export           overlay drawn over the video (object detection)
```
