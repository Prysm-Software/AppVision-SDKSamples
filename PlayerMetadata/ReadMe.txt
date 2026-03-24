PlayerMetadata - Video Metadata / Overlay Provider Sample
=========================================================

OVERVIEW
--------
A metadata provider that draws AI-detected object bounding boxes over a video
player displayed in AppVision. It uses the YOLO neural network (via the
Alturos.Yolo NuGet package) for real-time object detection.

The provider:
  1. Decodes a video stream from the AppVision config directory using FFmpeg.
  2. Runs each decoded frame through the YOLO detector.
  3. Draws labelled bounding boxes on a Canvas overlaid on top of the video player.

Use this as a reference for any metadata provider that needs to annotate live
video with analytics data (licence plates, heatmaps, intrusion zones, etc.).

This C# project references:
  appSDK.dll          (Prysm.AppVision.SDK)    - Sdk.Server, IPlayerOverlay
  appVideo.dll        (Prysm.AppVision.Video)  - IPlayerOverlay interface
  Prysm.AppVision.FFmpeg                       - FFmpegVideoPlayerCtrl
  Alturos.Yolo        (NuGet)                  - YoloWrapper, ConfigurationDetector,
                                                 YoloItem

The output 'PlayerMetadata.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  PlayerMetadata.cs   IPlayerOverlay implementation: decode, detect, draw.


KEY CONCEPTS
------------
1. IPlayerOverlay
   A metadata provider is a class that implements IPlayerOverlay (not a
   UserControl). AppVision instantiates it alongside the video player control
   and passes a WPF Canvas for drawing.

   Core methods:
       Connect(cameraName, address, parameters)   Camera assigned.
       Disconnect()                               Camera removed.
       Play(canvas)                               Start live overlay drawing.
       Play(canvas, start)                        Start playback overlay.
       PlaybackPosition  { set; }                 Seek notification.

2. FFmpegVideoPlayerCtrl
   An off-screen FFmpeg decoder used to obtain raw frames independently of the
   visible player. Subscribe to:
       .NewImageDecoded += ImageDecoded    Fired for each decoded frame (BitmapSource).
       .ErrorOccured    += ...             Fired on decode errors.
       .StatusChanged   += ...             Fired on state changes.
   Start decoding: media.Play(url).   Stop: media.Stop().

3. Canvas drawing
   All drawing is done on the Canvas passed by Play(). Use WPF shapes:
       Rectangle  - bounding box.
       TextBlock  - label and confidence score.
       Image      - icon or thumbnail.
   Always dispatch drawing back to the UI thread if detection runs on a
   background thread.

4. YoloWrapper  (Alturos.Yolo)
   Wrap the YOLO native library:
       new ConfigurationDetector().Detect()   Auto-detect model files in the
                                              working directory.
       new YoloWrapper(config)                Load the detector.
       yolo.Detect(imageBytes)                Returns IEnumerable<YoloItem>.
   Each YoloItem has: Type, Confidence, X, Y, Width, Height (pixel coords).

5. Server.Hostname usage
   The video file URL is built by combining the server hostname with a relative
   path from the camera's address field:
       url = Sdk.Server.Hostname + "Config/Param/Client/" + address;
   This convention allows the config file to specify a relative path that works
   regardless of which server the client is connected to.

6. media.EnableSynchro
   Setting EnableSynchro = true makes FFmpegVideoPlayerCtrl decode at the
   natural frame rate of the source, preventing the decoder from running
   as fast as possible and saturating the CPU.


APPVISION SDK REFERENCE
-----------------------
IPlayerOverlay  (appVideo.dll)
  .Connect(name, address, param)   Camera assigned.
  .Disconnect()                    Camera removed.
  .Play(canvas)                    Start live overlay.
  .Play(canvas, start)             Start playback overlay.
  .PlaybackPosition                Property: current playback position.

Sdk.Server  (appSDK.dll)
  .Hostname                        Server base URL.

FFmpegVideoPlayerCtrl  (Prysm.AppVision.FFmpeg)
  .Play(url)                       Start decoding.
  .Stop()                          Stop decoding.
  .NewImageDecoded                 Event: new frame (BitmapSource) decoded.
  .ErrorOccured                    Event: decode error.
  .EnableSynchro                   Throttle to native frame rate.


CONFIGURATION IN APPVISION
---------------------------
1. Variable:
       Configurator > Variables > Add node
           Address:   relative path to a video file under Config/Param/Client/
           Metadata:  PlayerMetadata.dll#PlayerMetadata.PlayerMetadata

2. Ensure YOLO model files (*.cfg, *.weights, *.names) are present in the
   AppVision 4\Bin\ directory next to PlayerMetadata.dll.

3. FFmpeg native binaries must be present in Bin\.


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy PlayerMetadata.dll to:  AppVision 4\Bin\
3. Copy YOLO model files to:    AppVision 4\Bin\


SEE ALSO
--------
PlayerSimple    - Simple live player (no metadata).
PlayerAdvanced  - Full player with playback, PTZ, export.
