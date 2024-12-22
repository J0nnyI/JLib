using System.Security.Cryptography;
using JLib.Helper;

// ReSharper disable UnusedMember.Local

namespace JLib.ValueTypes.Implementations.Cryptography;

public static class BinaryHashes
{ 
    public abstract record ShaHash(IReadOnlyCollection<byte> Value) : ValueType<IReadOnlyCollection<byte>>(Value);
    public record Sha1(IReadOnlyCollection<byte> Value) : ShaHash(Value)
    {
        public static Sha1 Create(IReadOnlyCollection<byte> value)
            => new (SHA1.HashData(value is byte[] arr?arr: value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(20);
        }
    }
    public record Sha256(IReadOnlyCollection<byte> Value) : ShaHash(Value)
    {
        public static Sha256 Create(IReadOnlyCollection<byte> value)
            => new (SHA256.HashData(value is byte[] arr?arr: value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(32);
        }
    }
    public record Sha384(IReadOnlyCollection<byte> Value) : ShaHash(Value)
    {
        public static Sha384 Create(IReadOnlyCollection<byte> value)
            => new (SHA384.HashData(value is byte[] arr?arr: value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(48);
        }
    }
    public record Sha512(IReadOnlyCollection<byte> Value) : ShaHash(Value)
    {
        public static Sha512 Create(IReadOnlyCollection<byte> value)
            => new (SHA512.HashData(value is byte[] arr?arr: value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(64);
        }
    }
}

public static class Base64Hashes
{
    public abstract record Base64Hash(string Value) : Base64String(Value)
    {
        
    }
    public abstract record Base64ShaHash(string Value) : Base64Hash(Value);
    public record Sha1(string Value) : Base64ShaHash(Value)
    {
        public static Sha1 Create(IReadOnlyCollection<byte> value)
            => new (Convert.ToBase64String(SHA1.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(28);
        }
    }
    public record Sha256(string Value) : Base64ShaHash(Value)
    {
        public static Sha256 Create(IReadOnlyCollection<byte> value)
            => new (Convert.ToBase64String(SHA256.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(44);
        }
    }
    public record Sha384(string Value) : Base64ShaHash(Value)
    {
        public static Sha384 Create(IReadOnlyCollection<byte> value)
            => new (Convert.ToBase64String(SHA384.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(64);
        }
    }
    public record Sha512(string Value) : Base64ShaHash(Value)
    {
        public static Sha512 Create(IReadOnlyCollection<byte> value)
            => new (Convert.ToBase64String(SHA512.HashData(value.GetArray())));
        [Validation]
        private static void Validate(ValidationContext<string> must)
        {
            must.BeOfLength(88);
        }
    }
    
}