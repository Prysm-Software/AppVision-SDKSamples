using System.Windows.Controls;

namespace WizardSample
{
	/// <summary>
	/// Wizard step 4 — Device login page.
	/// Collects the device credentials bound to <see cref="ViewModel.DeviceUsername"/>
	/// and <see cref="ViewModel.DevicePassword"/>.
	/// Forwards to <see cref="ViewModel.DeviceDetailsCommand"/> on <b>Next</b>.
	/// </summary>
	public partial class DeviceLoginPage : UserControl
	{
		public DeviceLoginPage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Propagates the device password to <see cref="ViewModel.DevicePassword"/>.
		/// Direct binding is not possible because <see cref="System.Windows.Controls.PasswordBox.Password"/> is not a dependency property.
		/// </summary>
		private void devicePwd_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			((ViewModel)DataContext).DevicePassword = devicePwd.Password;
		}
	}
}
