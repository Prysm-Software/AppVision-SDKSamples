using System.Windows;
using System.Windows.Controls;

namespace WizardSample
{
    public partial class LoginAppPage : UserControl
    {
        public LoginAppPage()
        {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            pwd.Focus();
        }

        private void pwd_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ViewModel)DataContext).AppPassword = pwd.Password;
        }
    }
}
