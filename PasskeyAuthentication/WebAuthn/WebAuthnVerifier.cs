using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PasskeyAuthentication.WebAuthn;

/// <summary>
/// Relying party checks of WebAuthn Level 2, § 7.1 (registration) and § 7.2 (authentication), for one relying party id and origin.
/// Attestation statements are not verified: the sample accepts any authenticator (format <c>none</c> or other), which is what most
/// web sites do. A site that must trust only some authenticator models adds the attestation chain check here.
/// </summary>
public sealed class WebAuthnVerifier(string rpId, string origin)
{
	public string RpId { get; } = rpId;
	public string Origin { get; } = origin;

	/// <summary>Public key, credential id and AAGUID of a new passkey, once the attestation is checked against the challenge sent.</summary>
	public AuthenticatorData VerifyRegistration(byte[] clientDataJson, byte[] attestationObject, byte[] challenge)
	{
		CheckClientData(clientDataJson, "webauthn.create", challenge);

		// attestationObject = CBOR { fmt, attStmt, authData }
		var attestation = new CborReader(attestationObject).ReadMap();
		var authData = AuthenticatorData.Parse(attestation.GetBytes("authData") ?? throw new FormatException("attestation: authData missing"));
		CheckAuthenticatorData(authData);
		if (!authData.HasCredential)
			throw new CryptographicException("attestation: no attested credential data");
		if (CoseKey.Algorithm(authData.CredentialPublicKey) == null)
			throw new CryptographicException("attestation: unsupported key algorithm, ES256 or RS256 expected");
		return authData;
	}

	/// <summary>Checks an assertion against the registered key and returns its signature counter.</summary>
	public uint VerifyAssertion(byte[] clientDataJson, byte[] authenticatorData, byte[] signature, byte[] challenge, byte[] publicKey, uint storedSignCount)
	{
		CheckClientData(clientDataJson, "webauthn.get", challenge);
		var authData = AuthenticatorData.Parse(authenticatorData);
		CheckAuthenticatorData(authData);

		using var sha = SHA256.Create();
		if (!CoseKey.Verify(publicKey, [.. authenticatorData, .. sha.ComputeHash(clientDataJson)], signature))
			throw new CryptographicException("assertion: invalid signature");

		// a counter that does not grow means a cloned authenticator (or one that does not count: both at 0)
		if (authData.SignCount != 0 || storedSignCount != 0)
			if (authData.SignCount <= storedSignCount)
				throw new CryptographicException($"assertion: signature counter {authData.SignCount} not above {storedSignCount}, possible cloned authenticator");
		return authData.SignCount;
	}

	void CheckClientData(byte[] clientDataJson, string type, byte[] challenge)
	{
		var client = Json.Deserialize<ClientData>(clientDataJson);
		if (client.Type != type)
			throw new CryptographicException($"clientData: type '{client.Type}', '{type}' expected");
		if (!Base64Url.Decode(client.Challenge ?? "").SequenceEqual(challenge))
			throw new CryptographicException("clientData: challenge does not match the one sent");
		if (!string.Equals(client.Origin, Origin, StringComparison.OrdinalIgnoreCase))
			throw new CryptographicException($"clientData: origin '{client.Origin}', '{Origin}' expected");
	}

	void CheckAuthenticatorData(AuthenticatorData authData)
	{
		using var sha = SHA256.Create();
		if (!sha.ComputeHash(Encoding.UTF8.GetBytes(RpId)).SequenceEqual(authData.RpIdHash))
			throw new CryptographicException($"authenticator data: rpIdHash is not SHA-256('{RpId}')");
		if (!authData.UserPresent)
			throw new CryptographicException("authenticator data: user presence flag missing");
		// a passkey with user verification (PIN, face, fingerprint) is two factors: the server requires it
		if (!authData.UserVerified)
			throw new CryptographicException("authenticator data: user verification flag missing");
	}
}
