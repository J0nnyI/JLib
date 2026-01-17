using System.Collections.Concurrent;
using FluentAssertions;
using JLib.Helper;
using Xunit;

namespace JLib.Helper.Tests;

public class EnumerableExtensionsTests
{
    [Fact]
    public void ConcurrentDictionary_RemoveWhere_ShouldRemoveMatchingItems()
    {
        // Arrange
        var dict = new ConcurrentDictionary<string, int>();
        dict.TryAdd("keep1", 1);
        dict.TryAdd("remove1", 2);
        dict.TryAdd("keep2", 3);
        dict.TryAdd("remove2", 4);

        // Act
        dict.RemoveWhere(kvp => kvp.Key.StartsWith("remove"));

        // Assert
        dict.Should().HaveCount(2);
        dict.Keys.Should().Contain(new[] { "keep1", "keep2" });
        dict.Keys.Should().NotContain(new[] { "remove1", "remove2" });
    }

    [Fact]
    public void ConcurrentDictionary_RemoveWhere_ByValue_ShouldRemoveMatchingItems()
    {
        // Arrange
        var dict = new ConcurrentDictionary<string, int>();
        dict.TryAdd("a", 10);
        dict.TryAdd("b", 20);
        dict.TryAdd("c", 30);

        // Act
        dict.RemoveWhere(kvp => kvp.Value > 15);

        // Assert
        dict.Should().HaveCount(1);
        dict.Keys.Should().Contain("a");
        dict.Keys.Should().NotContain(new[] { "b", "c" });
    }
}
