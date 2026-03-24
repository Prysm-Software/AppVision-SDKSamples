ClientSimple - Simple AppVision Client Sample
=============================================

OVERVIEW
--------
A minimal WPF application that connects to AppVision as a client and demonstrates
the three core real-time notification streams:

  - Variable state changes    (VariableManager.StateChanged)
  - Alarm events              (AlarmManager.AlarmChanged)
  - Custom events             (EventManager.EventChanged)

It also allows the user to read all current variable values and set a variable
value interactively.

Use this as the simplest possible starting point when learning the client-side SDK.

This C# project references:
  appData.dll   (Prysm.AppVision.Data)   - VariableState, AlarmInfo, EventRow
  appSDK.dll    (Prysm.AppVision.SDK)    - AppServer

The output 'ClientSimple.exe' is a standalone WPF application.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs   All client logic: connection, subscriptions, set variable.


KEY CONCEPTS
------------
1. AppServer vs AppServerForDriver
   AppServer       - used by client applications (operators, dashboards, tools).
   AppServerForDriver - used by protocol drivers. They share the same base but
   AppServer adds LoadDatabase(), UserManager, full AlarmManager, etc.

2. Connection sequence
       server.Open(hostUrl)                Connect. Accepts "http://hostname:port".
       server.Login(user, password)        Authenticate. Default: "ADM" / "123".
       await server.LoadDatabase(true)     Download configuration into local cache.
       server.AddFilterNotifications(f)    Choose notification scope.
       server.StartNotifications(false, false)   Begin receiving callbacks.

3. Notification filters
   The filter string determines which events are pushed to this client:
       "All"                        Everything.
       "$V.CR1.*"                   Only variables under node CR1.
       "$A.*"                       All alarms.
       "NotificationType=VariableState"   Only variable changes.

4. GetStates()
   After LoadDatabase, VariableManager.GetStates() returns the current
   VariableState for every variable in the configuration. Useful to populate
   a ComboBox or grid without waiting for a change event.

5. Set a variable
       server.VariableManager.Set(variableName, value)
   Pushes a new value to AppVision. AppVision distributes it to all subscribers
   (including drivers) according to the variable's command flag.


APPVISION SDK REFERENCE
-----------------------
AppServer  (appSDK.dll)
  .Open(hostUrl)                         Connect to AppVision.
  .Login(user, password)                 Authenticate.
  .LoadDatabase(forceReload)             Download configuration cache.
  .AddFilterNotifications(filter)        Select notification scope.
  .StartNotifications(initial, queue)    Start receiving callbacks.
  .Close()                               Disconnect cleanly.
  .IsConnected                           True if currently connected.

VariableManager
  .StateChanged                          Event: variable value changed.
  .GetStates()                           Snapshot of all current values.
  .Set(name, value)                      Push a new value.

AlarmManager
  .AlarmChanged                          Event: alarm status changed.

EventManager
  .EventChanged                          Event: custom event raised.


CONFIGURATION IN APPVISION
---------------------------
No specific configuration is required. Connect with any user account that has
read access. Default credentials: user "ADM", password "123".


HOW TO BUILD AND RUN
--------------------
1. Build in Release configuration.
2. Run ClientSimple.exe.
3. Enter the server address (e.g. http://localhost), user and password, then click Connect.


DEBUGGING
---------
- Set breakpoints in the StateChanged / AlarmChanged / EventChanged callbacks.
- All received events are appended to the ListBox in the main window.
- Use the variable ComboBox to inspect and change any variable value live.


LEARNING PATH
-------------
ClientSimple  ->  ClientAdvanced
Standalone        Sdk.Server singleton, RColor theme, embedded WPF controls
