using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OverlayManager
{
	public partial class App : Application
	{
        public App()
        {
            App.Current.DispatcherUnhandledException += (s, e) =>
            {
                e.Handled = true;
                MessageBox.Show(e.Exception.Message);
            };
        }
    }
}
