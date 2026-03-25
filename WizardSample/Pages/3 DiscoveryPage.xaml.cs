using Prysm.AppVision.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 3 — Device discovery page.
    /// Displays the list of devices found by <see cref="ViewModel.DiscoverDevices"/> and
    /// lets the operator select one or more for import.
    /// Forwards to <see cref="ViewModel.DeviceLoginCommand"/> on <b>Next</b>.
    /// </summary>
    public partial class DiscoveryPage : UserControl
    {
        public DiscoveryPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Keeps <see cref="ViewModel.SelectedDevices"/> in sync with the grid's multi-selection.
        /// </summary>
        private void SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // shortcut to sync seleted devices
            ((ViewModel)DataContext).SelectedDevices.ReplaceAll(grid.SelectedItems.OfType<Device>());
        }
    }
}
