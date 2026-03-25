using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using System.Windows.Media;

namespace WizardSample
{
    /// <summary>
    /// Data model for a single entity (sub-item) belonging to a discovered device.
    /// Each selected entity becomes one or more child variables under the device node
    /// in the AppVision configuration after the import step.
    /// Inherits <see cref="ObservableObject"/> so WPF list bindings update automatically.
    /// </summary>
    public class Entity : ObservableObject
    {
        /// <summary>Gets or sets the entity identifier (used as the AppVision child variable name suffix).</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a human-readable description shown in the entity grid.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the icon displayed in the entity selection grid.</summary>
        public ImageSource Icon { get; set; }

        // <todo> add more properties if necessary
        //public string OtherInfo { get; set; }

        /// <summary>
        /// Gets or sets the existing AppVision variable for this entity when one already exists,
        /// or <see langword="null"/> if the entity has not yet been imported.
        /// Used to display a visual hint in the wizard before the import step.
        /// </summary>
        public VariableRow VariableRow { get; set; }
    }
}
