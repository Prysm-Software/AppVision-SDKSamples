using Prysm.AppVision.Data;
using Prysm.AppVision.SDK.Widgets;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WidgetLibrary;

/// <summary>
/// Displays the live value of a variable, with a title and a unit.
/// <code>&lt;wl:ValueTile xmlns:wl="clr-namespace:WidgetLibrary;assembly=WidgetLibrary" Title="Room" Variable="Building.Room1.Temperature" Unit="°C" Format="0.0" /&gt;</code>
/// </summary>
/// <remarks>
/// Two toolbox entries for the same type: the second one presets <see cref="Unit"/> and <see cref="Format"/>
/// through <c>Properties</c>, so the operator gets a temperature tile ready to use.
/// </remarks>
[AppWidget("Sample widgets", "Value tile", Order = 10, Width = 220, Height = 110,
	IconData = "F0 M3,5 H21 V19 H3 Z M5,7 V17 H19 V7 Z M7,9 H13 V11 H7 Z M7,13 H17 V15 H7 Z",
	Description = "Live value of a variable, with a title and a unit")]
[AppWidget("Sample widgets", "Temperature tile", Order = 11, Width = 220, Height = 110,
	IconData = "M10,3 A2,2 0 0 1 14,3 V13.5 A4,4 0 1 1 10,13.5 Z",
	Description = "Value tile preset for a temperature in °C", Properties = "Unit=°C;Format=0.0")]
public class ValueTile : UserControl
{
	readonly TextBlock title = new() { FontSize = 12, Opacity = 0.7 };
	readonly TextBlock value = new() { FontSize = 32, FontWeight = FontWeights.SemiBold, Text = "--" };
	readonly TextBlock unit = new() { FontSize = 14, Margin = new(6, 0, 0, 6), VerticalAlignment = VerticalAlignment.Bottom };

	/// <summary>A public parameterless constructor is required: the editor and the client create the widget with it.</summary>
	public ValueTile()
	{
		Foreground = Brushes.White;
		Content = new Border
		{
			Background = new SolidColorBrush(Color.FromRgb(0x15, 0x32, 0x46)),
			CornerRadius = new(6),
			Padding = new(12, 8, 12, 8),
			Child = new StackPanel { Children = { title, new StackPanel { Orientation = Orientation.Horizontal, Children = { value, unit } } } },
		};
		Loaded += (_, _) => (title.Text, unit.Text) = (Title, Unit);
		_ = new LiveVariable(this, () => Variable, Show);
	}

	// Plain CLR properties: written as XAML attributes in the synoptic, set at creation by the toolbox (Properties),
	// and shown by the configurator in the « Other » section because their category is "AppVision".

	/// <summary>Variable to display, e.g. <c>Building.Room1.Temperature</c> (the <c>$V.</c> prefix is optional).</summary>
	[Category("AppVision"), Description("Variable to display (e.g. Building.Room1.Temperature)")]
	public string Variable { get; set; }

	/// <summary>Title above the value.</summary>
	[Category("AppVision"), Description("Title above the value")]
	public string Title { get; set; }

	/// <summary>Unit after the value.</summary>
	[Category("AppVision"), Description("Unit after the value")]
	public string Unit { get; set; }

	/// <summary>.NET number format, e.g. <c>0.0</c>; empty: the value as received.</summary>
	[Category("AppVision"), Description(".NET number format, e.g. 0.0 (empty: value as received)")]
	public string Format { get; set; }

	void Show(VariableState state) => value.Text = state?.Value switch
	{
		null => "--",
		IFormattable number when !string.IsNullOrEmpty(Format) => number.ToString(Format, CultureInfo.CurrentCulture),
		var v => Convert.ToString(v, CultureInfo.CurrentCulture),
	};
}
