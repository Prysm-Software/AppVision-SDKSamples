using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace WizardSample
{
    /// <summary>
    /// Modal dialog that lets the operator manually enter device credentials
    /// when automatic discovery does not find a device.
    /// The dialog's <see cref="Window.DataContext"/> must be a <see cref="Device"/> instance;
    /// it is populated via the form and the caller reads it after <see cref="Window.DialogResult"/> is <see langword="true"/>.
    /// </summary>
    public partial class AddDeviceDialog : Window
    {
        public AddDeviceDialog()
        {
            InitializeComponent();
        }

        /// <summary>Confirms the dialog and returns <see cref="Window.DialogResult"/> = <see langword="true"/>.</summary>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
