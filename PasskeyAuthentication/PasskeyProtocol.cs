using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PasskeyAuthentication;

/// <summary>
/// What the server plugin and the client plugin exchange through <c>Login</c> calls of one session (see the README).
/// The server answers a <c>Begin</c> step with HTTP 401 and a message <c>Passkey:Create:{json}</c> or <c>Passkey:Get:{json}</c>;
/// the client completes the WebAuthn ceremony with Windows and sends the <c>Finish</c> step.
/// </summary>
public static class PasskeyProtocol
{
	/// <summary>Login argument naming the step: RegisterBegin, RegisterFinish, Begin, Finish.</summary>
	public const string Step = "Step";
	public const string RegisterBegin = "RegisterBegin", RegisterFinish = "RegisterFinish", Begin = "Begin", Finish = "Finish";

	/// <summary>Prefixes of the 401 messages carrying the WebAuthn options to the client.</summary>
	public const string CreatePrefix = "Passkey:Create:", GetPrefix = "Passkey:Get:";

	/// <summary>Builds the <c>clientDataJSON</c> the authenticator signs: the client makes it, the server checks every field.</summary>
	public static byte[] ClientData(string type, string challenge, string origin) =>
		Encoding.UTF8.GetBytes($"{{\"type\":\"{type}\",\"challenge\":\"{challenge}\",\"origin\":\"{origin}\",\"crossOrigin\":false}}");
}

/// <summary>Options of <c>navigator.credentials.create</c> the server sends to register a passkey (WebAuthn § 5.4, reduced to what the sample uses).</summary>
[DataContract]
public class CreateOptions
{
	[DataMember] public string RpId { get; set; }
	[DataMember] public string RpName { get; set; }
	/// <summary>Opaque user handle (base64url): the authenticator returns it with each assertion.</summary>
	[DataMember] public string UserId { get; set; }
	[DataMember] public string UserName { get; set; }
	[DataMember] public string DisplayName { get; set; }
	[DataMember] public string Challenge { get; set; }
	/// <summary>Credential ids (base64url) already registered for the account: the authenticator refuses to create a duplicate.</summary>
	[DataMember] public string[] ExcludeCredentials { get; set; }
}

/// <summary>Options of <c>navigator.credentials.get</c> the server sends to sign in (WebAuthn § 5.5).</summary>
[DataContract]
public class GetOptions
{
	[DataMember] public string RpId { get; set; }
	[DataMember] public string Challenge { get; set; }
	/// <summary>Credential ids (base64url) of the account when the user typed a name; empty: any passkey of this relying party.</summary>
	[DataMember] public string[] AllowCredentials { get; set; }
}

/// <summary>The <c>clientDataJSON</c> as the server reads it back (WebAuthn § 5.8.1).</summary>
[DataContract]
public class ClientData
{
	[DataMember(Name = "type")] public string Type { get; set; }
	[DataMember(Name = "challenge")] public string Challenge { get; set; }
	[DataMember(Name = "origin")] public string Origin { get; set; }
}

/// <summary>JSON through DataContractJsonSerializer: nothing to add to the AppVision Bin folder.</summary>
public static class Json
{
	public static string Serialize<T>(T value)
	{
		using var stream = new MemoryStream();
		new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value);
		return Encoding.UTF8.GetString(stream.ToArray());
	}

	public static T Deserialize<T>(string json)
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
		return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
	}

	public static T Deserialize<T>(byte[] utf8) => Deserialize<T>(Encoding.UTF8.GetString(utf8));
}

/// <summary>Base64url (RFC 4648 § 5, no padding): the encoding WebAuthn uses for challenges and ids.</summary>
public static class Base64Url
{
	public static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

	public static byte[] Decode(string text)
	{
		if (string.IsNullOrEmpty(text))
			return [];
		var s = text.Replace('-', '+').Replace('_', '/');
		return Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
	}
}
