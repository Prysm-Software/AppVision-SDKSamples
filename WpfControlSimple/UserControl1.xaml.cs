using Prysm.AppVision.SDK;
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

namespace WpfControlSimple
{
	/// <summary>
	/// Embeddable synoptic control that lets an operator read and write AppVision
	/// variable values directly from a WPF UserControl placed on a synoptic page.
	/// Uses <see cref="Sdk.Server"/>'s <see cref="Prysm.AppVision.SDK.OperandManager"/>
	/// for single-call read/write without explicit subscriptions.
	/// </summary>
	public partial class UserControl1 : UserControl
	{
		/// <summary>
		/// Initialises the control and populates the variable name drop-down
		/// from the AppVision variable cache (when already connected).
		/// </summary>
		public UserControl1()
		{
			InitializeComponent();
			
			if (Sdk.Server.IsConnected)
				varName.ItemsSource = from var in Sdk.Server.VariableManager.GetStates() select var.Name;
		}

		/// <summary>
		/// Reads the current value of the selected variable from AppVision via
		/// <see cref="Prysm.AppVision.SDK.OperandManager.Read"/> and displays it.
		/// </summary>
		private void Variable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			varValue.Text = Sdk.Server.OperandManager.Read("$V." + varName.SelectedItem) + "";
		}

		/// <summary>
		/// Writes the value entered in the text box back to AppVision.
		/// </summary>
		private void SetVariableValue(object sender, RoutedEventArgs e)
		{
			Sdk.Server.OperandManager.Write("$V." + varName.SelectedItem, varValue.Text);
		}

		/// <summary>
		/// Demonstrates opening a second instance of this control in a new floating window
		/// using <see cref="Prysm.AppVision.SDK.IClient.Open"/>.
		/// </summary>
		private void OpenNewWindow(object sender, RoutedEventArgs e)
		{
			Sdk.Client.Open("WpfControlSimple.dll#WpfControlSimple.UserControl1", "Size=320,240");
		}
	}
}
