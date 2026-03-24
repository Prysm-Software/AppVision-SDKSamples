DriverMinimal - Minimal AppVision Driver Sample
===============================================

OVERVIEW
--------
The simplest possible AppVision driver: a console application with no GUI.
Use this as a starting point to understand the basic SDK connection sequence
before moving on to DriverSimple or DriverAdvanced.

The driver logs every variable state change received from AppVision to the
console. It does not manage device objects, alarms, or value caching.

This C# project references:
  appSDK.dll  (Prysm.AppVision.SDK)  - AppServerForDriver

The output 'DriverMinimal.exe' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  Program.cs   Single-file driver: connection, notification loop, shutdown.


KEY CONCEPTS
------------
1. Minimal connection sequence
       AppServerForDriver.Open(host)                      Connect (empty = localhost).
       AppServerForDriver.Login(protocolName)             Register as driver.
       AppServerForDriver.AddFilterNotifications("$V.*")  Subscribe to all variables.
       AppServerForDriver.StartNotifications(false)       Begin receiving callbacks.
       AppServerForDriver.ProtocolSynchronized()          Signal readiness to AppVision.

2. VariableState
   VariableManager.StateChanged delivers a VariableState on every change:
       .Name    Full variable name.
       .Value   New value (object).
       .Date    Timestamp of the change.

3. Lifecycle
   ControllerManager.Closed is raised when AppVision shuts down or removes the
   protocol. The driver sets _closing = true and exits the polling loop cleanly.

4. Pushing values to AppVision
   Call VariableManager.Set() from the polling loop to reflect external device
   events in AppVision:
       _appServer.VariableManager.Set("Some.Variable.Name", "some value");


APPVISION SDK REFERENCE
-----------------------
AppServerForDriver  (appSDK.dll)
  .Open(host)                           Connect (empty = localhost).
  .Login(protocolName)                  Register as the protocol handler.
  .AddFilterNotifications(filter)       Subscribe to variable events.
  .StartNotifications(false)            Start receiving state-change callbacks.
  .ProtocolSynchronized()               Notify AppVision the driver is ready.
  .VariableManager.StateChanged         Event: variable value changed.
  .VariableManager.Set(name, value)     Push a value to AppVision.
  .ControllerManager.Closed            Event: AppVision requests shutdown.


CONFIGURATION IN APPVISION
---------------------------
1. Create the protocol:
       Configurator > Protocols > Add
           Name:    DriverMinimal
           Manager: DriverMinimal.exe


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy DriverMinimal.exe to:  AppVision 4\Bin\
3. Start the protocol from AppVision. The driver is launched automatically.


DEBUGGING
---------
- Project Properties > Debug > Command line arguments: DriverMinimal[@localhost]
- Every received variable state change is printed to the console.


LEARNING PATH
-------------
DriverMinimal  ->  DriverSimple  ->  DriverAdvanced
No GUI             WPF + 8 I/O      Device pattern, alarms, caching
