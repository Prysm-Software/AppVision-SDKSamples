using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace PasskeyAuthentication;

/// <summary>
/// The Windows WebAuthn API (<c>webauthn.dll</c>, Windows 10 1903 and later): the same calls a browser makes for
/// <c>navigator.credentials.create</c> and <c>get</c>. Windows shows its own "Windows Security" dialog (face, fingerprint, PIN,
/// security key, phone) and talks to the authenticator; the application only builds <c>clientDataJSON</c> and reads the result.
/// Structures are the version 1 ones of <c>webauthn.h</c> (github.com/microsoft/webauthn), enough for passkeys.
/// </summary>
public static class WindowsWebAuthn
{
	const string CredentialType = "public-key";
	const uint UserVerificationRequired = 2, AttestationNone = 1, AttachmentAny = 0, TimeoutMs = 60_000;

	/// <summary>Result of a sign-in ceremony, as the server wants it.</summary>
	public sealed class AssertionResult
	{
		public byte[] CredentialId, AuthenticatorData, Signature, UserHandle;
	}

	public static bool IsAvailable
	{
		get
		{
			try { return WebAuthNGetApiVersionNumber() >= 1; }
			catch (DllNotFoundException) { return false; }
		}
	}

	/// <summary>True when Windows Hello (face, fingerprint or PIN) is set up on this computer.</summary>
	public static bool HasPlatformAuthenticator => WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable(out var available) == 0 && available != 0;

	/// <summary>Creates a passkey for the relying party and returns the attestation object (CBOR) the server verifies.</summary>
	public static byte[] MakeCredential(IntPtr hwnd, CreateOptions options, byte[] clientDataJson)
	{
		using var userId = new Pinned(Base64Url.Decode(options.UserId));
		using var clientData = new Pinned(clientDataJson);
		using var exclude = new CredentialList(options.ExcludeCredentials);
		using var algorithms = new Unmanaged<CoseParameter>([new() { Version = 1, Type = CredentialType, Alg = -7 }, new() { Version = 1, Type = CredentialType, Alg = -257 }]);

		var rp = new RpInfo { Version = 1, Id = options.RpId, Name = options.RpName };
		var user = new UserInfo { Version = 1, IdLength = (uint)userId.Length, Id = userId.Pointer, Name = options.UserName, DisplayName = options.DisplayName };
		var parameters = new CoseParameters { Count = 2, Parameters = algorithms.Pointer };
		var data = new ClientDataStruct { Version = 1, Length = (uint)clientData.Length, Data = clientData.Pointer, HashAlgId = "SHA-256" };
		var make = new MakeCredentialOptions
		{
			Version = 1, TimeoutMs = TimeoutMs, ExcludeList = exclude.Credentials, Attachment = AttachmentAny,
			RequireResidentKey = 1,		// discoverable credential: sign-in without typing a user name
			UserVerification = UserVerificationRequired, Attestation = AttestationNone,
		};

		Check(WebAuthNAuthenticatorMakeCredential(hwnd, ref rp, ref user, ref parameters, ref data, ref make, out var result));
		try
		{
			var attestation = Marshal.PtrToStructure<CredentialAttestation>(result);
			return Copy(attestation.AttestationObject, attestation.AttestationObjectLength);
		}
		finally { WebAuthNFreeCredentialAttestation(result); }
	}

	/// <summary>Signs the server challenge with a passkey of the relying party.</summary>
	public static AssertionResult GetAssertion(IntPtr hwnd, GetOptions options, byte[] clientDataJson)
	{
		using var clientData = new Pinned(clientDataJson);
		using var allow = new CredentialList(options.AllowCredentials);

		var data = new ClientDataStruct { Version = 1, Length = (uint)clientData.Length, Data = clientData.Pointer, HashAlgId = "SHA-256" };
		var get = new GetAssertionOptions { Version = 1, TimeoutMs = TimeoutMs, AllowList = allow.Credentials, Attachment = AttachmentAny, UserVerification = UserVerificationRequired };

		Check(WebAuthNAuthenticatorGetAssertion(hwnd, options.RpId, ref data, ref get, out var result));
		try
		{
			var assertion = Marshal.PtrToStructure<Assertion>(result);
			return new()
			{
				CredentialId = Copy(assertion.Credential.Id, assertion.Credential.IdLength),
				AuthenticatorData = Copy(assertion.AuthenticatorData, assertion.AuthenticatorDataLength),
				Signature = Copy(assertion.Signature, assertion.SignatureLength),
				UserHandle = Copy(assertion.UserId, assertion.UserIdLength),
			};
		}
		finally { WebAuthNFreeAssertion(result); }
	}

	static void Check(int hr)
	{
		if (hr >= 0)
			return;
		var name = Marshal.PtrToStringUni(WebAuthNGetErrorName(hr));		// NotAllowedError, InvalidStateError, ConstraintError...
		throw new InvalidOperationException(hr switch
		{
			unchecked((int)0x800704C7) or unchecked((int)0x80090036) => "Cancelled by the user",
			unchecked((int)0x8009000F) when name == "InvalidStateError" => "A passkey of this account already exists on this authenticator",
			_ => name == "NotAllowedError" ? "Refused by Windows Security: no matching passkey, time out or cancelled" : $"Windows Security: {name} (0x{hr:X8})",
		});
	}

	static byte[] Copy(IntPtr source, uint length)
	{
		var bytes = new byte[length];
		if (length > 0)
			Marshal.Copy(source, bytes, 0, (int)length);
		return bytes;
	}

	#region Native memory

	// a byte array pinned for the duration of the call
	sealed class Pinned(byte[] bytes) : IDisposable
	{
		GCHandle _handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		public int Length => bytes.Length;
		public IntPtr Pointer => bytes.Length == 0 ? IntPtr.Zero : _handle.AddrOfPinnedObject();
		public void Dispose() => _handle.Free();
	}

	// an array of structures copied to unmanaged memory
	sealed class Unmanaged<T> : IDisposable where T : struct
	{
		readonly int _size = Marshal.SizeOf<T>();
		public IntPtr Pointer { get; }
		public Unmanaged(T[] items)
		{
			Pointer = Marshal.AllocHGlobal(_size * Math.Max(items.Length, 1));
			for (var i = 0; i < items.Length; i++)
				Marshal.StructureToPtr(items[i], Pointer + i * _size, false);
		}
		public void Dispose() => Marshal.FreeHGlobal(Pointer);
	}

	// the allow / exclude list: credential ids pinned, descriptors in unmanaged memory
	sealed class CredentialList : IDisposable
	{
		readonly Pinned[] _ids;
		readonly Unmanaged<Credential> _list;
		public Credentials Credentials { get; }
		public CredentialList(string[] ids)
		{
			_ids = [.. (ids ?? []).Select(id => new Pinned(Base64Url.Decode(id)))];
			_list = new([.. _ids.Select(p => new Credential { Version = 1, IdLength = (uint)p.Length, Id = p.Pointer, Type = CredentialType })]);
			Credentials = new() { Count = (uint)_ids.Length, List = _ids.Length == 0 ? IntPtr.Zero : _list.Pointer };
		}
		public void Dispose()
		{
			_list.Dispose();
			foreach (var id in _ids)
				id.Dispose();
		}
	}

	#endregion

	#region webauthn.h, version 1 structures

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct RpInfo { public uint Version; public string Id, Name, Icon; }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct UserInfo { public uint Version, IdLength; public IntPtr Id; public string Name, Icon, DisplayName; }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct CoseParameter { public uint Version; public string Type; public int Alg; }

	[StructLayout(LayoutKind.Sequential)]
	struct CoseParameters { public uint Count; public IntPtr Parameters; }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct ClientDataStruct { public uint Version, Length; public IntPtr Data; public string HashAlgId; }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct Credential { public uint Version, IdLength; public IntPtr Id; public string Type; }

	[StructLayout(LayoutKind.Sequential)]
	public struct Credentials { public uint Count; public IntPtr List; }

	[StructLayout(LayoutKind.Sequential)]
	struct Extensions { public uint Count; public IntPtr List; }

	[StructLayout(LayoutKind.Sequential)]
	struct MakeCredentialOptions
	{
		public uint Version, TimeoutMs;
		public Credentials ExcludeList;
		public Extensions Extensions;
		public uint Attachment;
		public int RequireResidentKey;
		public uint UserVerification, Attestation, Flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct GetAssertionOptions
	{
		public uint Version, TimeoutMs;
		public Credentials AllowList;
		public Extensions Extensions;
		public uint Attachment, UserVerification, Flags;
	}

	// first fields of WEBAUTHN_CREDENTIAL_ATTESTATION: the structure is longer, only this prefix is read
	[StructLayout(LayoutKind.Sequential)]
	struct CredentialAttestation
	{
		public uint Version;
		public IntPtr FormatType;
		public uint AuthenticatorDataLength;
		public IntPtr AuthenticatorData;
		public uint AttestationLength;
		public IntPtr Attestation;
		public uint AttestationDecodeType;
		public IntPtr AttestationDecode;
		public uint AttestationObjectLength;
		public IntPtr AttestationObject;
		public uint CredentialIdLength;
		public IntPtr CredentialId;
	}

	// first fields of WEBAUTHN_ASSERTION
	[StructLayout(LayoutKind.Sequential)]
	struct Assertion
	{
		public uint Version, AuthenticatorDataLength;
		public IntPtr AuthenticatorData;
		public uint SignatureLength;
		public IntPtr Signature;
		public Credential Credential;
		public uint UserIdLength;
		public IntPtr UserId;
	}

	[DllImport("webauthn.dll")] static extern uint WebAuthNGetApiVersionNumber();
	[DllImport("webauthn.dll")] static extern int WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable(out int available);
	[DllImport("webauthn.dll", CharSet = CharSet.Unicode)]
	static extern int WebAuthNAuthenticatorMakeCredential(IntPtr hwnd, ref RpInfo rp, ref UserInfo user, ref CoseParameters parameters, ref ClientDataStruct clientData, ref MakeCredentialOptions options, out IntPtr attestation);
	[DllImport("webauthn.dll", CharSet = CharSet.Unicode)]
	static extern int WebAuthNAuthenticatorGetAssertion(IntPtr hwnd, string rpId, ref ClientDataStruct clientData, ref GetAssertionOptions options, out IntPtr assertion);
	[DllImport("webauthn.dll")] static extern void WebAuthNFreeCredentialAttestation(IntPtr attestation);
	[DllImport("webauthn.dll")] static extern void WebAuthNFreeAssertion(IntPtr assertion);
	[DllImport("webauthn.dll")] static extern IntPtr WebAuthNGetErrorName(int hr);

	#endregion
}
