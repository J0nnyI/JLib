using System.Security.Cryptography;

// ReSharper disable UnusedMember.Local

namespace JLib.ValueTypes.Implementations.Cryptography;

public static class BinaryHashes
{
    public abstract record BinaryHash(IReadOnlyCollection<byte> Value) : ValueType<IReadOnlyCollection<byte>>(Value), IHashValueType;
    public abstract record ShaHash(IReadOnlyCollection<byte> Value) : BinaryHash(Value);
    public record Sha1(IReadOnlyCollection<byte> Value) : ShaHash(Value), ISha1ValueType
    {
        public static Sha1 Create(IReadOnlyCollection<byte> value)
            => new(SHA1.HashData(value is byte[] arr ? arr : value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(20);
        }
    }
    public record Sha256(IReadOnlyCollection<byte> Value) : ShaHash(Value), ISha256ValueType
    {
        public static Sha256 Create(IReadOnlyCollection<byte> value)
            => new(SHA256.HashData(value is byte[] arr ? arr : value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(32);
        }
    }
    public record Sha384(IReadOnlyCollection<byte> Value) : ShaHash(Value), ISha384ValueType
    {
        public static Sha384 Create(IReadOnlyCollection<byte> value)
            => new(SHA384.HashData(value is byte[] arr ? arr : value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(48);
        }
    }
    public record Sha512(IReadOnlyCollection<byte> Value) : ShaHash(Value), ISha512ValueType
    {
        public static Sha512 Create(IReadOnlyCollection<byte> value)
            => new(SHA512.HashData(value is byte[] arr ? arr : value.ToArray()));
        [Validation]
        private static void Validate(ValidationContext<IReadOnlyCollection<byte>> must)
        {
            must.HaveCount(64);
        }
    }
}