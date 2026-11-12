using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using System;
using System.Windows;

namespace WidgetLibrary;

/// <summary>
/// Follows one AppVision variable on behalf of a widget: reads its current state when the widget is loaded, then
/// calls <c>changed</c> on the UI thread each time the server notifies a new state. Subscribes on Loaded and
/// unsubscribes on Unloaded, so a widget removed from a synoptic leaves no handler behind.
/// </summary>
/// <remarks>
/// <c>internal</c>: this helper is not a widget. Only public types carrying <c>[AppWidget]</c> appear in the toolbox.
/// </remarks>
internal sealed class LiveVariable
{
	readonly FrameworkElement owner;
	readonly Func<string> name;
	readonly Action<VariableState> changed;

	/// <param name="owner">Widget that owns the subscription (its Loaded / Unloaded events drive it).</param>
	/// <param name="name">Variable name, read when the widget is loaded (a property set in the synoptic XAML).</param>
	/// <param name="changed">Called on the UI thread with the current state; null when the variable is unknown.</param>
	public LiveVariable(FrameworkElement owner, Func<string> name, Action<VariableState> changed)
	{
		(this.owner, this.name, this.changed) = (owner, name, changed);
		owner.Loaded += (_, _) => Start();
		owner.Unloaded += (_, _) => Stop();
	}

	/// <summary>True when the widget runs in an AppVision client connected to a server (false in the designer).</summary>
	public static bool Online => Sdk.Server is { IsConnected: true };

	/// <summary>Variable name without the optional <c>$V.</c> prefix.</summary>
	public string Name => Strip(name());

	/// <summary>Variable name without spaces nor the optional <c>$V.</c> prefix; null when empty.</summary>
	public static string Strip(string name) => name?.Trim() is { Length: > 0 } n ? (n.StartsWith("$V.") ? n.Substring(3) : n) : null;

	void Start()
	{
		if (!Online || Name is null)
			return;
		Sdk.Server.VariableManager.StateChanged += OnStateChanged;
		changed(Sdk.Server.VariableManager.GetStateByName(Name));
	}

	void Stop()
	{
		if (Online)
			Sdk.Server.VariableManager.StateChanged -= OnStateChanged;
	}

	// notifications may arrive on a background thread (client in web service mode): always go back to the UI thread
	// before touching the widget
	void OnStateChanged(VariableState state)
	{
		if (string.Equals(state.Name, Name, StringComparison.OrdinalIgnoreCase))
			owner.Dispatcher.InvokeAsync(() => changed(state));
	}
}
