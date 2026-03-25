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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 9 — Completion page.
    /// Shows a success or failure indicator driven by <see cref="ViewModel.Success"/>
    /// and the final message from <see cref="ViewModel.Message"/>.
    /// The <b>Close</b> button saves user settings and shuts down the application.
    /// </summary>
    public partial class CompletedPage : UserControl
    {
        public CompletedPage()
        {
            InitializeComponent();
        }

        /// <summary>Persists user settings and shuts down the wizard application.</summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            Application.Current.Shutdown(1); // still necessary? 
        }
    }
}
