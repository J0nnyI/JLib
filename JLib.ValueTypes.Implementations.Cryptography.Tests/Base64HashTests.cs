using FluentAssertions;
using Xunit;

namespace JLib.ValueTypes.Implementations.Cryptography.Tests;

public class Base64HashTests
{
    private static readonly IReadOnlyCollection<byte> Data = new byte[]{ 1, 2, 3, 4, 5, 23, 3, 26, 132, 1 };

    [Fact]
    public void Sha1() 
        => Base64Hashes.Sha1.Create(Data).Value.Should().Be("6gjyAzKFEPZNRy1DZO7yn36wul8=");
    [Fact]
    public void Sha1Short()
    {
        var sut = () => new Base64Hashes.Sha1("6gjyAzKFEPZNRy1DZO7yn36wul8");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha1Long() 
    {
        var sut = () =>new Base64Hashes.Sha1("6gjyAzKFEPZNRy1DZO7yn36wul8==");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha1RegexFail() 
    {
        var sut = () => new Base64Hashes.Sha1("6gjyAzKFEPZNRy1DZO7yn36wul8!");
        sut.Should().Throw<AggregateException>();
    }
    
    
    [Fact]
    public void Sha256() 
        => Base64Hashes.Sha256.Create(Data).Value.Should().Be("fFQObRAjsJPD+CG3xskM2bY5y+0m4ljL8lptBOxE0RQ=");
    [Fact]
    public void Sha256Short()
    {
        var sut = () => new Base64Hashes.Sha256("fFQObRAjsJPD+CG3xskM2bY5y+0m4ljL8lptBOxE0RQ");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha256Long() 
    {
        var sut = () =>new Base64Hashes.Sha256("fFQObRAjsJPD+CG3xskM2bY5y+0m4ljL8lptBOxE0RQ==");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha256RegexFail() 
    {
        var sut = () => new Base64Hashes.Sha256("fFQObRAjsJPD+CG3xskM2bY5y+0m4ljL8lptBOxE0RQ!");
        sut.Should().Throw<AggregateException>();
    }
    
    [Fact]
    public void Sha384() 
        => Base64Hashes.Sha384.Create(Data).Value.Should().Be("D8OG2XO6hkSp5byVYn99xoybLLXFxTnMofiboMnq6FwrOyOj++WTh6Zn40T1JwCr");
    [Fact]
    public void Sha384Short()
    {
        var sut = () => new Base64Hashes.Sha384("D8OG2XO6hkSp5byVYn99xoybLLXFxTnMofiboMnq6FwrOyOj++WTh6Zn40T1JwC");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha384Long() 
    {
        var sut = () =>new Base64Hashes.Sha384("D8OG2XO6hkSp5byVYn99xoybLLXFxTnMofiboMnq6FwrOyOj++WTh6Zn40T1JwCrr");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha384RegexFail() 
    {
        var sut = () => new Base64Hashes.Sha384("D8OG2XO6hkSp5byVYn99xoybLLXFxTnMofiboMnq6FwrOyOj++WTh6Zn40T1JwC!");
        sut.Should().Throw<AggregateException>();
    }
    
    
    [Fact]
    public void Sha512() 
        => Base64Hashes.Sha512.Create(Data).Value.Should().Be("UmruECSmlr7QJ3jlwlDOFyD8GaeuEurhtp65PIo+7wBSKmoj0GkZ73emTnB56euRFtZdp8w53TCiq3OCru/fLw==");
    [Fact]
    public void Sha512Short()
    {
        var sut = () => new Base64Hashes.Sha512("UmruECSmlr7QJ3jlwlDOFyD8GaeuEurhtp65PIo+7wBSKmoj0GkZ73emTnB56euRFtZdp8w53TCiq3OCru/fLw=");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha512Long() 
    {
        var sut = () =>new Base64Hashes.Sha512("UmruECSmlr7QJ3jlwlDOFyD8GaeuEurhtp65PIo+7wBSKmoj0GkZ73emTnB56euRFtZdp8w53TCiq3OCru/fLw===");
        sut.Should().Throw<AggregateException>();
    }
    [Fact]
    public void Sha512512RegexFail() 
    {
        var sut = () => new Base64Hashes.Sha512("UmruECSmlr7QJ3jlwlDOFyD8GaeuEurhtp65PIo+7wBSKmoj0GkZ73emTnB56euRFtZdp8w53TCiq3OCru/fLw=!");
        sut.Should().Throw<AggregateException>();
    }
}