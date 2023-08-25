using Prysm.AppVision.Common;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    public partial class DiscoveryPage : UserControl
    {
        public DiscoveryPage()
        {
            InitializeComponent();
        }
        private void SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // shortcut to sync seleted devices
            ((ViewModel)DataContext).SelectedDevices.ReplaceAll(grid.SelectedItems.OfType<Device>());
        }
    }
}
