# MapProvider — Custom GIS Map Provider

## Overview

A custom map provider for AppVision geographical synoptics that uses
**Microsoft Bing Maps** as its tile source.

The provider implements `IConfigurable` so the AppVision Configurator displays
a settings panel (API key, map mode, label visibility). Settings are serialised
as a semicolon-separated string and persisted in the synoptic definition.

> Use this as a template for any tile-based map service:
> OpenStreetMap, ArcGIS, HERE, Mapbox, etc.

**References:** `appCommon.dll`, `appSDK.dll`, `Telerik.Windows.Controls.Map`  
**Output:** `MapProvider.dll` → `AppVision 4\Bin\`

---

## Project Structure

| File | Role |
|---|---|
| `BingMapsProvider.cs` | `IConfigurable` + `BingRestMapProvider` subclass |
| `Settings.xaml.cs` | WPF settings panel (DataContext = `BingMapsProvider`) |

---

## Key Concepts

### 1. `BingRestMapProvider` (Telerik)

Inherit from `BingRestMapProvider` to get all Bing Maps tile fetching for free.
Override `ApplicationId`, `Mode`, and `IsLabelVisible` to expose settings.

### 2. `IConfigurable` (`appSDK.dll`)

AppVision calls these methods to show, save, and restore provider settings:

```csharp
UIElement GetUI();        // return the WPF settings panel
string    GetConfig();    // serialise current settings
void      SetConfig(s);   // restore from serialised string
```

### 3. Parameter string format

```csharp
// Serialise
public string GetConfig()
    => $"mode={Mode};labels={IsLabelVisible};appid={ApplicationId}";

// Restore
public void SetConfig(string s)
{
    ApplicationId  = Helper.GetParameter("appid",   s);
    Mode           = Helper.GetParameter("mode",    s).ToEnum(MapMode.Road);
    IsLabelVisible = Helper.GetParameter("labels",  s, true);
}
```

### 4. Bing Maps API key

A valid Bing Maps key is required for production use.  
Obtain one at: <https://docs.microsoft.com/en-us/bingmaps/getting-started/bing-maps-dev-center-help/getting-a-bing-maps-key>

---

## AppVision SDK Reference

### `IConfigurable` (`appSDK.dll`)

| Member | Description |
|---|---|
| `.GetUI()` | Return WPF `UIElement` for the settings panel |
| `.GetConfig()` | Serialise settings to a string |
| `.SetConfig(s)` | Restore settings from a string |

### `Helper` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.GetParameter(key, text)` | Extract `key=value` from a text block |
| `.GetParameter(key, text, default)` | With fallback default |

### `StringExtensions` (`appCommon.dll`)

| Member | Description |
|---|---|
| `.ToEnum<T>(defaultValue)` | Parse a string to an enum value |

---

## Configuration in AppVision

1. Copy `MapProvider.dll` → `AppVision 4\Bin\`.  
2. In the Configurator:

```
Synoptics › [synoptic] › Background › Map provider
    Select: MapProvider.dll#MapProvider.BingMapsProvider
    Enter API key and desired map mode in the settings panel.
```

---

## How to Build and Deploy

1. Build in **Release** configuration.  
2. Copy `MapProvider.dll` → `AppVision 4\Bin\`.  
3. Restart the Configurator to pick up the new provider.

---

## See Also

- [DriverOverlay](../DriverOverlay/README.md) — push geo-referenced overlay icons onto a map synoptic
- [OverlaySample](../OverlaySample/README.md) — manage overlays at runtime
