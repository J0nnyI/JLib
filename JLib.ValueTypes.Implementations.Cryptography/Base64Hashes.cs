using System.Security.Cryptography;

using JLib.Helper;

namespace JLib.ValueTypes.Implementations.Cryptography;

public static class Base64Hashes
{
    public abstract record Base64Hash(string Value) : Base64String(Value), IHashValueType;
    public abstract record Base64ShaHash(string Value) : Base64Hash(Value);
    public record Sha1(string Value) : Base64ShaHash(Value)
    {
        public static Sha1 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToBase64String(SHA1.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(28);
        }
    }
    public record Sha256(string Value) : Base64ShaHash(Value)
    {
        public static Sha256 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToBase64String(SHA256.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(44);
        }
    }
    public record Sha384(string Value) : Base64ShaHash(Value)
    {
        public static Sha384 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToBase64String(SHA384.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(64);
        }
    }
    public record Sha512(string Value) : Base64ShaHash(Value)
    {
        public static Sha512 Create(IReadOnlyCollection<byte> value)
            => new(Convert.ToBase64String(SHA512.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(88);
        }
    }

}