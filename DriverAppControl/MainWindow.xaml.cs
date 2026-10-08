using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DriverAppControl
{
	/// <summary>
	/// Main window for the AppControl driver process.
	/// Parses the command-line argument, starts the <see cref="Driver"/>, and displays a live
	/// trace log. The window can only be closed by the driver itself or by pressing Ctrl+Close.
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		#region Window_Loaded(),Window_Closing()
		/// <summary>
		/// Reads the command-line argument (<c>protocol_name[@hostname]</c>), creates the driver
		/// and starts the acquisition thread.
		/// </summary>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// check the command line
			string[] args = Environment.GetCommandLineArgs();
			if (args == null || args.Length < 2)
			{
				Trace("Usage: DriverAppControl.exe protocolName[@hostname]");
				return;
			}
			// start the driver
			_driver = new Driver(args[1]);
			_driver.TraceDelegate = Trace;
			_driver.ErrorDelegate = Error;
			_driver.Closed += _driver_Closed;
			_driver.Start();
		}
		/// <summary>
		/// Prevents accidental closure while the driver thread is running.
		/// Ctrl+Close is required to stop the driver.
		/// </summary>
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
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
		/// <summary>Appends a message to the trace list on the UI thread.</summary>
		private void Trace(string p)
		{
			if (Dispatcher.CheckAccess())
			{
				Debug.WriteLine(p);
				while (trace.Items.Count > 1000)		// the driver runs for months: keep the list bounded
					trace.Items.RemoveAt(0);
				p = DateTime.Now.ToString("HH:mm:ss.fff ") + p;
				trace.Items.Add(p);
				trace.ScrollIntoView(p);
			}
			else
				Dispatcher.BeginInvoke(new Action<string>(Trace), new object[] { p });
		}
		/// <summary>Prefixes the message with "ERROR: " and forwards it to <see cref="Trace"/>.</summary>
		private void Error(string p)
		{
			Trace( "ERROR: " + p);
		}
		#endregion

		Driver _driver;
	}
}
