using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 7 — Summary page.
    /// Displays the import plan from <see cref="ViewModel.Details"/> so the operator can
    /// review it before committing changes to the AppVision configuration.
    /// Forwards to <see cref="ViewModel.ImportCommand"/> on <b>Import</b>.
    /// </summary>
    public partial class SummaryPage : UserControl
    {
        public SummaryPage()
        {
            InitializeComponent();
        }
    }
}
