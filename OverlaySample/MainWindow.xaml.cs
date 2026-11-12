using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OverlayManager
{
	/// <summary>
	/// Standalone tool to list, create, modify and delete the overlays of an AppVision server.
	/// Displays a live-updating DataGrid backed by an <see cref="ObservableCollection{T}"/> of
	/// <see cref="OverlayItem"/> objects and synchronises create/update/delete operations with
	/// the AppVision server in real time.
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		/// <summary>AppVision server connection.</summary>
		readonly AppServer _server = new AppServer();
		/// <summary>Live-bound collection of overlays displayed in the DataGrid.</summary>
		readonly ObservableCollection<OverlayItem> _overlays = new ObservableCollection<OverlayItem>();
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
				// Log in AppVision server (connection string: http://user:password@host)
				var uri = Helper.ParseUri(connectionString.Text);
				_server.Open(uri.Scheme + "://" + uri.Authority);
				_server.Login(uri.GetUsername(), uri.GetPassword());

				// Subscribe to overlay updates BEFORE listing them, so that no change is missed.
				// "-=" first: the user can click Connect again after a connection loss.
				_server.ControllerManager.ConnectionLost -= OnConnectionLost;
				_server.ControllerManager.ConnectionLost += OnConnectionLost;
				_server.ClientManager.OverlayUpdated -= Overlay_Updated;
				_server.ClientManager.OverlayUpdated += Overlay_Updated;
				_server.AddFilterNotifications("NotificationType=Overlay");
				// No initial values (the list is read below); polling: no HTTP listener to open on this machine.
				_server.StartNotifications(false, false);
				UpdateUI(isConnected: true);

				// Retrieve the list of symbols and synoptics (suggestions of the Symbol and Synoptic columns)
				_files = await _server.FileInfoManager.GetRows();
				PropertyChanged?.Notify(this, "");

				// Retrieve the list of overlays
				_overlays.Clear();
				foreach (var overlay in await _server.ClientManager.GetOverlays())
					AddOrReplace(overlay);
			}
		}

		/// <summary>
		/// Raised by the SDK on a background thread when the connection with the server is lost.
		/// </summary>
		void OnConnectionLost(string hostname, bool tryReconnect) =>
			Dispatcher.InvokeAsync(() => UpdateUI(isConnected: false));

		void UpdateUI(bool isConnected)
		{
			connectionBar.IsEnabled = !isConnected;
			dataGrid.IsEnabled = isConnected;
		}

		/// <summary>
		/// Called by the SDK whenever an overlay is created, updated or deleted on the server.
		/// Keeps the local <see cref="_overlays"/> collection in sync with the server state.
		/// </summary>
		/// <remarks>
		/// Notifications arrive on a background thread, and a collection bound to a DataGrid can only be modified
		/// on the UI thread: go back to it with the Dispatcher. (Exceptions thrown by an event handler are swallowed
		/// by the SDK, so a missing Dispatcher call fails silently.)
		/// </remarks>
		/// <param name="event"><see cref="ModifType.Update"/> for a creation or a modification,
		/// <see cref="ModifType.Delete"/> for a deletion.</param>
		/// <param name="item">The overlay that was modified.</param>
		void Overlay_Updated(ModifType @event, OverlayItem item)
		{
			Dispatcher.InvokeAsync(() =>
			{
				if (@event == ModifType.Delete)
					_overlays.RemoveAll(ov => ov.Name == item.Name);
				else
					AddOrReplace(item);
			});
		}

		/// <summary>Adds an overlay to the list, or replaces the overlay of the same name.</summary>
		void AddOrReplace(OverlayItem item)
		{
			int idx = _overlays.IndexOf(ov => ov.Name == item.Name);
			if (idx == -1)
				_overlays.Add(item);
			else
				_overlays[idx] = item;
		}

		/// <summary>
		/// Replaces the DataGrid built-in Delete command (Del key) by a call to
		/// <see cref="Prysm.AppVision.SDK.ClientManager.RemoveOverlay"/> for each selected row:
		/// the server deletes the overlays and notifies every client, including this one (see <see cref="Overlay_Updated"/>).
		/// </summary>
		private async void DataGrid_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command != DataGrid.DeleteCommand)
				return;

			e.Handled = true;
			// OfType skips the placeholder of the new row
			foreach (var overlay in dataGrid.SelectedItems.OfType<OverlayItem>().ToList())
				await _server.ClientManager.RemoveOverlay(overlay.Name);
		}

		/// <summary>
		/// The name identifies an overlay: it can be typed for a new row only.
		/// Renaming an existing row would create a second overlay and keep the old one.
		/// </summary>
		private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			if (Equals(e.Column.Header, "Name") && !e.Row.IsNewItem)
				e.Cancel = true;
		}

		/// <summary>
		/// Pre-fills a default name when the user starts typing a new overlay row.
		/// </summary>
		private void DataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
		{
			var overlay = (OverlayItem)e.NewItem;
			overlay.Name = "new overlay " + dataGrid.Items.Count;
		}

		/// <summary>
		/// Commits a row edit and sends the overlay to AppVision with
		/// <see cref="Prysm.AppVision.SDK.ClientManager.UpdateOverlay(OverlayItem)"/> (creation or modification).
		/// Temporarily removes and re-adds this handler around <c>CommitEdit</c> to
		/// avoid a recursive call.
		/// </summary>
		async void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
		{
			if (e.EditAction == DataGridEditAction.Commit)
			{
				dataGrid.RowEditEnding -= DataGrid_RowEditEnding;
				dataGrid.CommitEdit();
				dataGrid.RowEditEnding += DataGrid_RowEditEnding;

				var overlay = (OverlayItem)e.Row.DataContext;
				await _server.ClientManager.UpdateOverlay(overlay);
			}
		}

		/// <summary>
		/// The columns declared in the XAML are kept; the other public properties of <see cref="OverlayItem"/>
		/// get an automatic column, except the internal or obsolete ones.
		/// </summary>
		private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
		{
			if (dataGrid.Columns.IndexOf(c => c.Header.Equals(e.PropertyName)) > -1
				|| e.PropertyName.EndsWith("String")                  // serialization helpers of Position and Size
				|| e.PropertyName is "Id" or "Tag" or "ServerName")   // obsolete alias of Name, internal properties
				e.Cancel = true;
		}

		/// <summary>Names of all synoptic files available on the server, plus the special <c>@geo</c> value.
		/// Used for column auto-complete in the DataGrid.</summary>
		public IEnumerable<string> Synoptics => _files?.Where(f => f.Type == FileInfoType.Synoptic).Select(f => f.Name).Prepend("@geo");
		/// <summary>Names of all symbol (XAML) files available on the server.
		/// Used for column auto-complete in the DataGrid.</summary>
		public IEnumerable<string> Symbols => _files?.Where(f => f.Type == FileInfoType.Symbol).Select(f => f.Name);
	}
}
