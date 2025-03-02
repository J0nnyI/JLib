using FluentAssertions;
using Snapshooter.Xunit;
using Xunit;

namespace JLib.Reflection.Tests;

public class TypePackageBuilderTests
{
    [Fact]
    public void EntryAssemblyTest()
    {
        var builder = new TypePackageBuilder()
            .AddEntryAssembly();
        var package =builder
            .Build();
        package.Types.Should().BeEmpty();
        
        package
            .ToJsonObject()
            .MatchSnapshot();
    }
}