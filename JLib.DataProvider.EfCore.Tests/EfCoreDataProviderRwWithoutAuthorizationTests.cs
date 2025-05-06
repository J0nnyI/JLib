using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataProvider.EfCore.Tests;

public class EfCoreDataProviderRwWithoutAuthorizationTests(ITestOutputHelper toh) : EfCoreDataProviderRwTestBase(toh)
{
    [Fact]
    public void ReturnsEverything()
        => DataProvider.Get().Should().HaveCount(4);
}