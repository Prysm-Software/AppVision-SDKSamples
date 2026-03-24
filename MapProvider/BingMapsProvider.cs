using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System.Windows;
using Telerik.Windows.Controls.Map;

namespace MapProvider
{
	// Note: Bing Maps APIs require a key. To get your Bing Maps Key, go to
	// https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key
	// Test ApplicationId: AphuhxyOA-aUOBefo0bXJjSnu6bKbf1C_pS9_lDzpkzGdriHWrUknWXmlJOL7sjA

	/// <summary>
	/// Custom Bing Maps tile provider for AppVision GIS synoptics.
	/// Extends <see cref="BingRestMapProvider"/> and implements <see cref="IConfigurable"/>
	/// so AppVision can persist and restore the map mode, label visibility and API key.
	/// </summary>
	public class BingMapsProvider : BingRestMapProvider, IConfigurable
	{
		/// <summary>
		/// Returns the settings panel shown in the AppVision map-provider configuration dialog.
		/// </summary>
		public UIElement GetUI() => new Settings { DataContext = this };

		/// <summary>
		/// Serialises the current provider configuration to a semicolon-separated parameter string
		/// that AppVision stores in the synoptic definition.
		/// </summary>
		public string GetConfig() => $"mode={Mode};labels={IsLabelVisible};appid={ApplicationId}";

		/// <summary>
		/// Restores provider settings from the parameter string previously saved by <see cref="GetConfig"/>.
		/// </summary>
		/// <param name="parameters">Semicolon-separated key=value pairs, e.g. <c>mode=Road;labels=True;appid=...</c></param>
		public void SetConfig(string parameters)
		{
			// Replace semicolons with newlines so Helper.GetParameter can parse key=value pairs.
			var lines = parameters.Replace(';', '\n');
			ApplicationId    = Helper.GetParameter("appid",  lines);
			Mode             = Helper.GetParameter("mode",   lines).ToEnum(MapMode.Road);
			IsLabelVisible   = Helper.GetParameter("labels", lines, true);
		}
	}
}
