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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        AppServer _server = new AppServer();
        ObservableCollection<OverlayItem> _overlays = new ObservableCollection<OverlayItem>();
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

        // Cancel the datagrid built-in DeleteCommand and call RemoveOverlay(id) for each selected overlays.
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


        // Pre-fill some data when the user create a new row in the datagrid.
        private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            var overlay = (OverlayItem)e.NewItem;
            overlay.Id = "new overlay " + dataGrid.Items.Count;
        }

        // Call UpdateOverlay(overlayItem) when the user modify a new or existing overlay.
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
        // List of symbols and synoptics in the configuration (auto completion)
        public IEnumerable<string> Synoptics => _files?.Where(f => f.Type == FileInfoType.Synoptic).Select(f => f.Name).Concat(0, "@geo");
        public IEnumerable<string> Symbols => _files?.Where(f => f.Type == FileInfoType.Symbol).Select(f => f.Name);
    }
}
