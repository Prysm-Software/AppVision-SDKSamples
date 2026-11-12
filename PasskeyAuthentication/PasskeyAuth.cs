using PasskeyAuthentication.WebAuthn;
using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.Script;
using Prysm.AppVision.SDK;
using Prysm.AppVision.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.ServiceModel.Web;
using System.Text;

namespace PasskeyAuthentication;

/// <summary>
/// Server authentication plugin: AppVision users sign in with a passkey (WebAuthn / FIDO2: Windows Hello, security key, phone)
/// instead of their password. The plugin is the relying party: it issues challenges, checks attestations and assertions, and keeps
/// the public keys in <c>Param\Server\Passkeys.json</c>.
/// </summary>
/// <remarks>
/// <para>Installed as <c>appPluginPasskey.dll</c> in the AppVision Bin folder: the method is named after the file (<c>Passkey</c>),
/// enabled in Configurator › Options › Authentication or by <c>App.Main.Authentication.Methods</c>.</para>
/// <para>A ceremony takes two <see cref="Login"/> calls of the same session. The first one (<c>Step=Begin</c> or
/// <c>Step=RegisterBegin</c>) is answered with a <see cref="WebFaultException{T}"/> 401 carrying the WebAuthn options: a
/// <c>FaultException</c> goes through the server to the client as is, without counting as a failed login. The second one
/// (<c>Step=Finish</c> or <c>Step=RegisterFinish</c>) brings the authenticator's answer and opens the session of the AppVision account.</para>
/// <para>Registering a passkey requires the account's password: <see cref="IServer.CheckUserPassword(NameValue[])"/> runs the
/// sign-in pipeline (failure counter, lockout, event) with the method named by <c>App.Main.Authentication.Passkey.ProofMethod</c>.</para>
/// </remarks>
public class PasskeyAuth : IAuthPlugin, IAuthClientInfo, IAuthProbe, IAuthAccountPlugin
{
	/// <summary>Method name: the DLL is appPluginPasskey.dll.</summary>
	public const string MethodName = "Passkey";
	public const string OptionPrefix = "App.Main.Authentication.Passkey.";

	static readonly TimeSpan ChallengeLifetime = TimeSpan.FromMinutes(2);

	// pending challenges by session id: a challenge is used once, by the session that asked for it
	static readonly Dictionary<string, Challenge> _challenges = [];

	IServer _server;

	public string Name => MethodName;

	/// <summary>Called by the server when the method starts; <see cref="ScriptServerBase.Server"/> is the fallback for older servers.</summary>
	public void Initialize(IServer server) => _server = server;
	public void CleanUp() { }

	IServer Server => _server ?? ScriptServerBase.Server;
	// no server in the configurator process (the plugin is instantiated there to describe itself): the defaults apply
	Settings Current => new(name => Server?.GetOption(OptionPrefix + name));
	PasskeyStore Store => new(Path.Combine(Server.GetPathParam(), "Server", PasskeyStore.FileName));

	/// <summary>
	/// The sign-in tile: a client plugin in the same DLL, which receives the relying party id and origin (public values), and for the
	/// web client the ES module web\Passkey.js, installed in Bin\Web\authplugins (WebModule=, a path relative to the server).
	/// </summary>
	public AuthenticationMethodInfo Describe()
	{
		var s = Current;
		return new()
		{
			Ui = AuthenticationUi.Plugin,
			ClientPlugin = "appPluginPasskey.dll",
			DisplayName = "Passkey",
			Parameters = $"RpId={s.RpId}\nOrigin={s.Origin}\nWebModule=/authplugins/Passkey.js",
		};
	}

	public UserRow Login(NameValue[] args) => Helper.GetParameter(PasskeyProtocol.Step, args) switch
	{
		PasskeyProtocol.RegisterBegin => RegisterBegin(args),
		PasskeyProtocol.RegisterFinish => RegisterFinish(args),
		PasskeyProtocol.Begin => Begin(args),
		PasskeyProtocol.Finish => Finish(args),
		// a user name and password login (default method with no Method=, a script...) is not a passkey ceremony
		_ => throw Refuse("Passkey: sign in with a passkey (Step=Begin), not with a password"),
	};

	#region Registration

	UserRow RegisterBegin(NameValue[] args)
	{
		var userName = Required("Username", args);
		var s = Current;

		// proof of identity by the sign-in pipeline: method named explicitly, the default one may be this plugin
		var identity = Server.CheckUserPassword(["Username=" + userName, "Password=" + Required("Password", args), "Method=" + s.ProofMethod]).GetAwaiter().GetResult()
			?? throw Refuse("Login:Invalid_user_password");
		var user = Server.UserManager.GetUserByName(identity.Name) ?? throw Refuse("Login:Invalid_user_password");

		var registered = Store.ForUser(user.Name);
		if (registered.Length >= s.MaxPerUser)
			throw Refuse($"Passkey: {registered.Length} passkeys are already registered for {user.Name}, remove one first");

		var challenge = NewChallenge(args, ChallengeKind.Create, user.Name);
		throw Continue(PasskeyProtocol.CreatePrefix + Json.Serialize(new CreateOptions
		{
			RpId = s.RpId,
			RpName = "AppVision",
			UserId = Base64Url.Encode(UserHandle(user)),
			UserName = user.Name,
			DisplayName = string.IsNullOrWhiteSpace(user.FullName) ? user.Name : user.FullName,
			Challenge = Base64Url.Encode(challenge),
			ExcludeCredentials = [.. registered.Select(c => c.Id)],
		}));
	}

	UserRow RegisterFinish(NameValue[] args)
	{
		var userName = Required("Username", args);
		var challenge = TakeChallenge(args, ChallengeKind.Create, userName);
		var s = Current;

		AuthenticatorData attested;
		try
		{
			attested = new WebAuthnVerifier(s.RpId, s.Origin).VerifyRegistration(
				Base64Url.Decode(Required("ClientData", args)), Base64Url.Decode(Required("Attestation", args)), challenge.Bytes);
		}
		catch (Exception ex)		// malformed data as well as a failed check: a refusal either way
		{
			Server.Log($"Passkey: registration refused for {userName}: {ex.Message}");
			throw Refuse("Passkey: registration refused, " + ex.Message);
		}

		var user = Server.UserManager.GetUserByName(userName) ?? throw Refuse("Login:Invalid_user_password");
		var device = Helper.GetParameter("Device", args) is { Length: > 0 } d ? d : "Passkey";
		Store.Add(new PasskeyCredential
		{
			User = user.Name,
			Id = Base64Url.Encode(attested.CredentialId),
			PublicKey = Base64Url.Encode(attested.CredentialPublicKey),
			SignCount = attested.SignCount,
			Name = device,
			Created = DateTime.UtcNow.ToString("o"),
			Aaguid = Base64Url.Encode(attested.Aaguid),
		});
		var algorithm = CoseKey.Algorithm(attested.CredentialPublicKey) == CoseKey.ES256 ? "ES256" : "RS256";
		Server.Log($"Passkey: {user.Name} registered '{device}' ({algorithm}), session opened");
		Notify(user, device);
		return user;		// the account itself (Id set): the server opens its session, no external account is created
	}

	// the user is told about every passkey added to their account: a registration they did not make is an alert
	void Notify(UserRow user, string device)
	{
		if (string.IsNullOrWhiteSpace(user.Email))
			return;
		try
		{
			_ = Server.ControllerManager.SendEmail(user.Email, "AppVision: a passkey was added to your account",
				$"The passkey '{device}' was registered for {user.Name} on {DateTime.Now:g}. If you did not do it, tell your administrator.");
		}
		catch (Exception ex)
		{
			Server.Log($"Passkey: e-mail to {user.Email} not sent: {ex.Message}");
		}
	}

	#endregion

	#region Sign-in

	UserRow Begin(NameValue[] args)
	{
		var userName = Helper.GetParameter("Username", args);
		var s = Current;
		// with a user name, the authenticator is told which credentials fit; without, it shows its own account list
		// (discoverable credentials). An unknown name gets an empty list too: no account enumeration.
		string[] allow = string.IsNullOrWhiteSpace(userName) ? [] : [.. Store.ForUser(userName).Select(c => c.Id)];
		var challenge = NewChallenge(args, ChallengeKind.Get, userName);
		throw Continue(PasskeyProtocol.GetPrefix + Json.Serialize(new GetOptions { RpId = s.RpId, Challenge = Base64Url.Encode(challenge), AllowCredentials = allow }));
	}

	UserRow Finish(NameValue[] args)
	{
		var challenge = TakeChallenge(args, ChallengeKind.Get, null);
		var id = Required("CredentialId", args);
		var credential = Store.Find(id) ?? throw Refuse("Passkey: unknown passkey, register this device first");
		if (!string.IsNullOrWhiteSpace(challenge.User) && !credential.User.EqualsInv(challenge.User))
			throw Refuse("Passkey: this passkey belongs to another account");
		var s = Current;

		uint signCount;
		try
		{
			signCount = new WebAuthnVerifier(s.RpId, s.Origin).VerifyAssertion(
				Base64Url.Decode(Required("ClientData", args)), Base64Url.Decode(Required("AuthenticatorData", args)),
				Base64Url.Decode(Required("Signature", args)), challenge.Bytes, Base64Url.Decode(credential.PublicKey), credential.SignCount);
		}
		catch (Exception ex)
		{
			Server.Log($"Passkey: sign-in refused for {credential.User}: {ex.Message}");
			throw Refuse("Passkey: sign-in refused, " + ex.Message);
		}
		Store.UpdateSignCount(id, signCount);

		var user = Server.UserManager.GetUserByName(credential.User) ?? throw Refuse("Login:Invalid_user_password");
		if (Helper.GetParameter("UserHandle", args) is { Length: > 0 } handle && !Base64Url.Decode(handle).SequenceEqual(UserHandle(user)))
			throw Refuse("Passkey: user handle does not match the account");

		Server.Log($"Passkey: {user.Name} signed in with '{credential.Name}'");
		return user;
	}

	#endregion

	#region Challenges

	enum ChallengeKind { Create, Get }

	sealed class Challenge(ChallengeKind kind, string user, byte[] bytes)
	{
		public ChallengeKind Kind { get; } = kind;
		public string User { get; } = user;
		public byte[] Bytes { get; } = bytes;
		public DateTime Expires { get; } = DateTime.UtcNow + ChallengeLifetime;
	}

	// 32 random bytes, kept for the session that asked (SessionId is added to the arguments by the server, AppVision 4.10.2)
	static byte[] NewChallenge(NameValue[] args, ChallengeKind kind, string user)
	{
		var bytes = new byte[32];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(bytes);
		lock (_challenges)
		{
			foreach (var expired in _challenges.Where(c => c.Value.Expires < DateTime.UtcNow).Select(c => c.Key).ToList())
				_challenges.Remove(expired);
			_challenges[Session(args)] = new(kind, user, bytes);
		}
		return bytes;
	}

	// single use: the challenge is removed, a replayed Finish finds nothing
	static Challenge TakeChallenge(NameValue[] args, ChallengeKind kind, string user)
	{
		lock (_challenges)
		{
			if (!_challenges.TryGetValue(Session(args), out var challenge) || challenge.Kind != kind || challenge.Expires < DateTime.UtcNow
				|| user != null && !user.EqualsInv(challenge.User))
				throw Refuse("Passkey: no pending challenge for this session, start again");
			_challenges.Remove(Session(args));
			return challenge;
		}
	}

	static string Session(NameValue[] args) => Helper.GetParameter("SessionId", args) ?? throw new InvalidOperationException("SessionId expected in the login arguments (AppVision 4.10.2)");

	// opaque, stable user handle returned by the authenticator with each assertion (WebAuthn § 5.4.3): never the name
	static byte[] UserHandle(UserRow user)
	{
		using var sha = SHA256.Create();
		return sha.ComputeHash(Encoding.UTF8.GetBytes("AppVision user " + user.Id));
	}

	#endregion

	// 401 with a readable message: a FaultException reaches the client as is, through the server, without a failed-login event
	static WebFaultException<string> Continue(string message) => new(message, HttpStatusCode.Unauthorized);
	static WebFaultException<string> Refuse(string message) => new(message, HttpStatusCode.Unauthorized);

	static string Required(string name, NameValue[] args) =>
		Helper.GetParameter(name, args) is { Length: > 0 } value ? value : throw Refuse($"Passkey: argument {name} expected");

	/// <summary>Options of the method, read from the server or from the values appConfig tests before saving them.</summary>
	sealed class Settings
	{
		/// <summary>
		/// Relying party id: the DNS name the passkeys are bound to. Default: the fully qualified name of the server (pc-01.example.com),
		/// browsers refuse a single-label name (pc-01). Changing it orphans every passkey.
		/// </summary>
		public string RpId { get; }
		/// <summary>Origin the client writes in clientDataJSON. Default: https://&lt;RpId&gt;.</summary>
		public string Origin { get; }
		/// <summary>Method that checks the password at registration (Local, AD, LDAP...). Default: Local.</summary>
		public string ProofMethod { get; }
		/// <summary>Passkeys per account. Default: 5.</summary>
		public int MaxPerUser { get; }

		public Settings(Func<string, string> option)
		{
			RpId = option("RpId") is { Length: > 0 } rp ? rp : FullHostName();
			Origin = option("Origin") is { Length: > 0 } o ? o : "https://" + RpId;
			ProofMethod = option("ProofMethod") is { Length: > 0 } m ? m : "Local";
			MaxPerUser = int.TryParse(option("MaxPerUser"), out var n) && n > 0 ? n : 5;
		}

		// fully qualified name of the server; its short name when the machine is not in a DNS domain
		static string FullHostName()
		{
			try
			{
				return Dns.GetHostEntry(string.Empty).HostName.ToLowerInvariant();
			}
			catch (SocketException)
			{
				return Dns.GetHostName().ToLowerInvariant();
			}
		}
	}

	#region IAuthProbe: state and test in Configurator › Options › Authentication

	public string[] RequiredOptions => [];

	public AuthProbeResult Probe(OptionRow[] options, NameValue[] args)
	{
		var s = new Settings(name => options?.FirstOrDefault(o => o.Name.EqualsInv(OptionPrefix + name))?.Value);
		return new()
		{
			Success = true,
			Message = $"Relying party '{s.RpId}', origin '{s.Origin}', identity proved by '{s.ProofMethod}' at registration",
			Details = [new("RpId", s.RpId), new("Origin", s.Origin), new("ProofMethod", s.ProofMethod), new("MaxPerUser", s.MaxPerUser)],
		};
	}

	#endregion
}
