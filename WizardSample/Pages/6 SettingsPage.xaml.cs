using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 6 — Import settings page.
    /// Lets the operator configure the AppVision node name, protocol name, entity symbol
    /// and whether to create a variable group before reviewing the summary.
    /// Exposes an expandable <b>More settings</b> panel for advanced options.
    /// Forwards to <see cref="ViewModel.SummaryCommand"/> on <b>Next</b>.
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        /// <summary>Expands the advanced-settings panel and hides the collapsed placeholder.</summary>
        private void More_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            less.Visibility = Visibility.Collapsed;
            more.Visibility = Visibility.Visible;
        }
    }
}
