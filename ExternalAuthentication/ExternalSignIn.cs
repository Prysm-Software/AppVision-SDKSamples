using Prysm.AppVision.UI.SignIn;

namespace ExternalAuthentication
{
	/// <summary>
	/// Sign-in screen of the clients (appClient, appConfig) when the server uses this plugin: the standard user name and
	/// password form, which this class reuses as is.
	/// </summary>
	/// <remarks>
	/// AppVision 4.10 clients look for an <c>ISignInPlugin</c> in the DLL named by <c>App.Main.Authentication</c>, so
	/// the DLL must also be installed on the client workstations. A plugin that needs another screen (a code, a badge,
	/// a button opening a web page) implements <c>ISignInPlugin</c> itself and raises <c>OnLogin</c> with the arguments
	/// that its server part reads.
	/// </remarks>
	public class ExternalSignIn : AppUserLogin
	{
	}
}
