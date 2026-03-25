using System.Windows.Controls;

namespace WizardSample
{
    /// <summary>
    /// Wizard step 1 — Welcome page.
    /// Introduces the wizard and offers a single <b>Next</b> button bound to
    /// <see cref="ViewModel.AppLoginCommand"/>.
    /// </summary>
    public partial class WelcomePage : UserControl
    {
        public WelcomePage()
        {
            InitializeComponent();
        }
    }
}
