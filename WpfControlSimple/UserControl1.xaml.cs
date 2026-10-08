using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfControlSimple
{
	/// <summary>
	/// Embeddable synoptic control that lets an operator read and write AppVision
	/// variable values directly from a WPF UserControl placed on a synoptic page.
	/// Uses <see cref="Sdk.Server"/>'s <see cref="Prysm.AppVision.SDK.OperandManager"/>
	/// for single-call read/write without explicit subscriptions.
	/// </summary>
	/// <remarks>
	/// The control never opens its own connection: it runs inside the AppVision client,
	/// which has already connected and logged in <see cref="Sdk.Server"/>.
	/// </remarks>
	public partial class UserControl1 : UserControl
	{
		/// <summary>
		/// Initialises the control and populates the variable name drop-down
		/// from the AppVision variable cache (when already connected).
		/// </summary>
		public UserControl1()
		{
			InitializeComponent();

			// no server in the Visual Studio designer or in the configurator
			if (Sdk.Server.IsConnected)
				varName.ItemsSource = (from state in Sdk.Server.VariableManager.GetStates()
									   where state.Row?.Type != VariableType.Node // nodes have no value
									   orderby state.Name
									   select state.Name).ToList();
		}

		/// <summary>
		/// Reads the current value of the selected variable from the client cache via
		/// <see cref="Prysm.AppVision.SDK.OperandManager.Read"/> and displays it.
		/// </summary>
		private void Variable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (varName.SelectedItem == null)
				return;
			varValue.Text = Sdk.Server.OperandManager.Read("$V." + varName.SelectedItem) + "";
		}

		/// <summary>
		/// Writes the value entered in the text box back to AppVision.
		/// </summary>
		private void SetVariableValue(object sender, RoutedEventArgs e)
		{
			if (varName.SelectedItem == null)
				return;
			try
			{
				Sdk.Server.OperandManager.Write("$V." + varName.SelectedItem, varValue.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Set value");
			}
		}

		/// <summary>
		/// Demonstrates opening a second instance of this control in a new floating window
		/// using <see cref="Prysm.AppVision.SDK.IClient.Open"/>.
		/// The "file.dll#Namespace.Class" syntax loads the DLL from the Bin folder of the AppVision client.
		/// </summary>
		private void OpenNewWindow(object sender, RoutedEventArgs e)
		{
			// Sdk.Client is null when the control is not hosted by the AppVision client (designer, custom application)
			Sdk.Client?.Open("WpfControlSimple.dll#WpfControlSimple.UserControl1", "Size=320,240");
		}
	}
}
