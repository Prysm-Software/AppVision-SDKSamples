using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DriverAppControl
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		#region Window_Loaded(),Window_Closing()
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// check the command line
			string[] args = Environment.GetCommandLineArgs();
			if (args == null || args.Length < 2)
			{
				Trace("Usage: AppDriverXXX protocol_name");
				return;
			}
			// start the driver
			_driver = new Driver(args[1]);
			_driver.TraceDelegate = Trace;
			_driver.ErrorDelegate = Error;
			_driver.Closed += _driver_Closed;
			_driver.Start();
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.OnActivated(null);
			if( _driver!=null )
			if (_driver.IsAlive)			// thread driver alive?
			{
				if( (Keyboard.Modifiers & ModifierKeys.Control)==0 )
					MessageBox.Show("Stop by user forbidden");
				else
					_driver.Stop();
				e.Cancel = true;
			}
		}
		#endregion

		/// <summary>
		/// Driver stop
		/// </summary>
		void _driver_Closed()
		{
			Dispatcher.BeginInvoke(new Action(Close));
		}

		#region Trace(),Error()
		private void Trace(string p)
		{
			if (Dispatcher.CheckAccess())
			{
				Debug.WriteLine(p);
				trace.Items.Add(p);
				trace.ScrollIntoView(p);
			}
			else
				Dispatcher.BeginInvoke(new Action<string>(Trace), new object[] { p });
		}
		private void Error(string p)
		{
			Trace( "ERROR: " + p);
		}
		#endregion

		Driver _driver;
	}
}
