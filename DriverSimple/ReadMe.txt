DriverSimple - Simple AppVision Driver Sample
=============================================

OVERVIEW
--------
A WPF driver that simulates a physical I/O device with 8 digital inputs,
8 command outputs, and one buffer variable.

It is an interactive demonstration of bi-directional communication:
  Inputs  : CheckBoxes in the UI simulate events from an external device.
            Clicking calls VariableManager.Set() to push the value to AppVision.
  Outputs : RadioButtons show the latest command received from AppVision
            via VariableManager.StateChanged.
  Buffer  : A random sequence of integers written to a Buffer-type variable.
  Event   : Custom event raised in AppVision via EventManager.AddEvent().
  Alarm   : Custom alarm raised in AppVision via AlarmManager.AddAlarmExt().

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - utility helpers
  appData.dll    (Prysm.AppVision.Data)    - VariableRow, VariableState
  appSDK.dll     (Prysm.AppVision.SDK)     - AppServerForDriver

The output 'DriverSimple.exe' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs   All driver logic: connection, UI events, AppVision interaction.


KEY CONCEPTS
------------
1. @init operation flag
   At startup, input variables are reset using the "@init" flag:
       VariableManager.Set("DrvSampleVar.Input1", 0, operation: "@init");
   This sets the initial value without recording a database change event,
   preventing spurious alarms or rule triggers on driver startup.

2. Restart support
   ControllerManager.Restart is raised when AppVision requests a driver restart
   (e.g. after a configuration reload). The driver closes and relaunches itself
   with the same command-line arguments using Process.Start().

3. GetVariablesByProtocol
   Returns all variables whose Source is set to the protocol:
       _protocolVariable = _appServer.GetVariablesByProtocol(protocolName).FirstOrDefault();
   The returned VariableRow gives the node name used to address child variables
   (e.g. DrvSampleVar.Input1).

4. Buffer variable
   A variable of type Buffer stores a space-separated list of integers.
   Used to transfer bulk data (schedules, access profiles, raw frames)
   between AppVision and the external system:
       VariableManager.Set("DrvSampleVar.Buffer", "12 35 78 456 -85 0");

5. Error reporting
   Error() logs to the trace window, calls AppServer.Log(), and raises an alarm:
       AlarmManager.AddAlarmExt(code, shortMessage, description);


APPVISION SDK REFERENCE
-----------------------
AppServerForDriver  (appSDK.dll)
  .GetVariablesByProtocol(name)            List variables linked to this protocol.
  .Log(text)                               Write to the AppVision server log.
  .LogLevel                                Server-side log verbosity level.

VariableManager
  .Set(name, value)                        Push a new value to AppVision.
  .Set(name, value, operation:"@init")     Push initial value (no DB event).
  .StateChanged                            Event: command received from AppVision.

EventManager
  .AddEvent(type, description)             Raise a custom event in AppVision.

AlarmManager
  .AddAlarmExt(code, msg, desc)            Raise a custom alarm in AppVision.


CONFIGURATION IN APPVISION
---------------------------
1. Protocol:
       Configurator > Protocols > Add
           Name:    DrvSample
           Manager: DriverSample.exe

2. Variables:
       DrvSampleVar         Source: $P.DrvSample
       DrvSampleVar.Input1  Type: Logical
       ...
       DrvSampleVar.Input8  Type: Logical
       DrvSampleVar.Output1 Type: Logical   Flag Command: checked   Source: =$parent.Input1
       ...
       DrvSampleVar.Output8 Type: Logical   Flag Command: checked   Source: =$parent.Input8
       DrvSampleVar.Buffer  Type: Buffer    Buffer type: Int32


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy DriverSimple.exe to:  AppVision 4\Bin\


DEBUGGING
---------
- Project Properties > Debug > Command line arguments: DrvSample[@localhost]
- The window title shows product name and version.
- The trace text area displays a live log. Errors are forwarded to the server log.


LEARNING PATH
-------------
DriverMinimal  ->  DriverSimple  ->  DriverAdvanced
No GUI             WPF + 8 I/O      Device pattern, alarms, caching
