using System.Windows;
using System.Windows.Controls;

namespace MapProvider
{
	/// <summary>Settings panel of <see cref="TileServerProvider"/>, bound to the provider.</summary>
	public partial class Settings : UserControl
	{
		public Settings()
		{
			InitializeComponent();
		}

		TileServerProvider Provider => (TileServerProvider)DataContext;

		void OpenStreetMap_Click(object sender, RoutedEventArgs e) => Preset(TileServerProvider.OpenStreetMap, 19);

		void OpenTopoMap_Click(object sender, RoutedEventArgs e) => Preset("https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png", 17);

		void Preset(string url, int maxZoom)
		{
			Provider.UrlTemplate = url;     // the provider notifies the change: the text boxes follow
			Provider.MaxZoom = maxZoom;
		}
	}
}
