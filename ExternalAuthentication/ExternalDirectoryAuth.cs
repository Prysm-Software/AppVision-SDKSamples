using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.Script;
using Prysm.AppVision.SDK;
using Prysm.AppVision.SDK.Plugins;
using System;
using System.IO;
using System.Linq;

namespace ExternalAuthentication
{
	/// <summary>
	/// Server authentication plugin: AppVision users sign in with the account they have in an external directory,
	/// and get the AppVision profile that matches their groups in that directory.
	/// </summary>
	/// <remarks>
	/// <para>Installed with the option <c>App.Main.Authentication = ExternalAuthentication.dll</c>: the server creates
	/// the first <see cref="IAuthPlugin"/> type of the DLL and calls <see cref="Login"/> for each sign-in, in the server
	/// process (appServer.exe).</para>
	/// <para>Returning a <see cref="UserRow"/> without <c>Id</c> makes the server handle an external account: it is
	/// created in AppVision at the first sign-in (without any local password), then updated at each sign-in. Its
	/// history (alarm acknowledgements, events) is kept under this name.</para>
	/// <para>A profile is chosen with the parameter <c>ExternalGroup=&lt;group&gt;</c> of the AppVision profile
	/// (Configurator › Users › Profiles › Parameters).</para>
	/// </remarks>
	public class ExternalDirectoryAuth : IAuthPlugin
	{
		/// <summary>Parameter of an AppVision profile naming the directory group it is given to.</summary>
		public const string GroupParameter = "ExternalGroup";

		/// <summary>Directory file, in the <c>Param\Server</c> folder of the configuration (not sent to the clients).</summary>
		public const string DirectoryFile = "ExternalUsers.json";

		public string Name => "ExternalDirectory";

		/// <summary>Not called by every AppVision version: do not rely on it, use <see cref="ScriptServerBase.Server"/>.</summary>
		public void Initialize(IServer server) { }

		public void CleanUp() { }

		/// <summary>
		/// Called by the server for each sign-in.
		/// </summary>
		/// <param name="args">Sign-in arguments sent by the client: <c>Username</c>, <c>Password</c>, ...</param>
		/// <returns>The user, with its profile. Never null: throw an exception to refuse.</returns>
		/// <remarks>When this method throws, the server tries the AppVision accounts (local fallback) with the same
		/// user name and password: an administrator account of AppVision can still sign in if the directory is down.</remarks>
		public UserRow Login(NameValue[] args)
		{
			var userName = Helper.GetParameter("Username", args);
			var password = Helper.GetParameter("Password", args);
			if (string.IsNullOrWhiteSpace(userName) || password == null)
				throw new UnauthorizedAccessException("User name and password expected");

			// The server API, in the server process: options, users, profiles, folders of the configuration...
			var server = ScriptServerBase.Server;

			IUserDirectory directory = new JsonUserDirectory(Path.Combine(server.GetPathParam(), "Server", DirectoryFile));
			var account = directory.Authenticate(userName, password)
				?? throw new UnauthorizedAccessException($"{userName}: unknown user, account disabled or wrong password");

			// First group of the account that an AppVision profile claims
			var profiles = server.UserManager.GetProfiles();
			var profile = account.Groups
				.Select(g => profiles.FirstOrDefault(p => string.Equals(p.GetParameter(GroupParameter), g, StringComparison.OrdinalIgnoreCase)))
				.FirstOrDefault(p => p != null)
				?? throw new UnauthorizedAccessException($"{userName}: no AppVision profile for the groups {string.Join(", ", account.Groups)}");

			server.Log($"ExternalDirectory: {account.Name} signed in, group profile '{profile.Name}'");

			return new UserRow
			{
				Name = account.Name,            // the server stores it in upper case
				FirstName = account.FirstName,
				LastName = account.LastName,
				Email = account.Email,
				ProfileId = profile.Id,
			};
		}
	}
}
