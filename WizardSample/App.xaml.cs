using Prysm.AppVision.Common;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace WizardSample
{
	public partial class App : Application
    {
        static App() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

		// Probes the AppVision assemblies (appSDK.dll, appCommon.dll, Telerik...) one and two folders up.
		// Needed when the wizard is deployed in a sub-folder of the AppVision 'Bin' folder,
		// e.g. Bin\Drivers\MyDriver\appMyDriverWizard.exe, without copying the AppVision DLLs next to it.
		// Do not reference AppVision types in this method: it runs before they can be loaded.
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var fileName = new AssemblyName(args.Name).Name + ".dll";
			var dir = AppDomain.CurrentDomain.BaseDirectory;
			foreach (var candidate in new[] { Path.Combine(dir, "..", fileName), Path.Combine(dir, "..", "..", fileName) })
			{
				var path = Path.GetFullPath(candidate);
				if (File.Exists(path))
					return Assembly.LoadFrom(path);
			}
			return null;
		}

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += Domain_UnhandledException;
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Helper.Log("Error: " + e.Exception);
            MessageBox.Show(e.Exception.GetLastInnerException().Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Helper.Log("Error: " + e.ExceptionObject);
            MessageBox.Show(ex?.GetLastInnerException().Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
