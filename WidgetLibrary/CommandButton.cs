using Prysm.AppVision.SDK;
using Prysm.AppVision.SDK.Widgets;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WidgetLibrary;

/// <summary>
/// Button that writes a fixed value into a variable, e.g. to start a scenario or switch a light on.
/// <code>&lt;wl:CommandButton xmlns:wl="clr-namespace:WidgetLibrary;assembly=WidgetLibrary" Text="Lights on" Variable="Building.Lights" Value="1" /&gt;</code>
/// </summary>
[AppWidget("Sample widgets", "Command button", Order = 30, Width = 140, Height = 36,
	IconData = "F0 M2,6 H22 V18 H2 Z M4,8 V16 H20 V8 Z M8,11 H16 V13 H8 Z",
	Description = "Writes a fixed value into a variable on click")]
public class CommandButton : Button
{
	public CommandButton() => Loaded += (_, _) => Content = Text;

	/// <summary>Variable written on click.</summary>
	[Category("AppVision"), Description("Variable written on click")]
	public string Variable { get; set; }

	/// <summary>Value written, converted by the server to the variable type.</summary>
	[Category("AppVision"), Description("Value written on click")]
	public string Value { get; set; } = "1";

	/// <summary>Button text.</summary>
	[Category("AppVision"), Description("Button text")]
	public string Text { get; set; } = "Command";

	protected override void OnClick()
	{
		base.OnClick();
		if (LiveVariable.Online && LiveVariable.Strip(Variable) is { } name)
			Sdk.Server.VariableManager.Set(name, Value);
	}
}
