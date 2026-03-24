WizardSample - AppVision Configuration Wizard Sample
=====================================================

OVERVIEW
--------
A multi-page WPF wizard that guides an operator through discovering, connecting,
and importing a new device into the AppVision configuration.

Use this as a starting template for a driver-specific setup wizard.
Replace every "<todo>" placeholder with device-specific logic.

This C# project references:
  appCommon.dll  (Prysm.AppVision.Common)  - Helper, RT (labels), RelayCommand
  appData.dll    (Prysm.AppVision.Data)    - VariableRow, VariableType, ProtocolRow
  appSDK.dll     (Prysm.AppVision.SDK)     - AppServer, VariableManager, ProtocolManager

The output 'WizardSample.exe' is a standalone WPF application.


PROJECT STRUCTURE
-----------------
  MainWindow.xaml.cs         Shell: hosts the current page UserControl.
  ViewModel.cs               Navigation commands and shared state.
  ViewModel.Import.cs        Import logic: create protocol, variables, groups.
  Device.cs                  Model: discovered device data (IP, name, entities).
  Entity.cs                  Model: one device entity (camera, reader, sensor).
  Pages\1 WelcomePage        Startup / splash page.
  Pages\2 AppLoginPage       Connect to AppVision server.
  Pages\3 DiscoveryPage      Discover devices on the network (optional).
  Pages\4 DeviceLoginPage    Collect device address and credentials.
  Pages\5 DeviceDetailsPage  Scan device; list all entities to import.
  Pages\6 SettingsPage       Configure import options (node name, symbol, etc.).
  Pages\7 SummaryPage        Review before import.
  Pages\8 ImportPage         Execute import and show progress.
  Pages\9 CompletedPage      Show import results and offer to open AppVision.


WIZARD FLOW
-----------
WelcomePage
    |
AppLoginPage    <- skip if credentials supplied on the command line
    |                  (http://user:pwd@host)
DiscoveryPage   <- remove if the device type has no discovery mechanism
    |
DeviceLoginPage <- remove if the device requires no authentication
    |
DeviceDetailsPage
    |
SettingsPage
    |
SummaryPage
    |
ImportPage
    |
CompletedPage


KEY CONCEPTS
------------
1. MVVM navigation
   MainWindow.xaml binds its content to ViewModel.CurrentPage.
   Each wizard step is a UserControl. Navigation commands (AppLoginCommand,
   DiscoveryCommand, ...) are ICommand properties on ViewModel that set
   CurrentPage to the next UserControl instance.

2. Shared state via ViewModel
   Device details discovered in early pages are stored as ViewModel properties
   (DeviceHostname, DeviceUsername, DevicePassword, SelectedEntities, ...) and
   read by later pages without needing explicit parameter passing.

3. AppServer in wizard context
   The wizard uses its own AppServer instance (not Sdk.Server) because it is
   a standalone tool, not a control hosted by the AppVision client.
   Connect sequence:
       appServer.Open(hostUrl)
       appServer.Login(user, password)
       await appServer.LoadDatabase(true)

4. Creating configuration objects
   In ViewModel.Import.cs, the wizard creates protocol and variable rows:
       // Create protocol
       var proto = new ProtocolRow { Name = ProtocolName, Manager = ManagerExe };
       appServer.ProtocolManager.Add(proto);
       // Create node variable
       var varDevice = new VariableRow
       {
           Name = NodeName, Type = VariableType.Node,
           Expression = "$P." + ProtocolName,
           Address = $"{DeviceUsername}:{DevicePassword}@{DeviceHostname}"
       };
       appServer.VariableManager.Add(varDevice);
       appServer.SaveChanges();

5. Progress reporting in ImportPage
   Import runs on a background thread (ConfigureAwait(false)).
   Progress is reported via IProgress<T> or ViewModel properties bound to
   a ProgressBar in ImportPage.xaml.

6. Command-line login skip
   If the wizard is launched with a URL containing credentials
   (e.g. http://ADM:123@myserver), AppLoginPage is automatically skipped
   and the server connection is established silently.


APPVISION SDK REFERENCE
-----------------------
AppServer  (appSDK.dll)
  .Open(hostUrl) / .Login(user, pwd) / .LoadDatabase(force)

VariableManager  (appSDK.dll)
  .GetRowByName(name)            Lookup existing variable.
  .Add(variableRow)              Create new variable.
  .SaveChanges()                 Persist all pending changes.

ProtocolManager  (appSDK.dll)
  .Add(protocolRow)              Create new protocol entry.

VariableRow  (appData.dll)
  .Name, .Description, .Type, .Expression, .Address, .Parameters

VariableType  (appData.dll)
  .Node, .Logical, .Analog, .Buffer, ...

RT  (appCommon.dll)
  .Load()                        Load localised label resources.


CONFIGURATION IN APPVISION
---------------------------
The wizard creates the required configuration itself. No manual pre-configuration
is needed. The operator runs the wizard, follows the steps, and the wizard writes
the protocol and variable definitions into AppVision.


HOW TO BUILD AND RUN
--------------------
1. Build in Release configuration.
2. Run WizardSample.exe [http://user:pwd@hostname]
   The optional command-line argument pre-fills and skips the login page.


DEBUGGING
---------
- Set a breakpoint in ViewModel.Import.cs / Import() to trace the created objects.
- Check the AppVision Configurator after a successful import to verify the
  created protocol and variables.


SEE ALSO
--------
DriverAdvanced   - The driver that the wizard would typically configure.
ClientAdvanced   - Client that uses Sdk.Server (contrast with wizard AppServer).
