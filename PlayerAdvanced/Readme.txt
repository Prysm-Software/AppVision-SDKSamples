PlayerAdvanced - Advanced AppVision Video Player Sample
=======================================================

OVERVIEW
--------
A full-featured WPF video player control for AppVision demonstrating all four
player interfaces that a production player may implement:

  IPlayerLive      - live streaming.
  IPlayerPlayback  - video playback with seek, speed control, and progress events.
  IPlayerPtz       - PTZ camera pan / tilt / zoom commands.
  IPlayerExport    - video clip export to a local file.

The player renders a clock face (no real video decoding) to demonstrate the
framework mechanics without a camera dependency. Every tick emits a new
"video frame" via FireFrameDecoded so that the rest of the AppVision pipeline
(recording, overlays, motion detection) receives the simulated stream.

This C# project references:
  appSDK.dll      (Prysm.AppVision.SDK)    - PlayerBaseControl, player interfaces
  appVideo.dll    (Prysm.AppVision.Video)  - IPlayerLive, IPlayerPlayback, IPlayerPtz,
                                             IPlayerExport, FireFrameDecoded
  appCommon.dll   (Prysm.AppVision.Common) - Helper.ParseUri, DebugWriteLine

The output 'PlayerClock.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  PlayerClockControl.xaml.cs   All player logic: live, playback, PTZ, export.


KEY CONCEPTS
------------
1. Implementing multiple player interfaces
   Implement only the interfaces that the real device supports.
   AppVision queries the control for each interface and enables/disables
   the corresponding UI buttons (playback bar, PTZ overlay, export button)
   automatically.

2. IPlayerPlayback
   Play(start, duration, speed)   Start playback from a point in time.
   Pause() / Resume()             Pause and resume.
   Stop()                         Stop and return to idle.
   SeekTo(position)               Jump to a specific timestamp.
   PlaybackSpeed                  Property set by AppVision for speed changes.
   PlaybackPosition               Property: current playback time.
   PlaybackEnded                  Event: raise when the clip has finished.

3. IPlayerPtz
   PtzMove(pan, tilt, zoom)       Continuous move (-1..+1 per axis).
   PtzStop()                      Stop all movement.
   PtzPreset(presetId)            Move to a saved preset position.
   PtzHome()                      Move to the home position.

4. IPlayerExport
   Export(outputPath, start, duration)   Export a clip to a file.
   ExportProgress                        Event: 0..100 percent.

5. PlaybackSpeed
   AppVision sets PlaybackSpeed before calling Play(). The driver should honour
   the requested speed (1x, 2x, 0.5x). Use a DispatcherTimer interval inversely
   proportional to PlaybackSpeed to simulate correct timing.

6. DebugWriteLine()
   Extension method (Prysm.AppVision.Common) that writes the caller's method
   name and optional message to the debug output. Useful for tracing lifecycle
   calls without verbose logging.

7. Helper.ParseUri(address)
   Parses a connection string of the form "user:pwd@hostname:port" into a Uri.
   Use .GetUsername(), .GetPassword() to extract credentials.


APPVISION SDK REFERENCE
-----------------------
PlayerBaseControl  (appVideo.dll / appSDK.dll)
  .Connect(name, address, param)   Camera assigned.
  .Disconnect()                    Camera removed.
  .StretchVideo                    Fill vs uniform stretch.
  .FireFrameDecoded(date, bmp)     Forward frame to AppVision pipeline.
  .SetError(message)               Report error.
  .Mode                            Current mode (Live / Playback / Idle).
  .Status                          Current status string.

IPlayerLive        Play() / Pause() / Resume() / Stop()
IPlayerPlayback    Play(start,dur,spd) / Pause / Resume / Stop / SeekTo
IPlayerPtz         PtzMove / PtzStop / PtzPreset / PtzHome
IPlayerExport      Export(path,start,dur) + ExportProgress event


CONFIGURATION IN APPVISION
---------------------------
1. Variable:
       Configurator > Variables > Add node
           Player: PlayerClock.dll#PlayerClock.PlayerClockControl

2. In a synoptic: add a Camera object and assign the variable.


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy PlayerClock.dll to:  AppVision 4\Bin\


DEBUGGING
---------
- Use DebugWriteLine() in each overridden method to trace lifecycle calls.
- tbBottom text block in the XAML shows Mode + Status at runtime.


LEARNING PATH
-------------
PlayerSimple  ->  PlayerAdvanced  ->  PlayerMetadata
Live only         Full interface set   Metadata overlay (YOLO)
