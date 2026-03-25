using Microsoft.Win32;
using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using WizardSample.Properties;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Collections.Specialized;

namespace WizardSample
{
	/// <summary>
	/// Main ViewModel for the WizardSample multi-page onboarding wizard.
	/// Implements <see cref="INotifyPropertyChanged"/> for WPF data-binding.
	/// Manages page navigation, AppVision server login, device discovery,
	/// entity scanning and the import of discovered devices into the AppVision configuration.
	/// </summary>
	public partial class ViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Initialises the AppVision connection, loads localisation labels,
		/// and shows the <see cref="WelcomePage"/>.
		/// </summary>
		public ViewModel()
		{
			// appvision server
			appServer = new AppServer();
			RT.Load(); // load labels

			// startup page
			CurrentPage = new WelcomePage();
		}

		#region gestion de la navigation

		/// <summary>Gets or sets the wizard page currently shown in the main content area.</summary>
		public Control CurrentPage
		{
			get { return _page; }
			set { _page = value; Notify(); }
		}
		Control _page;

		/// <summary>Navigates to the AppVision login page (or skips it when credentials come from the command line).</summary>
		public ICommand AppLoginCommand => new RelayCommand(GotoAppLoginPage);
		/// <summary>Logs in to AppVision and navigates to the device discovery page.</summary>
		public ICommand DiscoveryCommand => new RelayCommand(GotoDiscoveryPage, CanGotoDiscoveryPage);
		/// <summary>Opens <see cref="AddDeviceDialog"/> to manually add a device to the discovered list.</summary>
		public ICommand AddDeviceCommand => new RelayCommand(AddDevice);
		/// <summary>Navigates to the device credential page for the selected device.</summary>
		public ICommand DeviceLoginCommand => new RelayCommand(GotoDeviceLoginPage, CanGotoDeviceLoginPage);
		/// <summary>Logs in to the device and navigates to the entity selection page.</summary>
		public ICommand DeviceDetailsCommand => new RelayCommand(GotoDeviceDetailsPage);
		/// <summary>Navigates to the import settings page once at least one entity is selected.</summary>
		public ICommand SettingsCommand => new RelayCommand(GotoSettingsPage, CanGotoSettingsPage);
		/// <summary>Opens a file picker to choose the WPF XAML symbol used for entity variables.</summary>
		public ICommand BrowseSymbolCommand => new RelayCommand(BrowseSymbol);
		/// <summary>Composes the import summary and navigates to the summary page.</summary>
		public ICommand SummaryCommand => new RelayCommand(GotoSummaryPage, CanGotoSummaryPage);
		/// <summary>Starts the import and navigates to the progress page.</summary>
		public ICommand ImportCommand => new RelayCommand(GotoImportPage);
		/// <summary>Cancels the in-progress import; enabled only while the import has not yet committed changes.</summary>
		public ICommand CancelImportCommand => new RelayCommand(CancelImport, CanCancelImport);

		/// <summary>
		/// Parses the command-line for AppVision credentials; if they are complete, skips the login
		/// page and jumps directly to device discovery. Otherwise shows <see cref="LoginAppPage"/>
		/// and starts a local-network server scan.
		/// </summary>
		void GotoAppLoginPage()
		{
			// parse command line for AppVision credential
			var args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				var adrs = Helper.ParseUri(args[1]);
				AppHostname = adrs.Scheme + "://" + adrs.Authority;
				AppUsername = adrs.GetUsername();
				AppPassword = adrs.GetPassword();

				// skip AppVision login page if cmd line contains user/pwd
				if (CanGotoDiscoveryPage())
				{
					GotoDiscoveryPage();
					return;
				}
			}
			// else ask for user/pwd
			CurrentPage = new LoginAppPage();

			// scan for AppVision servers on local network
			AppServers.Servers.CollectionChanged -= AppServerFound;
			AppServers.Servers.CollectionChanged += AppServerFound;
			AppServers.Discover();
		}

		/// <summary>Auto-fills <see cref="AppHostname"/> with the first AppVision server discovered on the LAN.</summary>
		void AppServerFound(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(AppHostname))
				AppHostname = AppServers.Servers.FirstOrDefault()?.ToString();
		}

		/// <summary>Gets the mDNS/UDP discovery client used to locate AppVision servers on the local network.</summary>
		public DiscoveryClient AppServers { get; } = new DiscoveryClient();
		/// <summary>Returns <see langword="true"/> when hostname, username and password are all non-empty.</summary>
		bool CanGotoDiscoveryPage() => !StringExtensions.IsNullOrWhiteSpace(AppHostname, AppUsername, AppPassword);

		/// <summary>
		/// Logs in to AppVision (blocking with a wait cursor), then navigates to
		/// <see cref="DiscoveryPage"/> and starts background device scanning.
		/// On login failure returns to <see cref="LoginAppPage"/>.
		/// </summary>
		async void GotoDiscoveryPage()
		{
			try
			{
				using (new WaitCursor())
					await LoginApp();
			}
			catch
			{
				CurrentPage = new LoginAppPage(); // in case we came from welcome page
				throw;
			}

			// go to devices discovery page
			CurrentPage = new DiscoveryPage();

			_ = DiscoverDevices();

			/* if discovery is not supported, go directly to device login form
			CurrentPage = new DeviceLoginPage();
			*/
		}

		/// <summary>
		/// Opens <see cref="AddDeviceDialog"/> and, if confirmed, appends the new device to <see cref="Devices"/>.
		/// </summary>
		void AddDevice()
		{
			var device = new Device();
			var dlg = new AddDeviceDialog { DataContext = device };
			if (dlg.ShowDialog() != true) return;
			Devices.Add(device);
		}

		/// <summary>Returns <see langword="true"/> when at least one device is selected in the discovery grid.</summary>
		bool CanGotoDeviceLoginPage() => SelectedDevice != null;
		/// <summary>
		/// Stops any running discovery, then navigates to <see cref="DeviceLoginPage"/>.
		/// </summary>
		void GotoDeviceLoginPage()
		{
			StopDiscovery(); // if discovery is not done yet

			CurrentPage = new DeviceLoginPage();

			/* if device does not require login, skip login form and go to device details page
			CurrentPage = new DeviceDetailsPage();
			ScanDevice();
			*/
		}

		/// <summary>
		/// Logs in to the selected device (blocking with a wait cursor), navigates to
		/// <see cref="DeviceDetailsPage"/> and scans the device for entities asynchronously.
		/// </summary>
		async void GotoDeviceDetailsPage()
		{
			using (new WaitCursor())
				await LoginDevice();

			CurrentPage = new DeviceDetailsPage();

			/* if device does not need to be scanned or scanning is already done during logging, skip device details page and go to settings page
			CurrentPage = new SettingsPage();
			*/
			IsBusy = true;
			try
			{
				await ScanDevice();
			}
			finally
			{
				IsBusy = false;
			}
		}

		/// <summary>Returns <see langword="true"/> when at least one entity is selected in the entity grid.</summary>
		bool CanGotoSettingsPage() => SelectedEntities.Count > 0;
		/// <summary>
		/// Logs out of the device gracefully, pre-fills the node name from the selected device,
		/// and navigates to <see cref="SettingsPage"/>.
		/// </summary>
		void GotoSettingsPage()
		{
			_ = LogoutDevice(); // optional. graceful log off

			NodeName = SelectedDevice.Name.ToAlphaNum("_");
			NodeDescription = SelectedDevice.Name;

			CurrentPage = new SettingsPage();
		}

		/// <summary>
		/// Opens a file picker pre-filtered to AppVision XAML symbol files,
		/// converts the chosen path to relative and stores it in <see cref="EntitySymbol"/>.
		/// </summary>
		void BrowseSymbol()
		{
			var dlg = new OpenFileDialog
			{
				InitialDirectory = Path.Combine(appServer.GetPathParam(), "Client"),
				Filter = "Symbol files (*.xaml)|*.xaml",
			};
			if (dlg.ShowDialog() != true)
				return;

			var symbol = dlg.FileName;

			//  relative path from Config\Param\Client
			if (symbol.StartsWith(dlg.InitialDirectory, true, null))
				symbol = symbol.Substring(dlg.InitialDirectory.Length + 1);

			EntitySymbol = symbol;
		}

		/// <summary>Returns <see langword="true"/> when both <see cref="ProtocolName"/> and <see cref="NodeName"/> are non-empty.</summary>
		bool CanGotoSummaryPage() => !StringExtensions.IsNullOrWhiteSpace(ProtocolName, NodeName);
		/// <summary>
		/// Composes the import summary into <see cref="Details"/> and navigates to <see cref="SummaryPage"/>.
		/// </summary>
		void GotoSummaryPage()
		{
			// compose the summary as you like 
			Details.Clear();
			Log("--- Import summary ---");
			Log(RT.Get("App.Name") + " server: " + AppHostname);
			Log("Config: " + appServer.GetPathConfig());
			Log("Root node: $V." + NodeName);
			if (CreateGroup)
				Log("Group: $G." + NodeName);
			Log(SelectedDevices.Count + " device(s) to import");
			Log(SelectedEntities.Count + " entity(ies) to import");

			Message = $"{SelectedEntities.Count} entity(ies) ready to import.";

			CurrentPage = new SummaryPage();
		}


		/// <summary>
		/// Navigates to <see cref="ImportPage"/>, runs the import asynchronously,
		/// then navigates to <see cref="CompletedPage"/> when finished (successfully or not).
		/// </summary>
		async void GotoImportPage()
		{
			CurrentPage = new ImportPage();
			try
			{
				await Import();
			}
			catch (Exception ex)
			{
				Success = false;
				Message = ex.Message;
			}
			CurrentPage = new CompletedPage();
		}

		#endregion

		#region AppServer

		/// <summary>AppVision SDK client used for configuration read/write during the wizard.</summary>
		AppServer appServer;

		/// <summary>Gets or sets the AppVision server URL (persisted in user settings).</summary>
		public string AppHostname
		{
			get { return Settings.Default.AppHostname; }
			set { Settings.Default.AppHostname = value; }
		}
		/// <summary>Gets or sets the AppVision login name (persisted in user settings).</summary>
		public string AppUsername
		{
			get { return Settings.Default.AppUsername; }
			set { Settings.Default.AppUsername = value; }
		}
		/// <summary>Gets or sets the AppVision password (not persisted).</summary>
		public string AppPassword { get; set; }

		#endregion

		#region Devices 

		/// <summary>Gets or sets the network address of the device to import (persisted in user settings).</summary>
		public string DeviceHostname
		{
			get { return Settings.Default.DeviceHostname; }
			set { Settings.Default.DeviceHostname = value; }
		}
		/// <summary>Gets or sets the device login name (persisted in user settings).</summary>
		public string DeviceUsername
		{
			get { return Settings.Default.DeviceUsername; }
			set { Settings.Default.DeviceUsername = value; }
		}
		/// <summary>Gets or sets the device password (not persisted).</summary>
		public string DevicePassword { get; set; }

		/// <summary>Gets the list of discovered devices shown in the discovery grid.</summary>
		public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

		// <todo> use SelectedDevices collection if batch import is suppported
		// use SelectedDevice object if device must be imported one by one.
		/// <summary>Gets the devices checked by the user in the discovery grid.</summary>
		public ObservableCollection<Device> SelectedDevices { get; } = new ObservableCollection<Device>();
		/// <summary>Gets the first selected device (shortcut for single-device import scenarios).</summary>
		public Device SelectedDevice
		{
			//get; set;
			get => SelectedDevices.FirstOrDefault();
			set { }
		}

		/// <summary>Gets the entities scanned from the selected device.</summary>
		public ObservableCollection<Entity> Entities { get; } = new ObservableCollection<Entity>();
		/// <summary>Gets the entities checked by the user for import.</summary>
		public ObservableCollection<Entity> SelectedEntities { get; } = new ObservableCollection<Entity>();

		#endregion

		#region NodeName,NodeDescription

		/// <summary>Gets or sets the AppVision driver protocol name used as the executable prefix (persisted).</summary>
		public string ProtocolName
		{
			get { return Settings.Default.ProtocolName; }
			set { Settings.Default.ProtocolName = value; }
		}
		/// <summary>Gets or sets the root node variable name created in AppVision (persisted).</summary>
		public string NodeName
		{
			get { return Settings.Default.NodeName; }
			set { Settings.Default.NodeName = value; }
		}
		/// <summary>Gets or sets the description for the root node variable (persisted).</summary>
		public string NodeDescription
		{
			get { return Settings.Default.NodeDescription; }
			set { Settings.Default.NodeDescription = value; }
		}

		// <todo> add or remove settings if necessary
		//public string OtherSettings { get; set; }

		// example:
		/// <summary>Gets or sets whether to create a variable group for the imported device.</summary>
		public bool CreateGroup { get; set; }

		/// <summary>Gets or sets the relative XAML symbol path assigned to each entity variable.</summary>
		public string EntitySymbol // set an UI symbol for each entity variables
		{
			get { return _symbol; }
			set { _symbol = value; Notify(); }
		}
		string _symbol;
		#endregion

		/// <summary>Gets the log lines displayed in the summary and import pages.</summary>
		public ObservableCollection<string> Details { get; } = new ObservableCollection<string>();
		/// <summary>Gets or sets the completion message displayed on the final page.</summary>
		public string Message { get; set; }
		/// <summary>Gets or sets whether the import completed successfully (controls the completion icon).</summary>
		public bool Success { get; set; } = true;
		/// <summary>Gets or sets the import progress in the range [0, 1] for the progress bar.</summary>
		public double Progress
		{
			get { return _progress; }
			set { _progress = value; Notify(); }
		}
		double _progress;
		/// <summary>Gets or sets a value indicating whether an indeterminate operation is in progress.</summary>
		public bool IsBusy
		{
			get { return _busy; }
			set { _busy = value; Notify(); }
		}
		bool _busy;


		#region LoginApp(),LogoutApp()

		/// <summary>
		/// Connects and logs in to the AppVision server on a background thread,
		/// then persists the hostname and username to user settings.
		/// </summary>
		Task LoginApp()
		{
			return Task.Run(() =>
			{
				appServer.Open(AppHostname);
				appServer.Login(AppUsername, AppPassword);
				Settings.Default.Save(); // store host + user
			});
		}
		/// <summary>Closes the AppVision connection.</summary>
		void LogoutApp()
		{
			appServer.Close();
		}
		#endregion

		#region Discovery

		/// <summary>
		/// Discovers network devices and populates <see cref="Devices"/>.
		/// The demo implementation pings every host on the local subnet; replace with
		/// a real protocol-specific scan for production use.
		/// </summary>
		async Task DiscoverDevices()
		{
			IsBusy = true;
			Devices.Clear();

			// <todo> scan network for devices


			// <demo>
			// for demo purpose, we ping some IP adrs on local network 

			var hostentry = await Dns.GetHostEntryAsync(Dns.GetHostName());
			await Task.WhenAll(
					from local in hostentry.AddressList
					where local.AddressFamily == AddressFamily.InterNetwork // IPv4 only
					let l = local.ToString()
					select l.Remove(l.LastIndexOf('.')) into net // ex: 172.10.0.xxx
					from i in Enumerable.Range(1, 254)
					select SendPing(net + "." + i));

			IsBusy = false;

			// </demo>

		}


		// <demo>
		async Task SendPing(string ip)
		{
			using (var ping = new Ping())
			{
				var reply = await ping.SendPingAsync(ip).ConfigureAwait(false);
				if (reply.Status == IPStatus.Success)
				{

					// create and add a Device object for each equipment that reply to our ping request

					var device = new Device { Address = ip, Icon = "/Resources/device.png" };
					InvokeUI(() => Devices.Add(device));
					try
					{
						device.Name = (await Dns.GetHostEntryAsync(ip).ConfigureAwait(false)).HostName;
						InvokeUI(() => device.Notify(""));

						// if desired, check for devices already imported in AppVision configuration
						device.VariableRow = appServer.VariableManager.GetRowByName(device.Name.ToAlphaNum("_"));
						InvokeUI(() => device.Notify("VariableRow"));
					}
					catch (Exception ex)
					{
						device.Name = ex.Message;
					}
				}
			}
		}
		// </demo>


		/// <summary>Cancels any in-progress discovery scan (stub — implement for real hardware).</summary>
		void StopDiscovery()
		{
			// todo
			// stop device discovery
		}

		#endregion

		#region LoginDevice(),LogoutDevice()
		/// <summary>
		/// Authenticates with the selected device.
		/// Replace the demo <see cref="Task.Delay"/> with a real protocol login call.
		/// </summary>
		Task LoginDevice()
		{
			// <todo>
			// log on device

			// in case of error:
			//throw new Exception($"Login to {DeviceHostname} failed.");

			// <demo> simulate a time consuming action
			return Task.Delay(2000);
		}

		/// <summary>Closes the session with the device gracefully (stub — implement as needed).</summary>
		async Task LogoutDevice()
		{
			// <todo>
			// log out device
		}

		/// <summary>
		/// Fetches entities from the connected device and populates <see cref="Entities"/>.
		/// The demo implementation lists files from the common documents folder; replace with
		/// a real device enumeration call.
		/// </summary>
		async Task ScanDevice()
		{
			// <todo> get entities from selected device 

			// <demo>
			// for demo purpose, populate device's entities with files from MyDocuments folder

			var myDocs = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
			foreach (FileInfo file in myDocs.EnumerateFiles())
			{
				var icon = Icon.ExtractAssociatedIcon(file.FullName); // requires System.Drawing.dll
				var img = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());

				var entity = new Entity
				{
					Name = file.Name,
					Description = file.LastWriteTime + "",
					Icon = img
				};
				Entities.Add(entity);

				// if desired, check for entities already imported in AppVision configuration
				_ = Task.Run(() =>
				{
					entity.VariableRow = appServer.VariableManager.GetRowByName(SelectedDevice.Name.ToAlphaNum("_") + "." + entity.Name.ToAlphaNum("_"));
					InvokeUI(() => entity.Notify("VariableRow"));
				});

				await Task.Delay(50); // simulate time consuming action

			}
			// </demo>
		}

		#endregion

		#region Utils

		/// <summary>Appends a message to <see cref="Details"/> and writes it to the AppVision log.</summary>
		void Log(string s)
		{
			Helper.Log(s);
			InvokeUI(() => Details.Add(s));
		}

		/// <summary>Marshals an action to the WPF UI thread via the application dispatcher.</summary>
		void InvokeUI(Action action) => App.Current.Dispatcher.InvokeAsync(action);

		/// <summary>Raises <see cref="PropertyChanged"/> on the UI thread for the calling member.</summary>
		protected void Notify([CallerMemberName] string propertyName = null)
		{
			InvokeUI(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
		}
		/// <inheritdoc/>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
