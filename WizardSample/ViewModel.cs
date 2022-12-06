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

namespace WizardSample
{
    public partial class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            // appvision server
            appServer = new AppServer();
            RT.Load(); // load labels

            // startup page
            CurrentPage = new WelcomePage();
        }

        #region gestion de la navigation

        // navigate to page
        public Control CurrentPage
        {
            get { return _page; }
            set { _page = value; Notify(); }
        }
        Control _page;

        public ICommand AppLoginCommand => new RelayCommand(GotoAppLoginPage);
        public ICommand DiscoveryCommand => new RelayCommand(GotoDiscoveryPage, CanGotoDiscoveryPage);
        public ICommand AddDeviceCommand => new RelayCommand(AddDevice);
        public ICommand DeviceLoginCommand => new RelayCommand(GotoDeviceLoginPage, CanGotoDeviceLoginPage);
        public ICommand DeviceDetailsCommand => new RelayCommand(GotoDeviceDetailsPage);
        public ICommand SettingsCommand => new RelayCommand(GotoSettingsPage, CanGotoSettingsPage);
        public ICommand BrowseSymbolCommand => new RelayCommand(BrowseSymbol);
        public ICommand SummaryCommand => new RelayCommand(GotoSummaryPage, CanGotoSummaryPage);
        public ICommand ImportCommand => new RelayCommand(GotoImportPage);
        public ICommand CancelImportCommand => new RelayCommand(CancelImport, CanCancelImport);

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
        }

        bool CanGotoDiscoveryPage() => !StringExtensions.IsNullOrWhiteSpace(AppHostname, AppUsername, AppPassword);

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

            DiscoverDevices();

            /* if discovery is not supported, go directly to device login form
			CurrentPage = new DeviceLoginPage();
			*/
        }

        // manually add a device 
        void AddDevice()
        {
            var device = new Device();
            var dlg = new AddDeviceDialog { DataContext = device };
            if (dlg.ShowDialog() != true) return;
            Devices.Add(device);
        }


        bool CanGotoDeviceLoginPage() => SelectedDevice != null;
        void GotoDeviceLoginPage()
        {
            StopDiscovery(); // if discovery is not done yet

            CurrentPage = new DeviceLoginPage();

            /* if device does not require login, skip login form and go to device details page
			CurrentPage = new DeviceDetailsPage();
			ScanDevice();
			*/
        }

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

        bool CanGotoSettingsPage() => SelectedEntities.Count > 0;
        void GotoSettingsPage()
        {
            LogoutDevice(); // optional. graceful log off

            NodeName = SelectedDevice.Name.ToAlphaNum("_");
            NodeDescription = SelectedDevice.Name;

            CurrentPage = new SettingsPage();
        }

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

        bool CanGotoSummaryPage() => !StringExtensions.IsNullOrWhiteSpace(ProtocolName, NodeName);
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


        // start updating appvision config and switch to complete page when it's over
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

        AppServer appServer;

        public string AppHostname
        {
            get { return Settings.Default.AppHostname; }
            set { Settings.Default.AppHostname = value; }
        }
        public string AppUsername
        {
            get { return Settings.Default.AppUsername; }
            set { Settings.Default.AppUsername = value; }
        }
        public string AppPassword { get; set; }

        public string DeviceHostname
        {
            get { return Settings.Default.DeviceHostname; }
            set { Settings.Default.DeviceHostname = value; }
        }
        public string DeviceUsername
        {
            get { return Settings.Default.DeviceUsername; }
            set { Settings.Default.DeviceUsername = value; }
        }
        public string DevicePassword { get; set; }
        #endregion

        #region Devices

        public ObservableCollection<Device> Devices { get; } = new ObservableCollection<Device>();

        // <todo> use SelectedDevices collection if batch import is suppported
        // use SelectedDevice object if device must be imported one by one.
        public ObservableCollection<Device> SelectedDevices { get; } = new ObservableCollection<Device>();
        public Device SelectedDevice
        {
            //get; set;
            get => SelectedDevices.FirstOrDefault();
            set { }
        }

        public ObservableCollection<Entity> Entities { get; } = new ObservableCollection<Entity>();
        public ObservableCollection<Entity> SelectedEntities { get; } = new ObservableCollection<Entity>();

        #endregion

        #region NodeName,NodeDescription

        public string ProtocolName
        {
            get { return Settings.Default.ProtocolName; }
            set { Settings.Default.ProtocolName = value; }
        }
        public string NodeName
        {
            get { return Settings.Default.NodeName; }
            set { Settings.Default.NodeName = value; }
        }
        public string NodeDescription
        {
            get { return Settings.Default.NodeDescription; }
            set { Settings.Default.NodeDescription = value; }
        }

        // <todo> add or remove settings if necessary
        //public string OtherSettings { get; set; }

        // example:
        public bool CreateGroup { get; set; }


        public string EntitySymbol // set an UI symbol for each entity variables
        {
            get { return _symbol; }
            set { _symbol = value; Notify(); }
        }
        string _symbol;
        #endregion

        // summary and import logs 
        public ObservableCollection<string> Details { get; } = new ObservableCollection<string>();
        // completion message
        public string Message { get; set; }
        // controls completion icon
        public bool Success { get; set; } = true;
        // progress bar [0-1]
        public double Progress
        {
            get { return _progress; }
            set { _progress = value; Notify(); }
        }
        double _progress;
        // progress bar indeterminate
        public bool IsBusy
        {
            get { return _busy; }
            set { _busy = value; Notify(); }
        }
        bool _busy;


        #region LoginApp(),LogoutApp()

        Task LoginApp()
        {
            return Task.Run(() =>
            {
                appServer.Open(AppHostname);
                appServer.Login(AppUsername, AppPassword);
                Settings.Default.Save(); // store host + user
            });
        }
        void LogoutApp()
        {
            appServer.Close();
        }
        #endregion

        #region Discovery

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


        void StopDiscovery()
        {
            // todo
            // stop device discovery
        }

        #endregion

        #region LoginDevice(),LogoutDevice()
        Task LoginDevice()
        {
            // <todo>
            // log on device

            // in case of error:
            //throw new Exception($"Login to {DeviceHostname} failed.");

            // <demo> simulate a time consuming action
            return Task.Delay(2000);
        }

        async Task LogoutDevice()
        {
            // <todo>
            // log out device
        }

        async Task ScanDevice()
        {
            // <todo> get entities from selected device 

            // <demo>
            // for demo purpose, populate device's entities with files from MyDocuments folder

            var myDocs = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
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

        void Log(string s)
        {
            Helper.Log(s);
            InvokeUI(() => Details.Add(s));
        }

        // call on UI thread
        void InvokeUI(Action action) => App.Current.Dispatcher.InvokeAsync(action);


        protected void Notify([CallerMemberName]string propertyName = null)
        {
            InvokeUI(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
