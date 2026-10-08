using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace ExternalAuthentication
{
	/// <summary>An account of the external directory, as the plugin needs it.</summary>
	public class ExternalUser
	{
		public string Name { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		/// <summary>Groups of the account in the directory, mapped to AppVision profiles.</summary>
		public string[] Groups { get; set; } = new string[0];
	}

	/// <summary>
	/// The external system that knows the users. Replace <see cref="JsonUserDirectory"/> by your own implementation:
	/// REST API of an identity provider, database of an HR or access control system, ...
	/// </summary>
	public interface IUserDirectory
	{
		/// <summary>Checks the credentials. Returns the account, or null when the user is unknown, disabled or the password wrong.</summary>
		ExternalUser Authenticate(string userName, string password);
	}

	/// <summary>
	/// Directory read from a JSON file (<c>ExternalUsers.json</c>), with salted password hashes (PBKDF2-SHA256).
	/// Stands for the external system in this sample; the file is read at each login, so changes apply at once.
	/// </summary>
	public class JsonUserDirectory : IUserDirectory
	{
		readonly string _path;

		public JsonUserDirectory(string path) => _path = path;

		public ExternalUser Authenticate(string userName, string password)
		{
			var file = Load();
			var user = file.Users?.FirstOrDefault(u => string.Equals(u.Name, userName, StringComparison.OrdinalIgnoreCase));
			if (user == null || user.Disabled || !VerifyPassword(password ?? "", user.Password))
				return null;
			return new ExternalUser
			{
				Name = user.Name,
				FirstName = user.FirstName,
				LastName = user.LastName,
				Email = user.Email,
				Groups = user.Groups ?? new string[0],
			};
		}

		DirectoryFile Load()
		{
			if (!File.Exists(_path))
				throw new FileNotFoundException("User directory not found", _path);
			using (var stream = File.OpenRead(_path))
				return (DirectoryFile)new DataContractJsonSerializer(typeof(DirectoryFile)).ReadObject(stream);
		}

		/// <summary>
		/// Checks a password against a hash "pbkdf2$iterations$salt$hash" (Base64), as written by New-PasswordHash.ps1.
		/// Never store passwords in clear: a stolen file must not give the passwords.
		/// </summary>
		public static bool VerifyPassword(string password, string stored)
		{
			var parts = stored?.Split('$');
			if (parts?.Length != 4 || parts[0] != "pbkdf2" || !int.TryParse(parts[1], out var iterations))
				return false;
			var salt = Convert.FromBase64String(parts[2]);
			var expected = Convert.FromBase64String(parts[3]);
			using (var kdf = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256))
			{
				var actual = kdf.GetBytes(expected.Length);
				// constant-time comparison: the time taken does not tell how many bytes match
				var diff = 0;
				for (int i = 0; i < expected.Length; i++)
					diff |= actual[i] ^ expected[i];
				return diff == 0;
			}
		}

		[DataContract]
		class DirectoryFile
		{
			[DataMember(Name = "users")] public DirectoryUser[] Users { get; set; }
		}

		[DataContract]
		class DirectoryUser
		{
			[DataMember(Name = "name")] public string Name { get; set; }
			[DataMember(Name = "firstName")] public string FirstName { get; set; }
			[DataMember(Name = "lastName")] public string LastName { get; set; }
			[DataMember(Name = "email")] public string Email { get; set; }
			[DataMember(Name = "groups")] public string[] Groups { get; set; }
			[DataMember(Name = "disabled")] public bool Disabled { get; set; }
			[DataMember(Name = "password")] public string Password { get; set; }
		}
	}
}
