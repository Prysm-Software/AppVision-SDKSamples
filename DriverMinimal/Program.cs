using Prysm.AppVision.SDK;
using System;
using System.Threading;

namespace DriverMinimal
{
	class Program
	{
		static AppServerForDriver _appServer;
		static bool _closing;

		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Connecting to AppVision server using 'DriverMinimal' protocol");

				// Connect to AppVision server
				_appServer = new AppServerForDriver();
				_appServer.Open("");				// server address ("" = localhost)
				
				// Log in using the protocol name
				// note: a protocol named "DriverMinimal" and its manager set to "DriverMinimal.exe" must be present in the configuration
				_appServer.Login("DriverMinimal");  

				// Register for AppVision events
				_appServer.ControllerManager.Closed += () => _closing = true;
				_appServer.VariableManager.StateChanged += VariableManager_StateChanged;
				_appServer.AddFilterNotifications("$V.*");  // Subscribe to all variable changes
				_appServer.StartNotifications(false);
				_appServer.ProtocolSynchronized();

				Console.WriteLine("Ready");

				while (!_closing)
				{
					// todo monitor external systems or device and reflect its state in AppVision server using variables
					// ex: _appServer.VariableManager.Set("Some.Variable.Name", "some value");
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

		// Some variable has changed in AppVision
		private static void VariableManager_StateChanged(Prysm.AppVision.Data.VariableState state)
		{
			Console.WriteLine($"{state.Date}: {state.Name} = {state.Value}");

			// todo transfert the command to external system or device
		}
	}
}
