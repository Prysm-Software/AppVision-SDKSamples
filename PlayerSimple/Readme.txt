PlayerSimple - Simple AppVision Video Player Sample
====================================================

OVERVIEW
--------
A minimal WPF video player control for AppVision that streams live video using
the FFmpeg library.

The control inherits from PlayerBaseControl and implements only IPlayerLive,
meaning it supports live streaming only (no playback, PTZ, or export).
Check PlayerAdvanced for a full-featured player example.

This C# project references:
  appSDK.dll        (Prysm.AppVision.SDK)      - PlayerBaseControl, IPlayerLive
  appVideo.dll      (Prysm.AppVision.Video)    - FireFrameDecoded
  appCommon.dll     (Prysm.AppVision.Common)   - SetError helper
  Prysm.FFmpeg.Next                            - AVPlayerUri, PlayerOptions,
                                                 SoftwareVideoFrame, FrameEventArgs

The output 'PlayerSimple.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  PlayerSimple.xaml.cs   Single-file player: Connect, Play, Stop, frame callback.


KEY CONCEPTS
------------
1. PlayerBaseControl
   All AppVision video player controls must inherit PlayerBaseControl.
   The base class handles integration with AppVision (camera assignment,
   status reporting, StretchVideo, etc.) and expects the subclass to
   override the lifecycle methods.

2. IPlayerLive
   Implement this interface to support live streaming:
       Connect(cameraName, address, parameters)   Camera is assigned.
       Play()                                     Live stream requested.
       Stop()                                     Stop stream.
       Disconnect()                               Camera removed.
   AppVision calls these methods in order; implementations must be async.

3. AVPlayerUri and PlayerOptions  (Prysm.FFmpeg.Next)
   Pass the stream URL and decoding options to player.PlayAsync():
       new AVPlayerUri { Uri = url }
       new PlayerOptions { UseHardwareDecoding = false,
                           Rendering = RenderingMethod.Software }
   Set UseHardwareDecoding = true for GPU-accelerated HEVC/H.264 streams.

4. SoftwareVideoFrame and FireFrameDecoded
   player.FrameRendered fires for every decoded frame.
   Cast e.Frame to SoftwareVideoFrame to get the BitmapSource.
   Call FireFrameDecoded(timestamp, bitmapSource) to forward the frame to
   AppVision (recording, metadata overlay, motion detection, etc.):
       var bmp = (e.Frame as SoftwareVideoFrame)?.Image as BitmapSource;
       FireFrameDecoded(e.Timestamp.Date, bmp);

5. SetError(message)
   Inherited from PlayerBaseControl. Reports an error to AppVision so the
   client can display it and trigger reconnection if configured.


APPVISION SDK REFERENCE
-----------------------
PlayerBaseControl  (appVideo.dll / appSDK.dll)
  .Connect(name, address, param)   Override: camera assigned.
  .Disconnect()                    Override: camera removed.
  .Play()                          Override: start live stream.
  .Stop()                          Override: stop stream.
  .FireFrameDecoded(date, bmp)     Forward decoded frame to AppVision.
  .SetError(message)               Report an error to AppVision.
  .StretchVideo                    Property: fill vs uniform stretch.

AVPlayerUri  (Prysm.FFmpeg.Next)
  .Uri                             Stream URL (RTSP, HTTP, file path, etc.).

PlayerOptions  (Prysm.FFmpeg.Next)
  .UseHardwareDecoding             Enable GPU decoding.
  .Rendering                       RenderingMethod.Software or Hardware.


CONFIGURATION IN APPVISION
---------------------------
1. Variable (camera node):
       Configurator > Variables > Add node
           Name:    MyCam
           Source:  (stream URL, e.g. rtsp://camera-ip/stream)
           Player:  PlayerSimple.dll#PlayerSimple.PlayerSimple

2. Display in a synoptic:
       Add a Camera object and assign the MyCam variable to it.
   Or embed directly:
       Sdk.Client.Load("PlayerSimple.dll#PlayerSimple.PlayerSimple")


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy PlayerSimple.dll to:  AppVision 4\Bin\
3. Ensure FFmpeg native binaries (avcodec-*.dll, etc.) are present in Bin\.


DEBUGGING
---------
- Attach to the AppVision client process in Visual Studio.
- player.ErrorOccured callback calls SetError() â€” check the camera widget in
  the synoptic for the error message.
- Test Play() independently by assigning a local file path as the URL.


LEARNING PATH
-------------
PlayerSimple  ->  PlayerAdvanced  ->  PlayerMetadata
Live only         Playback+PTZ+Export  Overlay / metadata (YOLO)
