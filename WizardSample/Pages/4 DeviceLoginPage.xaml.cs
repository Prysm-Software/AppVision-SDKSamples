using System.Windows.Controls;

namespace WizardSample
{
    public partial class DeviceLoginPage : UserControl
    {
        public DeviceLoginPage()
        {
            InitializeComponent();
        }

		private void devicePwd_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			((ViewModel)DataContext).DevicePassword = devicePwd.Password;
		}
	}
}
