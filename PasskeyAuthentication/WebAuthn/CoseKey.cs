using System;
using System.Security.Cryptography;

namespace PasskeyAuthentication.WebAuthn;

/// <summary>
/// Public key of a passkey, in COSE_Key format (RFC 8152 § 7, § 13): the CBOR map the authenticator puts in the attested credential
/// data. The sample accepts the two algorithms every WebAuthn authenticator supports: ES256 (COSE -7, EC P-256) and RS256
/// (COSE -257, RSA PKCS#1 v1.5). Windows Hello backed by a TPM often produces RS256, so both are required.
/// </summary>
public static class CoseKey
{
	public const long ES256 = -7, RS256 = -257;

	// COSE_Key labels
	const long Kty = 1, Alg = 3;
	const long KtyEc2 = 2, KtyRsa = 3;
	const long Crv = -1, X = -2, Y = -3;		// EC2
	const long N = -1, E = -2;				// RSA
	const long P256 = 1;

	/// <summary>Algorithm identifier of a COSE key, or null when the key is not one the sample supports.</summary>
	public static long? Algorithm(byte[] coseKey)
	{
		var key = new CborReader(coseKey).ReadMap();
		return (key.GetInt(Kty), key.GetInt(Alg)) switch
		{
			(KtyEc2, ES256) when key.GetInt(Crv) == P256 => ES256,
			(KtyRsa, RS256) => RS256,
			_ => null,
		};
	}

	/// <summary>Checks a WebAuthn signature: <c>authenticatorData || SHA-256(clientDataJSON)</c> signed with the registered key.</summary>
	public static bool Verify(byte[] coseKey, byte[] signedData, byte[] signature)
	{
		var key = new CborReader(coseKey).ReadMap();
		switch (Algorithm(coseKey))
		{
			case ES256:
			{
				using var ecdsa = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, Q = new ECPoint { X = key.GetBytes(X), Y = key.GetBytes(Y) } });
				// authenticators sign in ASN.1 DER; .NET Framework expects the raw r || s form (IEEE P1363)
				return ecdsa.VerifyData(signedData, DerToP1363(signature, 32), HashAlgorithmName.SHA256);
			}
			case RS256:
			{
				using var rsa = RSA.Create();
				rsa.ImportParameters(new RSAParameters { Modulus = key.GetBytes(N), Exponent = key.GetBytes(E) });
				return rsa.VerifyData(signedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			}
			default:
				return false;
		}
	}

	// SEQUENCE { INTEGER r, INTEGER s } -> r || s, each integer left-padded to the field size, leading zero bytes removed
	static byte[] DerToP1363(byte[] der, int size)
	{
		if (der.Length < 8 || der[0] != 0x30)
			throw new FormatException("ECDSA signature: DER sequence expected");
		var pos = der[1] == 0x81 ? 3 : 2;		// long form length for signatures over 127 bytes
		var result = new byte[size * 2];
		for (var part = 0; part < 2; part++)
		{
			if (der[pos++] != 0x02)
				throw new FormatException("ECDSA signature: DER integer expected");
			int length = der[pos++], start = pos;
			pos += length;
			while (length > size && der[start] == 0) { start++; length--; }
			Buffer.BlockCopy(der, start, result, part * size + size - length, length);
		}
		return result;
	}
}
