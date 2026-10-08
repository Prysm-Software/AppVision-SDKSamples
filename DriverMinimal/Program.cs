using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Linq;
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
		/// <summary>
		/// Set to <c>true</c> by the <c>Closed</c> / <c>Restart</c> events or Ctrl+C to exit the main loop.
		/// Written by other threads, hence <c>volatile</c>.
		/// </summary>
		static volatile bool _closing;

		/// <summary>
		/// Driver entry point.
		/// Connects to the AppVision server, subscribes to the variables of the protocol
		/// and spins in a loop until AppVision closes the driver.
		/// </summary>
		/// <param name="args">
		/// <c>protocolName[@hostname]</c>, as passed by the AppVision server when it starts the protocol
		/// (the server can append other arguments, such as <c>-password=...</c>, read by <c>Login</c>).
		/// Defaults to <c>DriverMinimal</c> on the local server when the argument is missing.
		/// </param>
		/// <returns>0 on a normal stop, 1 when the connection failed.</returns>
		static int Main(string[] args)
		{
			// Command line: DriverMinimal.exe protocolName[@hostname]
			var a = (args.Length > 0 ? args[0] : "DriverMinimal").Split('@');
			var protocolName = a[0];
			var hostname = (a.Length > 1) ? a[1] : "";      // "" = local server, default port

			// Ctrl+C: stop the loop and disconnect cleanly instead of killing the process.
			Console.CancelKeyPress += (s, e) => { e.Cancel = true; _closing = true; };

			try
			{
				Console.WriteLine($"Connecting to AppVision server '{hostname}' using protocol '{protocolName}'");

				// Create the driver connection handle and open a session on the server.
				_appServer = new AppServerForDriver();
				_appServer.Open(hostname);

				// Identify this process to AppVision by its protocol name.
				// The protocol must exist in Configurator > Protocols, otherwise Login throws.
				_appServer.Login(protocolName);

				// Register for lifecycle and data-change events.
				// Closed : AppVision stops the protocol (configurator, server shutdown, protocol restart).
				// Restart: the SDK lost the connection to the server. AppVision relaunches the protocols
				//          marked "Autostart", so the driver just exits.
				_appServer.ControllerManager.Closed += () => _closing = true;
				_appServer.ControllerManager.Restart += () => _closing = true;
				_appServer.VariableManager.StateChanged += VariableManager_StateChanged;

				// Subscribe to the children of the variables linked to this protocol
				// (variables whose 'Source' is $P.<protocolName>), e.g. "$V.MinimalNode.*".
				// Several masks are joined with '|' in ONE call: a new "$V." filter replaces the previous one.
				var nodes = _appServer.GetVariablesByProtocol();
				if (nodes.Length == 0)
					Console.WriteLine($"No variable has its Source set to $P.{protocolName}: nothing to monitor.");
				else
					_appServer.AddFilterNotifications(string.Join("|", nodes.Select(v => "$V." + v.Name + ".*")));

				// StartNotifications is required even without filter: AppVision also uses this channel
				// to send its commands (Close, Download, Refresh) to the driver.
				// false: do not receive the current values of the subscribed variables now.
				_appServer.StartNotifications(false);

				// Signal AppVision that the driver has finished its initialisation (protocol state "Synchronized").
				_appServer.ProtocolSynchronized();

				Console.WriteLine("Ready. Press Ctrl+C to stop.");

				while (!_closing)
				{
					// TODO: poll external hardware here and push state changes to AppVision:
					// _appServer.VariableManager.Set("MinimalNode.Value", 42);
					Thread.Sleep(100);
				}

				Console.WriteLine("Closing...");
				return 0;
			}
			catch (Exception ex)
			{
				// Typical causes: server not running or not reachable, unknown protocol name.
				Console.WriteLine("Error: " + ex.Message);
				Thread.Sleep(5000);     // leave time to read the message, then exit
				return 1;
			}
			finally
			{
				// Clean disconnect: logs the protocol out so that the server shows it stopped at once.
				if (_appServer?.IsConnected == true)
					_appServer.Close();
			}
		}

		/// <summary>
		/// Called by the SDK, on a background thread, each time a subscribed variable changes:
		/// commands written by operators or scripts, but also the values written by the driver itself.
		/// In a real driver, this is where commands are forwarded to the external system
		/// (see DriverAdvanced: filter "Variable.StateType=1" to receive the command variables only).
		/// </summary>
		/// <param name="state">New variable state containing name, value and timestamp.</param>
		private static void VariableManager_StateChanged(VariableState state)
		{
			Console.WriteLine($"{state.Date}: {state.Name} = {state.Value}");

			// TODO: forward the command to the external system or device.
		}
	}
}
