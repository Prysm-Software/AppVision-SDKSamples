using Prysm.AppVision.Common;
using Prysm.AppVision.Data;

namespace WizardSample
{
    /// <summary>
    /// Data model for a discovered network device.
    /// Inherits <see cref="ObservableObject"/> so WPF bindings are notified of property changes.
    /// After import, each device becomes a node variable in the AppVision configuration.
    /// </summary>
    public class Device : ObservableObject
    {
        /// <summary>Gets or sets the device hostname or identifier (used as the AppVision variable name after sanitising).</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a human-readable description shown in the wizard and AppVision configurator.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the network address (IP or hostname) used to connect to the device.</summary>
        public string Address { get; set; }

        /// <summary>Gets or sets the relative path of the icon displayed in the discovery grid.</summary>
        public string Icon { get; set; }

        // <todo> add more properties if necessary
        //public string OtherInfo { get; set; }


        /// <summary>
        /// Gets or sets the existing AppVision variable for this device when one already exists,
        /// or <see langword="null"/> if the device has not yet been imported.
        /// Used to display a visual hint in the wizard before the import step.
        /// </summary>
        public VariableRow VariableRow { get; set; }


    }
}
