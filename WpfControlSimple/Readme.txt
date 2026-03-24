WpfControlSimple - Simple AppVision WPF Control Sample
======================================================

OVERVIEW
--------
A minimal WPF UserControl designed to be embedded inside an AppVision client.
It demonstrates how a custom control can interact with the AppVision server and
with its own client host using the SDK.

The control:
  - Lists all variable names from AppVision in a ComboBox.
  - Reads and displays the current value of the selected variable.
  - Writes a new value back to AppVision when the user clicks Set.
  - Opens a second instance of itself in a new floating window via Sdk.Client.Open().

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - utility helpers
  appData.dll    (Prysm.AppVision.Data)    - data model
  appSDK.dll     (Prysm.AppVision.SDK)     - Sdk.Server, Sdk.Client
  appUI.dll      (Prysm.AppVision.UI)      - AppVision UI integration

The output 'WpfControlSimple.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  UserControl1.xaml.cs   All control logic: variable read/write, self-open.


KEY CONCEPTS
------------
1. Sdk.Server - server interaction
   Sdk.Server is the static AppServer singleton already connected by the
   AppVision client host. The control must never call Open() or Login() itself.

   Read a variable value:
       Sdk.Server.OperandManager.Read("$V." + variableName)
   Write a variable value:
       Sdk.Server.OperandManager.Write("$V." + variableName, value)

   OperandManager.Read/Write accept the full operand path ("$V.nodeName.varName")
   and return / accept object values (string, bool, int, double).

2. Sdk.Client - host interaction
   Sdk.Client exposes the AppVision client shell API.
   Open a new floating window containing this same control:
       Sdk.Client.Open("WpfControlSimple.dll#WpfControlSimple.UserControl1", "Size=320,240")
   The first argument is  assemblyFileName#fullyQualifiedClassName.
   The second argument is an optional parameter string interpreted by the host.

3. Embedding in a synoptic
   The control can be placed on any AppVision synoptic via:
       Configurator > Synoptics > Objects > "Third-party .Net object"
       Assembly: WpfControlSimple.dll
       Class:    WpfControlSimple.UserControl1

4. Design-time guard
   Always check Sdk.Server.IsConnected before querying the server in the
   constructor or in the XAML designer, otherwise the designer will throw.


APPVISION SDK REFERENCE
-----------------------
Sdk.Server  (static AppServer - appSDK.dll)
  .IsConnected                            True if the client host is connected.
  .VariableManager.GetStates()            Snapshot of all current variable values.
  .OperandManager.Read(operandPath)       Read any operand value.
  .OperandManager.Write(operandPath, v)   Write any operand value.

Sdk.Client  (appSDK.dll)
  .Open(classRef, parameters)             Open a control in a new host window.


CONFIGURATION IN APPVISION
---------------------------
No protocol or variable configuration is required beyond the variables that
already exist in the project (they are enumerated dynamically at runtime).


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy WpfControlSimple.dll to:  AppVision 4\Bin\
3. Restart the AppVision client (or reload the synoptic) to pick up the new DLL.


DEBUGGING
---------
- Load the DLL from the AppVision client. Visual Studio can attach to the
  client process (Attach to Process) and hit breakpoints inside the control.
- In XAML designer, the control will be empty because Sdk.Server is not
  connected at design time â€” this is expected.


LEARNING PATH
-------------
WpfControlSimple  ->  WpfControlAdvanced
Read/Write             IPersistable, ICommandable, IContainer, async data loading
