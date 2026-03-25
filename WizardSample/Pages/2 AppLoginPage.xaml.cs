using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 2 — AppVision login page.
    /// Collects the server URL, username and password bound to
    /// <see cref="ViewModel.AppHostname"/>, <see cref="ViewModel.AppUsername"/> and
    /// <see cref="ViewModel.AppPassword"/>.
    /// Forwards to <see cref="ViewModel.DiscoveryCommand"/> on <b>Next</b>.
    /// </summary>
    public partial class LoginAppPage : UserControl
    {
        public LoginAppPage()
        {
            InitializeComponent();
        }

        /// <summary>Moves keyboard focus to the password box when the page is loaded.</summary>
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            pwd.Focus();
        }

        /// <summary>
        /// Propagates the password to <see cref="ViewModel.AppPassword"/>.
        /// Direct binding is not possible because <see cref="System.Windows.Controls.PasswordBox.Password"/> is not a dependency property.
        /// </summary>
        private void pwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ViewModel)DataContext).AppPassword = pwd.Password;
        }
    }
}
