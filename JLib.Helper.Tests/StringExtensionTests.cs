using FluentAssertions;
using Xunit;

namespace JLib.Helper.Tests;

public class StringExtensionTests
{
    [Fact]
    public void SubStringUntilPositive() 
        => "ABCDEFG".SubStringUntil('D').Should().Be("ABC");
    [Fact]
    public void SubStringUntilNegative()
        => "ABCDEFG".SubStringUntil('Z').Should().Be("ABCDEFG");
}