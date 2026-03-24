WpfControlAdvanced - Advanced AppVision WPF Control Sample
==========================================================

OVERVIEW
--------
A fully-featured WPF UserControl for the AppVision client that demonstrates:
  - IPersistable   - save and restore control state across sessions.
  - ICommandable   - receive commands issued by AppVision scripting rules.
  - IContainer     - discover the parent container (synoptic panel) by name.
  - Async data loading   - FileInfoManager, AreaManager, VariableManager.
  - Real-time subscriptions  - StateChanged, AlarmChanged, EventChanged.
  - Constructor parameters   - pass configuration tokens at load time.
  - Interactive navigation   - open a synoptic selected from a list.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - utility helpers, Join extension
  appData.dll    (Prysm.AppVision.Data)    - VariableState, AlarmInfo, EventRow
  appSDK.dll     (Prysm.AppVision.SDK)     - Sdk.Server, Sdk.Client
  appUI.dll      (Prysm.AppVision.UI)      - IPersistable, ICommandable, IContainer

The output 'WpfControlAdvanced.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  UserControl1.xaml.cs   All control logic: data loading, subscriptions, interfaces.


KEY CONCEPTS
------------
1. IPersistable
   The host calls GetConfig() / SetConfig(string) when saving and restoring the
   synoptic layout. Serialise all user-adjustable settings to a compact string
   (e.g. key=value pairs) and restore them in SetConfig.

2. ICommandable
   The host calls Execute(commandName, args) when an AppVision script issues a
   command targeted at this control. Implement a switch on commandName to handle
   custom actions (e.g. "Highlight", "Reset", "Filter").

3. IContainer and GetAncestor<T>()
   GetAncestor<IContainer>() walks the WPF visual tree upward and returns the
   nearest IContainer ancestor (the synoptic panel hosting this control).
   Use .Name to identify which panel the control is placed in.

4. Parameterised constructor
   Embedding code can pass parameters at load time:
       Sdk.Client.Load(0, "main", "WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1",
                       "arg1", "argN");
   AppVision instantiates the control with the public constructor that accepts
   params string[] args. Inspect args to configure behaviour.

5. Async data loading in Loaded event
   Heavy queries (FileInfoManager.GetRows(), AreaManager.GetRows()) are called
   in UserControl_Loaded with async/await so the UI thread is never blocked.

6. Subscription cleanup in Unloaded event
   Always remove event handlers in UserControl_Unloaded to prevent memory leaks
   when the synoptic is closed or the control is replaced.

7. Server information API
   Sdk.Server.GetOption("App.Main.ProjectDescription")   Project name.
   Sdk.Server.ControllerManager.ServerState.Version      Server version string.
   Sdk.Server.Hostname                                    Server address.
   Sdk.Server.GetLicenseInfo("SN")                        Serial number.
   Sdk.Server.UserManager.CurrentUser?.FullName           Connected user.


APPVISION SDK REFERENCE
-----------------------
Sdk.Server  (static AppServer - appSDK.dll)
  .GetOption(key)                           Read a server-side option.
  .GetLicenseInfo(key)                      Read a licence field.
  .ControllerManager.ServerState            Server version / status object.
  .FileInfoManager.GetRows()                List of synoptics, symbols, etc.
  .AreaManager.GetRows()                    List of areas (site tree).
  .VariableManager.GetStates()              All variable current values.
  .VariableManager.StateChanged             Event: variable value changed.
  .AlarmManager.AlarmChanged                Event: alarm status changed.
  .EventManager.EventChanged                Event: custom event raised.

Sdk.Client  (appSDK.dll)
  .Load(filename)                           Open a synoptic by filename.

IPersistable  (appUI.dll)
  .GetConfig()    string                    Serialise control state.
  .SetConfig(s)                             Restore control state.

ICommandable  (appUI.dll)
  .Execute(cmd, args)                       Handle a scripted command.

IContainer  (appUI.dll)
  .Name                                     Name of the host container.


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy WpfControlAdvanced.dll to:  AppVision 4\Bin\


DEBUGGING
---------
- Attach to the AppVision client process (Attach to Process in Visual Studio).
- Check the Log() rolling list in the control UI for received events.


LEARNING PATH
-------------
WpfControlSimple  ->  WpfControlAdvanced  ->  PlayerSimple / PlayerAdvanced
Basic read/write      Full interface set        Video controls
