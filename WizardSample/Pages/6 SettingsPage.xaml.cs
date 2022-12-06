using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void More_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            less.Visibility = Visibility.Collapsed;
            more.Visibility = Visibility.Visible;
        }
    }
}
