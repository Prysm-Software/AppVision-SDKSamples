using Prysm.AppVision.Data;
using Prysm.AppVision.SDK.Widgets;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WidgetLibrary;

/// <summary>
/// Round lamp that lights up when a variable is on (any non-zero value), with an optional label.
/// <code>&lt;wl:StateLamp xmlns:wl="clr-namespace:WidgetLibrary;assembly=WidgetLibrary" Variable="Building.Door1.Open" OnColor="Orange" Label="Door 1" /&gt;</code>
/// </summary>
[AppWidget("Sample widgets", "State lamp", Order = 20, Width = 120, Height = 40,
	IconData = "F0 M2,12 A10,10 0 1 1 22,12 A10,10 0 1 1 2,12 Z M6,12 A6,6 0 1 0 18,12 A6,6 0 1 0 6,12 Z",
	Description = "Lights up when a variable is on (non-zero)")]
public class StateLamp : UserControl
{
	readonly Ellipse lamp = new() { Width = 24, Height = 24, Stroke = Brushes.Gray, StrokeThickness = 1 };
	readonly TextBlock label = new() { Margin = new(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
	bool on;

	public StateLamp()
	{
		Foreground = Brushes.White;
		Content = new StackPanel { Orientation = Orientation.Horizontal, Children = { lamp, label } };
		Loaded += (_, _) => { label.Text = Label; Paint(); };
		_ = new LiveVariable(this, () => Variable, Show);
	}

	/// <summary>Logical or numeric variable: the lamp is on for any non-zero value.</summary>
	[Category("AppVision"), Description("Variable followed by the lamp (on for any non-zero value)")]
	public string Variable { get; set; }

	/// <summary>Text next to the lamp.</summary>
	[Category("AppVision"), Description("Text next to the lamp")]
	public string Label { get; set; }

	/// <summary>Colour when on, as a name or #RRGGBB (default LimeGreen).</summary>
	[Category("AppVision"), Description("Colour when on: name or #RRGGBB")]
	public string OnColor { get; set; } = "LimeGreen";

	/// <summary>Colour when off (default DimGray).</summary>
	[Category("AppVision"), Description("Colour when off: name or #RRGGBB")]
	public string OffColor { get; set; } = "DimGray";

	void Show(VariableState state)
	{
		on = state?.Value switch
		{
			null => false,
			bool b => b,
			var v => double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) && d != 0,
		};
		Paint();
	}

	void Paint() => lamp.Fill = Brush(on ? OnColor : OffColor);

	static Brush Brush(string color)
	{
		try { return (Brush)new BrushConverter().ConvertFromInvariantString(color); }
		catch (FormatException) { return Brushes.Magenta; }   // visible on purpose: the colour is misspelled
	}
}
