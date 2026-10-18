using Prysm.AppVision.Common;
using Prysm.AppVision.SDK;
using Prysm.AppVision.SDK.Plugins;
using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;

namespace PasskeyAuthentication;

/// <summary>
/// Sign-in tile of appClient and appConfig for the Passkey method: one button, Windows does the rest (it lists the passkeys of the
/// relying party: no user name to type). A link opens the <see cref="RegisterDialog"/> that enrols the current device with the
/// AppVision password. Loaded by the clients from <c>appPluginPasskey.dll</c>, the DLL the server names in
/// <see cref="PasskeyAuth.Describe"/>, which also hands the relying party id and origin to <see cref="SetConfig"/>.
/// </summary>
/// <remarks>
/// <see cref="Login"/> drives the ceremony: the first server call answers 401 with the WebAuthn options, Windows does the
/// cryptography and shows its dialog, the second call opens the session. Server calls stay on the UI thread, because appClient routes
/// a 401 to the thread that called; only the Windows API runs on a worker thread, so the dispatcher keeps running behind the dialog.
/// Standard WPF controls only: the application theme styles them like the built-in sign-in forms.
/// </remarks>
public class PasskeySignIn : ISignInPlugin
{
	public string Name => PasskeyAuth.MethodName;

	readonly StackPanel _ui = new() { Width = 300 };
	readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0), Opacity = 0.7 };

	AppServer _server;
	string _origin;

	public PasskeySignIn()
	{
		var signIn = new Button { Content = "Sign in with a passkey", IsDefault = true, Margin = new Thickness(0, 24, 0, 0) };
		signIn.Click += (s, e) => OnLogin?.Invoke(this, [PasskeyProtocol.Step + "=" + PasskeyProtocol.Begin]);

		var register = new Hyperlink(new Run("Register this device..."));
		register.Click += (s, e) => Register();

		_ui.Children.Add(signIn);
		_ui.Children.Add(new TextBlock(register) { Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Center });
		_ui.Children.Add(_status);

		if (!WindowsWebAuthn.IsAvailable)
			Status("Passkeys need Windows 10 1903 or later.");
		else if (!WindowsWebAuthn.HasPlatformAuthenticator)
			Status("Windows Hello is not set up on this computer: a security key or a phone can be used.");
	}

	// the dialog gives the arguments of the RegisterBegin step; the sign-in pipeline of the client does the rest (Login below)
	void Register()
	{
		var dialog = new RegisterDialog();
		if (dialog.ShowDialog() == true)
			OnLogin?.Invoke(this, [PasskeyProtocol.Step + "=" + PasskeyProtocol.RegisterBegin, "Username=" + dialog.UserName, "Password=" + dialog.Password, "Device=" + dialog.Device]);
	}

	public UIElement GetUI() => _ui;
	public event EventHandler<NameValue[]> OnLogin;
	public void Initialize(IServer server) => _server = (AppServer)server;
	public void CleanUp() { }
	public string GetConfig() => "";

	/// <summary>Parameters published by the server (<c>RpId=</c>, <c>Origin=</c> lines): the origin signed in clientDataJSON.</summary>
	public void SetConfig(string settings)
	{
		foreach (var line in (settings ?? "").Split('\n'))
			if (line.StartsWith("Origin=", StringComparison.OrdinalIgnoreCase))
				_origin = line.Substring("Origin=".Length).Trim();
	}

	public async Task Login(NameValue[] args)
	{
		var hwnd = new WindowInteropHelper(Window.GetWindow(_ui) ?? Application.Current.MainWindow).Handle;
		try
		{
			// the server answers a Begin step with the WebAuthn options: a 401 whose message starts with Passkey:Create: or Passkey:Get:
			await _server.Login(args);
		}
		catch (HttpRequestException ex) when (Challenge(ex, PasskeyProtocol.CreatePrefix) is { } json)
		{
			await Register(hwnd, Json.Deserialize<CreateOptions>(json), Helper.GetParameter("Device", args));
		}
		catch (HttpRequestException ex) when (Challenge(ex, PasskeyProtocol.GetPrefix) is { } json)
		{
			await SignIn(hwnd, Json.Deserialize<GetOptions>(json));
		}
	}

	static string Challenge(HttpRequestException ex, string prefix) =>
		ex.StatusCode == HttpStatusCode.Unauthorized && ex.Message?.StartsWith(prefix) == true ? ex.Message.Substring(prefix.Length) : null;

	async Task Register(IntPtr hwnd, CreateOptions options, string device)
	{
		Status("Windows Security: confirm with your face, fingerprint, PIN or security key to create the passkey...");
		var clientData = PasskeyProtocol.ClientData("webauthn.create", options.Challenge, Origin(options.RpId));
		var attestation = await Task.Run(() => WindowsWebAuthn.MakeCredential(hwnd, options, clientData));
		await _server.Login([PasskeyProtocol.Step + "=" + PasskeyProtocol.RegisterFinish, "Username=" + options.UserName, "Device=" + device,
			"ClientData=" + Base64Url.Encode(clientData), "Attestation=" + Base64Url.Encode(attestation)]);
		Status("Passkey registered: next time, sign in without a password.");
	}

	async Task SignIn(IntPtr hwnd, GetOptions options)
	{
		Status("Windows Security: choose your passkey and confirm...");
		var clientData = PasskeyProtocol.ClientData("webauthn.get", options.Challenge, Origin(options.RpId));
		var assertion = await Task.Run(() => WindowsWebAuthn.GetAssertion(hwnd, options, clientData));
		await _server.Login([PasskeyProtocol.Step + "=" + PasskeyProtocol.Finish, "ClientData=" + Base64Url.Encode(clientData),
			"CredentialId=" + Base64Url.Encode(assertion.CredentialId), "AuthenticatorData=" + Base64Url.Encode(assertion.AuthenticatorData),
			"Signature=" + Base64Url.Encode(assertion.Signature), "UserHandle=" + Base64Url.Encode(assertion.UserHandle)]);
		Status("");
	}

	string Origin(string rpId) => string.IsNullOrEmpty(_origin) ? "https://" + rpId : _origin;

	void Status(string text) => _status.Text = text;
}

/// <summary>
/// "Register this device": user name, AppVision password and device name. A <see cref="ChildWindow"/> of appCommon, so that it
/// takes the theme and the owner of the application like the built-in dialogs.
/// </summary>
public class RegisterDialog : ChildWindow
{
	readonly TextBox _user = new() { Text = ConfigurationManager.AppSettings["Username"] };	// the name the standard form remembers
	readonly PasswordBox _password = new();
	readonly TextBox _device = new() { Text = Environment.MachineName };
	readonly TextBlock _message = new() { Opacity = 0.7, Margin = new Thickness(0, 12, 0, 0), TextWrapping = TextWrapping.Wrap };

	public string UserName => _user.Text.Trim();
	public string Password => _password.Password;
	public string Device => _device.Text.Trim();

	public RegisterDialog()
	{
		Title = "Register this device";
		Width = 350;
		SizeToContent = SizeToContent.Height;
		WindowStyle = WindowStyle.ToolWindow;
		ResizeMode = ResizeMode.NoResize;

		var ok = new Button { Content = "Register", MinWidth = 75, Margin = new Thickness(8, 0, 8, 0), IsDefault = true };
		ok.Click += (s, e) =>
		{
			if (UserName.Length == 0 || Password.Length == 0)
				_message.Text = "Type your user name and your AppVision password.";
			else
				DialogResult = true;
		};
		var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
		buttons.Children.Add(ok);
		buttons.Children.Add(new Button { Content = "Cancel", MinWidth = 75, IsCancel = true });

		var panel = new StackPanel { Margin = new Thickness(12) };
		panel.Children.Add(new TextBlock { Text = "The passkey is created on this computer (Windows Hello) or on a security key, after a check of your AppVision password.", TextWrapping = TextWrapping.Wrap, Opacity = 0.7 });
		panel.Children.Add(new TextBlock { Text = "User name", Margin = new Thickness(0, 16, 0, 0) });
		panel.Children.Add(_user);
		panel.Children.Add(new TextBlock { Text = "AppVision password", Margin = new Thickness(0, 16, 0, 0) });
		panel.Children.Add(_password);
		panel.Children.Add(new TextBlock { Text = "Device name", Margin = new Thickness(0, 16, 0, 0) });
		panel.Children.Add(_device);
		panel.Children.Add(_message);
		panel.Children.Add(buttons);
		Content = panel;

		Loaded += (s, e) => (UserName.Length == 0 ? (Control)_user : _password).Focus();
	}
}
