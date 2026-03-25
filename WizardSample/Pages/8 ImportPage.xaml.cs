using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 8 — Import progress page.
    /// Shows a progress bar and live log bound to <see cref="ViewModel.Progress"/>,
    /// <see cref="ViewModel.IsBusy"/> and <see cref="ViewModel.Details"/>.
    /// A <b>Cancel</b> button is bound to <see cref="ViewModel.CancelImportCommand"/>.
    /// </summary>
    public partial class ImportPage : UserControl
    {
        public ImportPage()
        {
            InitializeComponent();
        }
    }
}
