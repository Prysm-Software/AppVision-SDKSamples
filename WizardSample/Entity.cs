using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using System.Windows.Media;

namespace WizardSample
{
    public class Entity : ObservableObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }

        // <todo> add more properties if necessary
        //public string OtherInfo { get; set; }


        // if this entity already has a corresponding variable in AppVision configuration (to display a visual hint before import)
        public VariableRow VariableRow { get; set; }


    }
}
