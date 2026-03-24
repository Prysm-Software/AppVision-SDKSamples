using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OverlayManager
{
    /// <summary>
    /// Standalone CRUD tool for managing geo-referenced overlay icons on an AppVision GIS synoptic.
    /// Displays a live-updating DataGrid backed by an <see cref="ObservableCollection{T}"/> of
    /// <see cref="OverlayItem"/> objects and synchronises create/update/delete operations with
    /// the AppVision server in real time.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>AppVision client server connection handle.</summary>
        AppServer _server = new AppServer();
        /// <summary>Live-bound collection of overlays displayed in the DataGrid.</summary>
        ObservableCollection<OverlayItem> _overlays = new ObservableCollection<OverlayItem>();
        /// <summary>Cache of all file entries (symbols + synoptics) used for column auto-complete.</summary>
        FileInfoRow[] _files;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            dataGrid.ItemsSource = _overlays;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (_server.IsConnected)
                _server.Close();
        }

        /// <summary>
        /// Connects to AppVision, subscribes to overlay change notifications
        /// and loads the current overlay list into the DataGrid.
        /// </summary>
        async void Connect_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                // Log in AppVision server
                var uri = Helper.ParseUri(connectionString.Text);
                _server.Open(uri.Scheme + "://" + uri.Authority);
                _server.Login(uri.GetUsername(), uri.GetPassword());
                _server.ControllerManager.ConnectionLost += (s, b) => UpdateUI(isConnected: false);

                // Subscribe to overlay updates
                _server.ClientManager.OverlayUpdated += Overlay_Updated;
                _server.AddFilterNotifications("NotificationType=Overlay");
                _server.StartNotifications(false);
                UpdateUI(isConnected: true);
                PropertyChanged?.Notify(this, "");

                // Retrieve the list of symbols and synoptics
                _files = await _server.FileInfoManager.GetRows();

                // Retrieve the list of overlays
                _overlays.AddRange(await _server.ClientManager.GetOverlays());
            }
        }

        void UpdateUI(bool isConnected)
        {
            connectionBar.IsEnabled = !isConnected;
            dataGrid.IsEnabled = isConnected;
        }

        /// <summary>
        /// Called by AppVision whenever an overlay is created, updated or deleted on the server.
        /// Keeps the local <see cref="_overlays"/> collection in sync with the server state.
        /// </summary>
        /// <param name="event">The type of modification (Add/Update/Delete).</param>
        /// <param name="item">The overlay that was modified.</param>
        void Overlay_Updated(ModifType @event, OverlayItem item)
        {
            if (@event == ModifType.Delete)
            {
                _overlays.RemoveAll(ov => ov.Id == item.Id);
            }
            else
            {
                int idx = _overlays.IndexOf(ov => ov.Id == item.Id);
                if (idx == -1)
                    _overlays.Add(item);
                else
                    _overlays[idx] = item;
            }
        }

        /// <summary>
        /// Intercepts the DataGrid built-in Delete command and replaces it with a call to
        /// <see cref="Prysm.AppVision.SDK.ClientManager.RemoveOverlay"/> for each selected row,
        /// so the server is updated rather than just the local collection.
        /// </summary>
        private void DataGrid_PreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == DataGrid.DeleteCommand)
            {
                e.Handled = true;
                foreach (OverlayItem overlay in dataGrid.SelectedItems)
                {
                    _server.ClientManager.RemoveOverlay(overlay.Id);
                }
            }
        }


        /// <summary>
        /// Pre-fills a sensible default ID when the user starts typing a new overlay row.
        /// </summary>
        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            var overlay = (OverlayItem)e.NewItem;
            overlay.Id = "new overlay " + dataGrid.Items.Count;
        }

        /// <summary>
        /// Commits a row edit and persists the change to AppVision via
        /// <see cref="Prysm.AppVision.SDK.ClientManager.UpdateOverlay(OverlayItem)"/>.
        /// Temporarily removes and re-adds this handler around <c>CommitEdit</c> to
        /// avoid a recursive call.
        /// </summary>
        async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Console.WriteLine("edit " + e.Row);
            if (e.EditAction == DataGridEditAction.Commit)
            {
                dataGrid.RowEditEnding -= DataGrid_RowEditEnding;
                dataGrid.CommitEdit();
                dataGrid.RowEditEnding += DataGrid_RowEditEnding;

                var overlay = (OverlayItem)e.Row.DataContext;
                await _server.ClientManager.UpdateOverlay(overlay);

            }
        }

        // Do not auto-generate column for all OverlayItem properties.
        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (dataGrid.Columns.IndexOf(c => c.Header.Equals(e.PropertyName)) > -1 || e.PropertyName.EndsWith("String"))
                e.Cancel = true;
        }
        /// <summary>Names of all synoptic files available on the server, plus the special <c>@geo</c> value.
        /// Used for column auto-complete in the DataGrid.</summary>
        public IEnumerable<string> Synoptics => _files?.Where(f => f.Type == FileInfoType.Synoptic).Select(f => f.Name).Concat(0, "@geo");
        /// <summary>Names of all symbol (XAML) files available on the server.
        /// Used for column auto-complete in the DataGrid.</summary>
        public IEnumerable<string> Symbols => _files?.Where(f => f.Type == FileInfoType.Symbol).Select(f => f.Name);
    }
}
