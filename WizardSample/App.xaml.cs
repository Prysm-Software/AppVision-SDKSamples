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
