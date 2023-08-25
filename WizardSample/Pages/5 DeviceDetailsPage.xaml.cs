using Prysm.AppVision.Common;
using System.Linq;
using System.Windows.Controls;


namespace WizardSample
{
    public partial class DeviceDetailsPage : UserControl
    {
        public DeviceDetailsPage()
        {
            InitializeComponent();
        }
        private void SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            // shortcut to sync seleted entities
            ((ViewModel)DataContext).SelectedEntities.ReplaceAll(grid.SelectedItems.OfType<Entity>());
        }
    }
}
