DriverAppControl - AppControl Access Controller Driver Sample
=============================================================

OVERVIEW
--------
An AppVision driver that bridges AppVision with an AppControl access control
system. It connects to AppControl hardware via the AppControl SDK, mirrors
reader states and access events into AppVision variables, and forwards
AppVision commands (e.g. door unlock) to the physical controllers.

Use this as a reference for drivers that communicate with access controllers,
intrusion panels, or other security devices through a device-specific SDK.

This C# project references:
  appCommon.dll   (Prysm.AppVision.Common)  - utility helpers
  appData.dll     (Prysm.AppVision.Data)    - data model
  appSDK.dll      (Prysm.AppVision.SDK)     - AppServerForDriver
  appControl.dll  (Prysm.AppControl.SDK)    - ChannelService

The output 'DriverAppControl.exe' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  App.xaml.cs          WPF entry point, lifecycle management.
  MainWindow.xaml.cs   Diagnostic window (log display).
  Driver.cs            Protocol level: AppVision connection, device creation, main loop.
  Device.cs            Device level: AppControl device lifecycle, event mirroring.
  ChannelService.cs    Low-level TCP/IP channel to one AppControl controller.


KEY CONCEPTS
------------
1. AppControl SDK (appControl.dll)
   ChannelService manages the connection to one physical access controller.
   Events mirrored into AppVision:
       .ConnectChanged       Connection/disconnection.
       .InputChanged         Reader or digital input state changed.
       .AccessControlEvent   Badge swipe (grant / deny / alarm).

2. Download command
   AppServerForDriver.Download fires when AppVision requests a configuration push.
   The driver uploads timetables, access profiles, and badge lists to the controller
   using the AppControl SDK.

3. NewModification event
   AppServerForDriver.NewModification fires when configuration has changed on the
   server (e.g. a badge was added). The driver queues a download at the next cycle.

4. Cmd variable naming convention
   Command variables follow the pattern: DeviceName.Reader01.Cmd
   The notification filter "$V.DeviceName.*.Cmd" subscribes only to command
   variables, reducing notification traffic.

5. Thread-based main loop
   Driver.Main() runs on a dedicated background thread, polling device status
   and processing the download queue, avoiding blocking the WPF dispatcher.

6. Per-device physical limits
       MAX_READER    =  4       Readers per controller.
       MAX_HOLIDAY   = 24       Holiday slots.
       MAX_TIMETABLE = 48       Timetable slots.
       MAX_USER      = 10000    Cardholder capacity.
       MAX_PROFILE   = 256      Access profile slots.


CONFIGURATION IN APPVISION
---------------------------
1. Protocol:
       Configurator > Protocols > Add
           Name:    DriverAppControl
           Manager: DriverAppControl.exe

2. Variables (one set per AppControl controller):
       Device1              Source: $P.DriverAppControl
                            Address: <controller IP or connection string>
       Device1.Reader01     Type: Logical
       Device1.Reader02     Type: Logical
       Device1.Reader01.Cmd Type: Logical   Flag Command: checked


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy DriverAppControl.exe to:  AppVision 4\Bin\


DEBUGGING
---------
- Project Properties > Debug > Command line arguments: DriverAppControl[@localhost]
- The diagnostic window displays traces from both the Driver and Device layers.


SEE ALSO
--------
DriverAdvanced              - Driver template with the Device pattern.
WebServiceODataAppControl   - OData access to AppControl people/badges data.
AppControlSynchro           - CSV import of people into AppControl via OData.
