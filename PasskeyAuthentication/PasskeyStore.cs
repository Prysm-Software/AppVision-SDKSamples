using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PasskeyAuthentication;

/// <summary>A registered passkey: the public key the server checks assertions with, and what is needed to list or revoke it.</summary>
[DataContract]
public class PasskeyCredential
{
	/// <summary>AppVision user name, upper case as the server stores it.</summary>
	[DataMember(Name = "user")] public string User { get; set; }
	/// <summary>Credential id chosen by the authenticator (base64url).</summary>
	[DataMember(Name = "id")] public string Id { get; set; }
	/// <summary>COSE public key (base64url).</summary>
	[DataMember(Name = "publicKey")] public string PublicKey { get; set; }
	/// <summary>Last signature counter seen: an assertion must bring a higher one (clone detection).</summary>
	[DataMember(Name = "signCount")] public uint SignCount { get; set; }
	/// <summary>Device name typed by the user at registration.</summary>
	[DataMember(Name = "name")] public string Name { get; set; }
	[DataMember(Name = "created")] public string Created { get; set; }
	/// <summary>Authenticator model identifier (base64url of 16 bytes), zero for a platform authenticator that hides it.</summary>
	[DataMember(Name = "aaguid")] public string Aaguid { get; set; }
}

[DataContract]
class PasskeyFile
{
	[DataMember(Name = "credentials")] public List<PasskeyCredential> Credentials { get; set; } = [];
}

/// <summary>
/// The passkeys of the site, in <c>Param\Server\Passkeys.json</c> of the configuration: a folder the server never sends to the
/// clients. Public keys are not secrets, but only the server must be able to add or remove entries. Written atomically (temporary
/// file then replace), read under a lock: several users may sign in at the same time.
/// </summary>
public sealed class PasskeyStore(string path)
{
	public const string FileName = "Passkeys.json";

	readonly object _lock = new();

	public PasskeyCredential[] ForUser(string user)
	{
		lock (_lock)
			return [.. Load().Credentials.Where(c => c.User.Equals(user, StringComparison.OrdinalIgnoreCase))];
	}

	public PasskeyCredential Find(string id)
	{
		lock (_lock)
			return Load().Credentials.FirstOrDefault(c => c.Id == id);
	}

	public void Add(PasskeyCredential credential)
	{
		lock (_lock)
		{
			var file = Load();
			file.Credentials.RemoveAll(c => c.Id == credential.Id);
			file.Credentials.Add(credential);
			Save(file);
		}
	}

	public void UpdateSignCount(string id, uint signCount)
	{
		lock (_lock)
		{
			var file = Load();
			if (file.Credentials.FirstOrDefault(c => c.Id == id) is { } credential)
			{
				credential.SignCount = signCount;
				Save(file);
			}
		}
	}

	PasskeyFile Load() => File.Exists(path) ? Json.Deserialize<PasskeyFile>(File.ReadAllText(path)) : new PasskeyFile();

	void Save(PasskeyFile file)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		var temp = path + ".tmp";
		File.WriteAllText(temp, Json.Serialize(file));
		if (File.Exists(path))
			File.Replace(temp, path, null);
		else
			File.Move(temp, path);
	}
}
