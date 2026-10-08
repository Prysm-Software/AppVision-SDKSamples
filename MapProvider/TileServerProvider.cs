using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using Telerik.Windows.Controls.Map;

namespace MapProvider
{
	/// <summary>
	/// Map background for AppVision GIS synoptics, read from any "XYZ" tile server: OpenStreetMap, a commercial
	/// service, or a tile server hosted on the site (no Internet access needed).
	/// </summary>
	/// <remarks>
	/// A RadMap provider is a <see cref="TiledProvider"/> holding one or more <see cref="TiledMapSource"/>:
	/// the provider gives the projection, the source gives the address of each 256 x 256 tile.
	/// <para>
	/// The synoptic stores <c>Geo:Custom:MapProvider.dll</c> in its <c>Fill</c> property: AppVision creates the
	/// first public <see cref="MapProviderBase"/> type of the DLL, then calls <see cref="SetConfig"/> with the
	/// synoptic <c>MapParameters</c>. Implementing <see cref="IConfigurable"/> gives the configurator a settings panel.
	/// </para>
	/// </remarks>
	public class TileServerProvider : TiledProvider, IConfigurable, INotifyPropertyChanged
	{
		/// <summary>OpenStreetMap standard tiles: free, but subject to the OSM tile usage policy (light use only).</summary>
		public const string OpenStreetMap = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

		readonly MercatorProjection _projection = new MercatorProjection();
		TileServerSource _source;
		string _urlTemplate = OpenStreetMap;
		int _maxZoom = 19;
		string _cacheFolder = "";

		/// <summary>
		/// Address of a tile: <c>{z}</c> zoom level, <c>{x}</c> column, <c>{y}</c> row (from the top),
		/// <c>{s}</c> sub-domain (a, b or c), e.g. <c>https://{s}.tile.example.com/{z}/{x}/{y}.png</c>.
		/// An API key is part of the address: <c>...{y}.png?key=XXXX</c>.
		/// </summary>
		public string UrlTemplate { get => _urlTemplate; set => Set(ref _urlTemplate, value); }

		/// <summary>Highest zoom level served by the tile server (19 for OpenStreetMap).</summary>
		public int MaxZoom { get => _maxZoom; set => Set(ref _maxZoom, value); }

		/// <summary>
		/// Optional folder where the downloaded tiles are kept: the map stays available when the tile server
		/// does not answer, and the server is asked less often. Empty: no cache.
		/// </summary>
		public string CacheFolder { get => _cacheFolder; set => Set(ref _cacheFolder, value); }

		/// <summary>Projection of the tiles: Web Mercator, the one of every XYZ tile server.</summary>
		public override ISpatialReference SpatialReference => _projection;

		public TileServerProvider()
		{
			ApplySettings();
		}

		/// <summary>(Re)creates the tile source from the settings: the map reloads its tiles.</summary>
		void ApplySettings()
		{
			if (_source != null)
				MapSources.Remove(_source.UniqueId);

			_source = new TileServerSource(UrlTemplate, Math.Max(1, MaxZoom));
			// Tile servers ask to be identified (mandatory for OpenStreetMap).
			_source.WebHeaders.Add(HttpRequestHeader.UserAgent, $"{Helper.ProductName ?? "AppVision"} MapProvider sample");
			MapSources.Add(_source.UniqueId, _source);

			if (!string.IsNullOrWhiteSpace(CacheFolder))
			{
				// Telerik writes each downloaded tile in this folder and reads it back before downloading it again.
				CacheStorage = new FileSystemCache(Environment.ExpandEnvironmentVariables(CacheFolder));
				IsTileCachingEnabled = true;
			}
			else
				IsTileCachingEnabled = false;

			// Gives the source the cache and the credentials (Username/Password of the map window) of the provider.
			InheritCacheStorage(_source);
			InheritRequestCredentials(_source);
		}

		#region IConfigurable

		/// <summary>Settings panel shown in the map window of the configurator.</summary>
		public UIElement GetUI() => new Settings { DataContext = this };

		/// <summary>
		/// Settings saved in the synoptic (<c>MapParameters</c>), <c>key=value</c> pairs separated by <c>;</c>.
		/// Not by line breaks: when the synoptic is loaded, XAML turns them into spaces. The configurator writes
		/// these settings after its own <c>Username=</c> and <c>Password=</c> lines, hence the leading <c>;</c>.
		/// </summary>
		public string GetConfig() => $";Url={UrlTemplate};MaxZoom={MaxZoom};Cache={CacheFolder}";

		/// <summary>Restores the settings saved by <see cref="GetConfig"/> (null when the synoptic has none).</summary>
		public void SetConfig(string parameters)
		{
			var lines = (parameters ?? "").Replace(';', '\n');
			_urlTemplate = Helper.GetParameter("Url", lines, OpenStreetMap).Trim();
			_maxZoom = Helper.GetParameter("MaxZoom", lines, 19);
			_cacheFolder = Helper.GetParameter("Cache", lines, "").Trim();

			var user = Helper.GetParameter("Username", lines, "");
			RequestCredentials = string.IsNullOrEmpty(user) ? null : new NetworkCredential(user, Helper.GetParameter("Password", lines, ""));

			ApplySettings();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
		}

		#endregion

		/// <summary>Raised when a setting changes: the settings panel and the preview map follow.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		void Set<T>(ref T field, T value, [CallerMemberName] string name = null)
		{
			if (Equals(field, value))
				return;
			field = value;
			ApplySettings();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	/// <summary>Tile source: turns a tile position into the address of its image.</summary>
	public class TileServerSource : TiledMapSource
	{
		static readonly string[] SubDomains = { "a", "b", "c" };
		readonly string _urlTemplate;
		readonly string _cacheKey;

		public TileServerSource(string urlTemplate, int maxZoom)
			: base(1, maxZoom, 256, 256)
		{
			_urlTemplate = urlTemplate;
			// cached tiles are named after the server, so that two servers do not share their tiles
			_cacheKey = Uri.TryCreate(Format(0, 0, 0), UriKind.Absolute, out var uri) ? uri.Host : "tiles";
		}

		public override void Initialize() => RaiseInitializeCompleted();

		/// <summary>Called by RadMap for each tile to display.</summary>
		/// <param name="tileLevel">Tile level of RadMap, converted to the zoom level of the tile server.</param>
		protected override Uri GetTile(int tileLevel, int tilePositionX, int tilePositionY) =>
			string.IsNullOrWhiteSpace(_urlTemplate) ? null
				: new Uri(Format(ConvertTileToZoomLevel(tileLevel), tilePositionX, tilePositionY));

		/// <summary>Name of a tile in the cache folder (stable from one session to the next).</summary>
		protected override string GetCachedTileName(int tileLevel, int tilePositionX, int tilePositionY) =>
			$"{_cacheKey}_{ConvertTileToZoomLevel(tileLevel)}_{tilePositionX}_{tilePositionY}";

		string Format(int zoom, int x, int y) => _urlTemplate
			.Replace("{z}", zoom.ToString(CultureInfo.InvariantCulture))
			.Replace("{x}", x.ToString(CultureInfo.InvariantCulture))
			.Replace("{y}", y.ToString(CultureInfo.InvariantCulture))
			.Replace("{s}", SubDomains[Math.Abs(x + y) % SubDomains.Length]);
	}
}
