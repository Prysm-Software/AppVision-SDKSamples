using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WizardSample
{
    public partial class App : Application
    {
        static App() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        // probing assemblies in the parent folder
		// to be used if your driver is deployed in a sub-directory of the 'bin' folder (i.e. bin\MyDriver\my_driver.exe)
        static System.Reflection.Assembly LookForAssemblyInParentDir(object sender, ResolveEventArgs a)
        {
            Console.WriteLine(a.Name);
            var dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var ass = $@"{dir}\..\{a.Name.Split(',')[0]}.dll";
            return File.Exists(ass) ? Assembly.LoadFrom(ass) : null;
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
