MapProvider - Custom GIS Map Provider Sample
============================================

OVERVIEW
--------
A custom map provider for AppVision geographical synoptics that uses
Microsoft Bing Maps as its data source.

The provider is a class library that implements the IConfigurable interface
so that AppVision's configurator can display a settings panel to the operator
(API key, map mode, label visibility). The settings are serialised as a
semicolon-separated parameter string and persisted in the synoptic definition.

Use this as a template when integrating any tile-based map service
(OpenStreetMap, ArcGIS, HERE, etc.) as an AppVision synoptic background.

This C# project references:
  appCommon.dll       (Prysm.AppVision.Common)  - Helper.GetParameter, ToEnum
  appSDK.dll          (Prysm.AppVision.SDK)      - IConfigurable
  Telerik.Windows.Controls.Map                   - BingRestMapProvider, MapMode

The output 'MapProvider.dll' is copied into 'AppVision 4\Bin\' folder.


PROJECT STRUCTURE
-----------------
  BingMapsProvider.cs   IConfigurable + BingRestMapProvider subclass.
  Settings.xaml.cs      WPF settings panel (DataContext = BingMapsProvider).


KEY CONCEPTS
------------
1. BingRestMapProvider  (Telerik.Windows.Controls.Map)
   Inherit from BingRestMapProvider to get all Bing Maps tile fetching for free.
   Override ApplicationId, Mode, IsLabelVisible to expose settings.

2. IConfigurable  (Prysm.AppVision.SDK)
   AppVision calls these two methods to show/save/restore provider settings:
       GetUI()        Return a UIElement (WPF settings panel).
       GetConfig()    Serialise current settings to a string.
       SetConfig(s)   Restore settings from the serialised string.

3. Parameter string format
   Use semicolons as separators and Helper.GetParameter to parse:
       GetConfig(): "mode=Road;labels=True;appid=YOUR_KEY"
       SetConfig(): ApplicationId = Helper.GetParameter("appid", lines);
                    Mode          = Helper.GetParameter("mode", lines).ToEnum(MapMode.Road);
                    IsLabelVisible = Helper.GetParameter("labels", lines, true);

4. Bing Maps API key
   A valid Bing Maps key is required for production use.
   To obtain a key:
       https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key
   For testing you can use the key provided in the source comments.


APPVISION SDK REFERENCE
-----------------------
IConfigurable  (appSDK.dll)
  .GetUI()              Return WPF UIElement for the settings panel.
  .GetConfig()          Serialise settings to a string.
  .SetConfig(params)    Restore settings from a string.

Helper  (appCommon.dll)
  .GetParameter(key, text)           Extract a key=value from a block of text.
  .GetParameter(key, text, default)  With fallback default value.

StringExtensions  (appCommon.dll)
  .ToEnum<T>(defaultValue)           Parse a string to an enum value.


CONFIGURATION IN APPVISION
---------------------------
1. Copy MapProvider.dll to:  AppVision 4\Bin\

2. In the Configurator:
       Synoptics > [your synoptic] > Background > Map provider
           Select: MapProvider.dll#MapProvider.BingMapsProvider
           Enter API key and desired map mode in the settings panel.

3. The synoptic will display a live Bing Maps tile background when opened
   in the AppVision client.


HOW TO BUILD AND DEPLOY
-----------------------
1. Build in Release configuration.
2. Copy MapProvider.dll to:  AppVision 4\Bin\
3. Restart the Configurator to pick up the new provider.


SEE ALSO
--------
DriverOverlay   - Push geo-referenced overlay icons onto a map synoptic.
OverlaySample   - Manage overlays at runtime from a standalone tool.
