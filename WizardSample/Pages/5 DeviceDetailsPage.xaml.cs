using Prysm.AppVision.Common;
using System.Linq;
using System.Windows.Controls;


namespace WizardSample
{
    /// <summary>
    /// Wizard step 5 — Entity selection page.
    /// Displays the entities scanned from the connected device (populated by <see cref="ViewModel.ScanDevice"/>)
    /// and lets the operator choose which ones to import.
    /// Forwards to <see cref="ViewModel.SettingsCommand"/> on <b>Next</b>.
    /// </summary>
    public partial class DeviceDetailsPage : UserControl
    {
        public DeviceDetailsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Keeps <see cref="ViewModel.SelectedEntities"/> in sync with the grid's multi-selection.
        /// </summary>
        private void SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // shortcut to sync seleted entities
            ((ViewModel)DataContext).SelectedEntities.ReplaceAll(grid.SelectedItems.OfType<Entity>());
        }
    }
}
