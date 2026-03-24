
DriverAdvanced - AppVision SDK Driver Sample
============================================

OVERVIEW
--------
This project is a reference implementation for developing a custom AppVision driver.
A driver (also called a connector) is an executable that bridges AppVision with an
external system such as a security controller (PSIM), a field device, or a standard
communication protocol (MQTT, OPC-UA, BACnet, SIA, Modbus, etc.).

The driver runs as a separate process and communicates with the AppVision server
through the AppVision SDK (appSDK.dll). It is managed by AppVision (started, stopped,
restarted) through the protocol configuration.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - utility helpers
  appData.dll    (Prysm.AppVision.Data)    - data model
  appSDK.dll      (Prysm.AppVision.SDK)     - driver SDK
  App.Logger.dll  (Prysm.Logger)            - logging infrastructure (Trace, LogLevel, LogBox)
  App.Utility.dll (Prysm.Utility.Logger)    - AppVision log appenders (AppServerLogAppender)

The output 'DriverAdvanced.exe' is copied into 'AppVision 4\Bin\' folder.


ARCHITECTURE
------------
  AppVision Server
	  |
	  |-- ProtocolManager  --> launches and monitors DriverAdvanced.exe
	  |-- VariableManager  <--> Driver  (reads/writes variable values and states)
	  |-- AlarmManager      <-- Driver  (raises alarms on errors)
	  |-- ControllerManager --> notifies driver of server events (restart, close)

The driver subscribes to variable state changes from AppVision (commands) and pushes
variable values back to AppVision (data acquired from external devices).


PROJECT STRUCTURE
-----------------
  App.xaml.cs         Entry point: WPF application lifecycle, error handling, graceful shutdown.
  MainWindow.xaml.cs  Diagnostic UI: live log output with configurable level and max count.
  Driver.cs           Core driver: connects to AppVision, discovers devices, routes events.
  Device.cs           Per-device logic: connect/disconnect one external device, handle commands.
  BuildInfo.cs        Auto-generated build date used for the SDK warranty date check.


KEY CONCEPTS
------------

1. Protocol
   A protocol is a named configuration entry in AppVision that points to this executable.
   AppVision launches the driver automatically and passes the protocol name as the first
   command-line argument:
	   DriverAdvanced.exe MyProtocolName[@hostname]

2. Linked Variables ("owned variables")
   Variables whose Source property is set to "$P.<ProtocolName>" are owned by this driver.
   The driver discovers all of them at startup via AppServer.GetVariablesByProtocol().
   Each root variable (e.g. "Device1") typically represents one external device.

3. Comm Variable (Communication Status)
   Each device should have a child variable named "<Device>.Comm" of type Logical with the
   "Command" flag checked. The driver sets its integer value to reflect connection status:
	   CommStatus.Unknown       (0) - initial state, not yet determined
	   CommStatus.Connected     (1) - device is reachable and communication is active
	   CommStatus.Disconnected  (2) - device is intentionally offline (driver stopped)
	   CommStatus.Fault         (3) - communication error detected

4. Variable State Notifications
   The driver registers a filter so AppVision only sends notifications for relevant variables.
   Filter syntax "$V.Device1.*" matches all sub-variables of Device1.
   The additional filter "Variable.StateType=1" restricts events to variables that have the
   "Command" flag checked, i.e., variables that operators or rules use to send commands.

5. Value Caching
   SetVariable() uses an internal dictionary to suppress redundant updates.
   If the new value equals the cached value, no message is sent to AppVision.
   Use SetVariableNoCache() to force-send a value regardless of the cache.

6. Alarm Management
   AddAlarm() sends structured alarms to AppVision's alarm manager.
   Each alarm includes: alarm code, short message, variable description, full exception detail,
   area path, and group descriptions for rich contextual display in the AppVision UI.

7. Download / Refresh
   Download: called by AppVision to push a configuration snapshot to the external system
			 (typically used in AppControl scenarios).
   Refresh:  called by AppVision to request a full resynchronisation of device state.


APPVISION SDK REFERENCE (appSDK.dll)
-------------------------------------
AppServerForDriver               Main gateway to the AppVision server for drivers.
  .Open(host)                    Connect to the AppVision server (empty = local machine).
  .Login(protocolName)           Register this process as the handler for the given protocol.
  .GetVariablesByProtocol()      Return all variables linked to the current protocol.
  .AddFilterNotifications(f)     Subscribe to specific variable change events (filter string).
  .StartNotifications(false)     Start receiving notifications (false = synchronous delivery).
  .ProtocolSynchronized()        Signal to AppVision that the driver is ready and running.
  .CurrentProtocol               Protocol configuration row (name, address, parameters, ...).
  .CheckWarrantyDate(date)       Verify the build date is within the SDK warranty period.
  .Restart                       Event: AppVision requests a driver restart.
  .Download                      Event: AppVision sends a download command.
  .Refresh                       Event: AppVision sends a refresh command.
  .IsConnected                   True when the connection to the AppVision server is open.
  .Close()                       Cleanly disconnect from the AppVision server.

VariableManager                  Manages variable state.
  .StateChanged                  Event: a variable value changed (command from AppVision).
  .RowUpdated                    Event: a variable's configuration row was modified.
  .Set(name, value, info, op)    Push a new value for a variable to AppVision.

AlarmManager
  .AddAlarmExt(code, msg, ...)   Raise a structured alarm in AppVision.

Helper (Prysm.AppVision.Common)
  .ParseUri(address)             Parse a URI from a variable's Address field.
  .ParseEndPoint(address, port)  Parse a TCP endpoint (ip:port).
  .GetParameter(name, params)    Extract a named parameter from a parameter string.
  .GetParameter<T>(name, params) Typed variant of GetParameter.
  .ProductName / .ProductVersion AppVision product metadata strings.
  .PathResources                 Path to the AppVision resources folder.

CommStatus enum
  Unknown | Connected | Disconnected | Fault

Trace (Prysm.Logger - App.Logger.dll)
  The single static logging entry point used throughout the driver.
  Output is dispatched to all registered ILogAppender instances.
  Trace.Info(message)        Write an informational message.
  Trace.Error(message/ex)    Write an error message or exception.
  Trace.StatusText           Set the status bar text in the MainWindow.
  Trace.Logger               The underlying Logger; used to register appenders.
  Trace.Logger.AddAppender(appender, level, tag)
                             Attach any ILogAppender with a minimum log level and tag.
  LogLevel enum              All | Debug | Info | Warning | Error | Fatal | None
  LogBox                     WPF control (ILogAppender) that renders log entries in a
                             scrollable, colour-coded list in the MainWindow.
                             .MaxCount - maximum number of lines kept in memory.
                             .LogLevel - minimum level displayed in the control.

AppServerLogAppender (Prysm.Utility.Logger - App.Utility.dll)
  An ILogAppender that forwards Trace entries to the AppVision server log,
  making driver messages visible in the AppVision Manager diagnostic console.
  Usage: Trace.Logger.AddAppender(new AppServerLogAppender(AppServer), LogLevel.All, "app");
  Requires an open AppServerForDriver connection; no extra configuration needed.


CONFIGURATION IN APPVISION
---------------------------
1. Create the protocol:
	   Configurator > Protocols > Add
		   Name:    DriverAdvanced
		   Manager: DriverAdvanced.exe

2. Create the device variables:
	   Configurator > Variables > Add

		   Device1         Source: $P.DriverAdvanced
		   Device1.Comm    Type: Logical    Flag Command: checked

		   Device2         Source: $P.DriverAdvanced
		   Device2.Comm    Type: Logical    Flag Command: checked

   Additional data sub-variables can be added under each device as needed.
   Their names must start with the device name (e.g. "Device1.Temperature").


HOW TO BUILD AND DEPLOY
-----------------------
1. Build the project in Release configuration.
2. Copy DriverAdvanced.exe to the AppVision Bin folder:
	   AppVision 4\Bin\
   Alternatively, place it in a subfolder (e.g. Bin\Drivers\DriverAdvanced\):
	   The assembly resolver in App.xaml.cs will locate the SDK DLLs in the parent folder.
3. Start the protocol from AppVision Configurator or the AppVision Manager.
   The driver will be launched automatically with the protocol name as argument.


DEBUGGING
---------
- In Visual Studio: Project Properties > Debug > Command line arguments:
	  DriverAdvanced[@localhost]
- The MainWindow shows a live log. Right-click to change log level or maximum line count.
- To stop the driver manually: hold Ctrl and close the window (Ctrl+close).
  Closing without Ctrl is blocked intentionally to prevent accidental shutdown in production.
- Log output is forwarded to the AppVision server log via AppServerLogAppender,
  so it also appears in the AppVision Manager diagnostic console.


HOW TO ADAPT THIS SAMPLE FOR YOUR OWN DRIVER
---------------------------------------------
1. Rename the project, assembly, and namespace to match your system.
2. Update BuildInfo.cs with the correct build date.
3. In Driver.cs:
   - Parse protocol-level settings from AppServer.CurrentProtocol.Address and .Parameters
	 using Helper.ParseEndPoint() or Helper.GetParameter().
   - Establish a shared/global connection to your system if required by the protocol.
4. In Device.cs:
   - Constructor: parse device address/parameters from deviceVariable.Address / .FullParameters.
   - Start(): connect to the external device and begin monitoring its state.
   - Stop(): cleanly disconnect and release resources.
   - VariableChanged(): receive commands from AppVision and forward them to your device.
   - variableUpdated(): react to variable configuration row changes if relevant.
   - Call _driver.SetVariable() to push values received from your device into AppVision.
   - Call _driver.AddAlarm() to report communication errors.
5. Adjust the notification filter in Driver.cs to cover the variables you need to watch.
6. Implement Download() and Refresh() in Driver.cs if your system supports them,
   or remove them if not needed.


DEPENDENCIES
------------
  Assembly         Namespace                 Key types / purpose
  ---------------  ------------------------  ----------------------------------------------------------
  appCommon.dll    Prysm.AppVision.Common    Helper, CommStatus, RT (labels), HidePasswords extension
  appData.dll      Prysm.AppVision.Data      VariableRow, VariableState, ProtocolRow
  appSDK.dll       Prysm.AppVision.SDK       AppServerForDriver, VariableManager, AlarmManager,
                                            ControllerManager
  App.Logger.dll   Prysm.Logger              Trace, LogLevel, ILogAppender, LogBox (WPF control)
  App.Utility.dll  Prysm.Utility.Logger      AppServerLogAppender

  AppServerLogAppender bridges the driver's Trace output to the AppVision server log.
  It depends on both App.Logger.dll (ILogAppender interface) and appSDK.dll.

  All assemblies ship with AppVision and are marked <Private>False</Private> in the
  .csproj: they are NOT copied to the driver output folder. The driver resolves them
  at runtime from the AppVision Bin\ folder (or its parent, via LookForAssemblyInParentDir).