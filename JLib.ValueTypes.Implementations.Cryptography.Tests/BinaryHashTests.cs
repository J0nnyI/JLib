using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Implementations.Cryptography.Tests;

public class BinaryHashTests
{
    private static readonly IReadOnlyCollection<byte> Data = new byte[]{ 1, 2, 3, 4, 5, 23, 3, 26, 132, 1 };
    [Fact]
    public void Sha1()
    {
        BinaryHashes.Sha1.Create(Data).Value.Should()
            . BeEquivalentTo(new byte[]
            {
                0xEA, 0x08, 0xF2, 0x03, 0x32, 0x85, 0x10, 0xF6, 0x4D, 0x47, 0x2D, 0x43, 0x64, 0xEE, 0xF2, 0x9F, 0x7E, 
                0xB0, 0xBA, 0x5F
            });
    }
    [Fact]
    public void Sha1Short()
    {
        var sut  = ()=> new BinaryHashes.Sha1(new BinaryHashes.Sha1(Data).Value.Skip(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha1Long()
    {
        var sut  = ()=> new BinaryHashes.Sha1(new BinaryHashes.Sha1(Data).Value.Append<byte>(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha256()
    {
        BinaryHashes.Sha256.Create(Data).Value.Should()
            . BeEquivalentTo(new byte[]
            {
                0x7C, 0x54, 0x0E, 0x6D, 0x10, 0x23, 0xB0, 0x93, 0xC3, 0xF8, 0x21, 0xB7, 0xC6, 0xC9, 0x0C, 0xD9, 0xB6, 
                0x39, 0xCB, 0xED, 0x26, 0xE2, 0x58, 0xCB, 0xF2, 0x5A, 0x6D, 0x04, 0xEC, 0x44, 0xD1, 0x14
            }.ToArray());
    }
    [Fact]
    public void Sha256Short()
    {
        var sut  = ()=> new BinaryHashes.Sha256(new BinaryHashes.Sha256(Data).Value.Skip(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha256Long()
    {
        var sut  = ()=> new BinaryHashes.Sha256(new BinaryHashes.Sha256(Data).Value.Append<byte>(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha384()
    {
        BinaryHashes.Sha384.Create(Data).Value.Should()
            . BeEquivalentTo(new byte[]
            {
                0x0F, 0xC3, 0x86, 0xD9, 0x73, 0xBA, 0x86, 0x44, 0xA9, 0xE5, 0xBC, 0x95, 0x62, 0x7F, 0x7D, 0xC6, 0x8C, 
                0x9B, 0x2C, 0xB5, 0xC5, 0xC5, 0x39, 0xCC, 0xA1, 0xF8, 0x9B, 0xA0, 0xC9, 0xEA, 0xE8, 0x5C, 0x2B, 0x3B, 
                0x23, 0xA3, 0xFB, 0xE5, 0x93, 0x87, 0xA6, 0x67, 0xE3, 0x44, 0xF5, 0x27, 0x00, 0xAB
            }.ToArray());
    }
    [Fact]
    public void Sha384Short()
    {
        var sut  = ()=> new BinaryHashes.Sha384(new BinaryHashes.Sha384(Data).Value.Skip(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha384Long()
    {
        var sut  = ()=> new BinaryHashes.Sha384(new BinaryHashes.Sha384(Data).Value.Append<byte>(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha512()
    {
        BinaryHashes.Sha512.Create(Data).Value.Should()
            . BeEquivalentTo(new byte[]
            {
                0x52, 0x6A, 0xEE, 0x10, 0x24, 0xA6, 0x96, 0xBE, 0xD0, 0x27, 0x78, 0xE5, 0xC2, 0x50, 0xCE, 0x17, 0x20,
                0xFC, 0x19, 0xA7, 0xAE, 0x12, 0xEA, 0xE1, 0xB6, 0x9E, 0xB9, 0x3C, 0x8A, 0x3E, 0xEF, 0x00, 0x52, 0x2A,
                0x6A, 0x23, 0xD0, 0x69, 0x19, 0xEF, 0x77, 0xA6, 0x4E, 0x70, 0x79, 0xE9, 0xEB, 0x91, 0x16, 0xD6, 0x5D, 
                0xA7, 0xCC, 0x39, 0xDD, 0x30, 0xA2, 0xAB, 0x73, 0x82, 0xAE, 0xEF, 0xDF, 0x2F
            }.ToArray());
    }
    [Fact]
    public void Sha512Short()
    {
        var sut  = ()=> new BinaryHashes.Sha512(new BinaryHashes.Sha512(Data).Value.Skip(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha512Long()
    {
        var sut  = ()=> new BinaryHashes.Sha512(new BinaryHashes.Sha512(Data).Value.Append<byte>(1).ToArray());
        sut.Should().Throw<AggregateException>();
    }
}