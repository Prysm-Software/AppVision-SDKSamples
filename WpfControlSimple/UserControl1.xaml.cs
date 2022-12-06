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
	public partial class UserControl1 : UserControl
	{
		public UserControl1()
		{
			InitializeComponent();
			
			if (Sdk.Server.IsConnected)
				varName.ItemsSource = from var in Sdk.Server.VariableManager.GetStates() select var.Name;
		}

		// server interaction: get/set a variable value
		private void Variable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			varValue.Text = Sdk.Server.OperandManager.Read("$V." + varName.SelectedItem) + "";
		}

		private void SetVariableValue(object sender, RoutedEventArgs e)
		{
			Sdk.Server.OperandManager.Write("$V." + varName.SelectedItem, varValue.Text);
		}

		// client interaction: open this custom control in a new window
		private void OpenNewWindow(object sender, RoutedEventArgs e)
		{
			Sdk.Client.Open("WpfControlSimple.dll#WpfControlSimple.UserControl1", "Size=320,240");
		}
	}
}
