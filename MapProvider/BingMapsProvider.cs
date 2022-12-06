using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System.Windows;
using Telerik.Windows.Controls.Map;

namespace MapProvider
{
    // Note: Bing Maps APIs require a key.To get your Bing Maps Key, go to
    // https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key
    // Test ApplicationId: AphuhxyOA-aUOBefo0bXJjSnu6bKbf1C_pS9_lDzpkzGdriHWrUknWXmlJOL7sjA
    public class BingMapsProvider : BingRestMapProvider, IConfigurable
	{
		public UIElement GetUI() => new Settings { DataContext = this };

		public string GetConfig() => $"mode={Mode};labels={IsLabelVisible};appid={ApplicationId}";

		public void SetConfig(string parameters)
		{
			var lines = parameters.Replace(';', '\n');
			ApplicationId = Helper.GetParameter("appid", lines);
			Mode = Helper.GetParameter("mode", lines).ToEnum(MapMode.Road);
			IsLabelVisible = Helper.GetParameter("labels", lines, true);
		}
	}
}
