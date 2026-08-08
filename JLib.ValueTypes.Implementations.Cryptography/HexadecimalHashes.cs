using System.Security.Cryptography;

using JLib.Helper;

namespace JLib.ValueTypes.Implementations.Cryptography;

public static class HexadecimalHashes
{
    public abstract record HexadecimalHash(string Value) : Base64String(Value), IHashValueType;
    public abstract record HexadecimalShaHash(string Value) : HexadecimalHash(Value);
    public record Sha1(string Value) : HexadecimalShaHash(Value), ISha1ValueType
    {
        public static Sha1 Create(Stream dataStream)
            => new(Convert.ToHexString(SHA1.HashData(dataStream)));
        public static Sha1 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToHexString(SHA1.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(28);
        }
    }
    public record Sha256(string Value) : HexadecimalShaHash(Value), ISha256ValueType
    {
        public static Sha256 Create(Stream dataStream)
            => new(Convert.ToHexString(SHA256.HashData(dataStream)));
        public static Sha256 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToHexString(SHA256.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(44);
        }
    }
    public record Sha384(string Value) : HexadecimalShaHash(Value), ISha384ValueType
    {
        public static Sha384 Create(Stream dataStream)
            => new(Convert.ToHexString(SHA384.HashData(dataStream)));
        public static Sha384 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToHexString(SHA384.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(64);
        }
    }
    public record Sha512(string Value) : HexadecimalShaHash(Value), ISha512ValueType
    {
        public static Sha512 Create(Stream dataStream)
            => new(Convert.ToHexString(SHA512.HashData(dataStream)));
        public static Sha512 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToHexString(SHA512.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(88);
        }
    }

}