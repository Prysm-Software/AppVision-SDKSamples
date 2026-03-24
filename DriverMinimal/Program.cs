using Prysm.AppVision.SDK;
using System;
using System.Threading;

namespace DriverMinimal
{
	/// <summary>
	/// Entry point for the minimal AppVision driver.
	/// Demonstrates the bare-minimum connection sequence required to
	/// register a driver process with the AppVision server.
	/// </summary>
	class Program
	{
		/// <summary>AppVision driver server connection handle.</summary>
		static AppServerForDriver _appServer;
		/// <summary>Set to <c>true</c> by the <c>Closed</c> event to exit the main loop gracefully.</summary>
		static bool _closing;

		/// <summary>
		/// Driver entry point.
		/// Connects to the AppVision server, subscribes to variable notifications
		/// and spins in a loop until AppVision closes the driver.
		/// </summary>
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Connecting to AppVision server using 'DriverMinimal' protocol");

				// Create the driver connection handle and open a socket to the server.
				// An empty string connects to localhost on the default port.
				_appServer = new AppServerForDriver();
				_appServer.Open("");				// server address ("" = localhost)

				// Identify this process to AppVision by its protocol name.
				// A protocol named "DriverMinimal" with its manager set to "DriverMinimal.exe"
				// must exist in the AppVision Configurator / Protocols.
				_appServer.Login("DriverMinimal");  

				// Register for lifecycle and data-change events.
				_appServer.ControllerManager.Closed += () => _closing = true;
				_appServer.VariableManager.StateChanged += VariableManager_StateChanged;
				// Subscribe to all variable changes ("$V.*" = every variable in the system).
				_appServer.AddFilterNotifications("$V.*");
				_appServer.StartNotifications(false);
				// Signal AppVision that the driver has finished its initialisation.
				_appServer.ProtocolSynchronized();

				Console.WriteLine("Ready");

				while (!_closing)
				{
					// TODO: poll external hardware here and push state changes to AppVision:
					// _appServer.VariableManager.Set("Some.Variable.Name", "some value");
					Thread.Sleep(100);
				}

				Console.WriteLine("Closing in 2s...");
				Thread.Sleep(2000);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.ReadKey();
			}
		}

		/// <summary>
		/// Called by AppVision whenever an output variable value changes.
		/// In a real driver, this is where commands should be forwarded to
		/// the external hardware or system.
		/// </summary>
		/// <param name="state">New variable state containing name, value and timestamp.</param>
		private static void VariableManager_StateChanged(Prysm.AppVision.Data.VariableState state)
		{
			Console.WriteLine($"{state.Date}: {state.Name} = {state.Value}");

			// TODO: forward the command to the external system or device.
		}
	}
}
