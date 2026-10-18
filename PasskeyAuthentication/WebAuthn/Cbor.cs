using System;
using System.Collections.Generic;
using System.Text;

namespace PasskeyAuthentication.WebAuthn;

/// <summary>
/// Minimal CBOR decoder (RFC 8949), enough for WebAuthn attestation objects and COSE keys: unsigned and negative integers, byte and
/// text strings, arrays, maps and the simple values. Integers come back as <see cref="long"/>, byte strings as <c>byte[]</c>, maps as
/// <c>Dictionary&lt;object, object&gt;</c> keyed by <see cref="long"/> or <see cref="string"/>. Indefinite lengths and floats are not
/// used by authenticators and are refused.
/// </summary>
public sealed class CborReader(byte[] data, int offset = 0)
{
	/// <summary>Position after the last item read: tells how many bytes a COSE key embedded in authenticator data occupies.</summary>
	public int Position { get; private set; } = offset;

	public object Read()
	{
		var initial = data[Position++];
		int major = initial >> 5, info = initial & 0x1f;
		var argument = info switch
		{
			< 24 => (ulong)info,
			24 => data[Position++],
			25 => ReadBigEndian(2),
			26 => ReadBigEndian(4),
			27 => ReadBigEndian(8),
			_ => throw new NotSupportedException("CBOR: indefinite length or reserved additional information"),
		};
		switch (major)
		{
			case 0: return (long)argument;
			case 1: return -1L - (long)argument;
			case 2: return ReadBytes((int)argument);
			case 3: return Encoding.UTF8.GetString(ReadBytes((int)argument));
			case 4:
				var list = new List<object>((int)argument);
				for (ulong i = 0; i < argument; i++)
					list.Add(Read());
				return list;
			case 5:
				var map = new Dictionary<object, object>((int)argument);
				for (ulong i = 0; i < argument; i++)
					map[Read()] = Read();
				return map;
			case 6: return Read();	// tag: the tagged item itself
			default:
				return argument switch
				{
					20 => false,
					21 => true,
					22 or 23 => null,
					_ => throw new NotSupportedException("CBOR: floating point values are not supported"),
				};
		}
	}

	/// <summary>Reads a map, the only top-level shape WebAuthn uses.</summary>
	public Dictionary<object, object> ReadMap() =>
		Read() as Dictionary<object, object> ?? throw new FormatException("CBOR: a map was expected");

	ulong ReadBigEndian(int count)
	{
		ulong value = 0;
		for (var i = 0; i < count; i++)
			value = value << 8 | data[Position++];
		return value;
	}

	byte[] ReadBytes(int count)
	{
		if (Position + count > data.Length)
			throw new FormatException("CBOR: truncated data");
		var bytes = new byte[count];
		Buffer.BlockCopy(data, Position, bytes, 0, count);
		Position += count;
		return bytes;
	}
}

public static class CborExtensions
{
	/// <summary>Value of a map entry by integer or text key, null when absent.</summary>
	public static object Get(this Dictionary<object, object> map, object key) => map.TryGetValue(key is int i ? (long)i : key, out var v) ? v : null;

	public static byte[] GetBytes(this Dictionary<object, object> map, object key) => map.Get(key) as byte[];
	public static long? GetInt(this Dictionary<object, object> map, object key) => map.Get(key) as long?;
	public static string GetText(this Dictionary<object, object> map, object key) => map.Get(key) as string;
}
