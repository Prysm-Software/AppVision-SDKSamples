using Prysm.AppVision.Common;
using WizardSample;
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

namespace WizardSample
{
    /// <summary>
    /// Shell window for the WizardSample application.
    /// Sets the title to the AppVision product name and version, and binds the <see cref="ViewModel"/>
    /// as the root <see cref="Window.DataContext"/>.
    /// All wizard page transitions are controlled by <see cref="ViewModel.CurrentPage"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Title = Helper.ProductName + " v" + Helper.ProductVersion;
            DataContext = new ViewModel();
            InitializeComponent();
        }
    }
}
