using System;

namespace PasskeyAuthentication.WebAuthn;

/// <summary>
/// The authenticator data of an attestation or an assertion (WebAuthn § 6.1): hash of the relying party id, flags, signature counter
/// and, at registration only, the attested credential data (AAGUID, credential id, COSE public key).
/// </summary>
public sealed class AuthenticatorData
{
	public byte[] Raw { get; private set; }
	public byte[] RpIdHash { get; private set; }
	public byte Flags { get; private set; }
	public uint SignCount { get; private set; }
	public byte[] Aaguid { get; private set; }
	public byte[] CredentialId { get; private set; }
	public byte[] CredentialPublicKey { get; private set; }

	public bool UserPresent => (Flags & 0x01) != 0;
	public bool UserVerified => (Flags & 0x04) != 0;
	public bool HasCredential => (Flags & 0x40) != 0;

	public static AuthenticatorData Parse(byte[] raw)
	{
		if (raw == null || raw.Length < 37)
			throw new FormatException("authenticator data: at least 37 bytes expected");
		var data = new AuthenticatorData { Raw = raw, RpIdHash = new byte[32], Flags = raw[32] };
		Buffer.BlockCopy(raw, 0, data.RpIdHash, 0, 32);
		data.SignCount = (uint)(raw[33] << 24 | raw[34] << 16 | raw[35] << 8 | raw[36]);
		if (data.HasCredential)
		{
			data.Aaguid = new byte[16];
			Buffer.BlockCopy(raw, 37, data.Aaguid, 0, 16);
			var idLength = raw[53] << 8 | raw[54];
			data.CredentialId = new byte[idLength];
			Buffer.BlockCopy(raw, 55, data.CredentialId, 0, idLength);
			// the COSE key is the CBOR item that follows: decode it once to know where it ends
			var reader = new CborReader(raw, 55 + idLength);
			reader.ReadMap();
			data.CredentialPublicKey = new byte[reader.Position - (55 + idLength)];
			Buffer.BlockCopy(raw, 55 + idLength, data.CredentialPublicKey, 0, data.CredentialPublicKey.Length);
		}
		return data;
	}
}
