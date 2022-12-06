using Prysm.AppVision.Common;
using Prysm.AppVision.Data;

namespace WizardSample
{
    public class Device : ObservableObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Icon { get; set; }

        // <todo> add more properties if necessary
        //public string OtherInfo { get; set; }


        // if this device already has a corresponding variable in AppVision configuration (to display a visual hint before import)
        public VariableRow VariableRow { get; set; }


    }
}
