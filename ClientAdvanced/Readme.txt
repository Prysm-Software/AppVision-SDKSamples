ClientAdvanced - Advanced AppVision Client Sample
=================================================

OVERVIEW
--------
A WPF client application that demonstrates the advanced SDK patterns used when
building rich client tools or dashboards that embed AppVision WPF controls.

Compared to ClientSimple, this sample:
  - Uses the Sdk.Server singleton instead of a local AppServer instance.
  - Loads the AppVision colour theme with RColor.Load() so embedded controls
    render with the correct branding.
  - Connects with StartNotifications(true, false) to receive the initial state
    of all variables as soon as the subscription starts.
  - Reads a server-side project option with GetOption().
  - Provides a clean disconnect flow including unsubscribing from events.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - Helper, RColor, extension methods
  appSDK.dll     (Prysm.AppVision.SDK)     - Sdk.Server, AppServer
  appUI.dll      (Prysm.AppVision.UI)      - (used by embedded WPF controls)

The output 'ClientAdvanced.exe' is a standalone WPF application.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs   Connection, subscription, and disconnect logic.
  Properties\Settings.settings   Persisted host / user / password.


KEY CONCEPTS
------------
1. Sdk.Server singleton
   Sdk.Server is a static AppServer instance shared across the entire process.
   AppVision WPF controls (WpfControlSimple, WpfControlAdvanced, etc.) all
   reference Sdk.Server internally â€” loading them before connecting Sdk.Server
   results in empty controls. Always connect Sdk.Server before loading controls.

2. RColor.Load(true)
   Called in the constructor before InitializeComponent. Loads the AppVision
   colour resource dictionary so that embedded controls use the correct palette.
   Pass true to use the dark theme (default AppVision theme).

3. StartNotifications(initialValues, queue)
   First parameter  true  : the server sends the current value of every
                            subscribed variable immediately after subscribing.
   Second parameter false : notifications are dispatched directly (not queued).

4. GetOption(key)
   Reads a named option from the AppVision server configuration:
       Sdk.Server.GetOption("App.Main.ProjectDescription")
   Useful for displaying the project name or reading custom configuration values.

5. Persisted connection settings
   Host, user, and password are stored in Properties.Settings and saved on
   successful connection. The form is pre-filled on the next launch.

6. Unsubscribe before Close
   Always remove event handlers before calling Close() to avoid callbacks
   arriving on a partially torn-down object:
       Sdk.Server.AlarmManager.AlarmChanged -= handler;
       Sdk.Server.Close();


APPVISION SDK REFERENCE
-----------------------
Sdk.Server  (static AppServer - appSDK.dll)
  .Open(hostUrl)                          Connect to AppVision.
  .Login(user, password)                  Authenticate.
  .LoadDatabase(forceReload)              Download configuration cache.
  .AddFilterNotifications(filter)         Select notification scope.
  .StartNotifications(initial, queue)     Begin receiving callbacks.
  .GetOption(key)                         Read a server-side option value.
  .Close()                                Disconnect cleanly.
  .UserManager.CurrentUser.FullName       Display name of the logged-in user.
  .AlarmManager.AlarmChanged              Event: alarm status changed.

RColor  (appCommon.dll)
  .Load(darkTheme)                        Load AppVision colour resources into
                                          Application.Current.Resources.

Helper  (appCommon.dll)
  .GetInnerMessages(exception)            Flatten nested exception messages.


CONFIGURATION IN APPVISION
---------------------------
No specific configuration is required. Use any user account with read access.
Default credentials: user "ADM", password "123".


HOW TO BUILD AND RUN
--------------------
1. Build in Release configuration.
2. Run ClientAdvanced.exe.
3. Fill in the host / user / password fields and click Connect.


DEBUGGING
---------
- Trace.WriteLine is used for exceptions â€” check the Visual Studio Output window.
- Inspect Sdk.Server.IsConnected to verify connection state.
- txtStatus displays the connected user name and project description.


LEARNING PATH
-------------
ClientSimple  ->  ClientAdvanced  ->  WpfControlSimple / WpfControlAdvanced
Standalone        Sdk singleton        Embedded custom controls
