using System.Collections.Immutable;
using System.Reflection;
using FluentAssertions;
using JLib.Exceptions;
using JLib.Helper;
using Xunit;

namespace JLib.Reflection.Tests;

public class AssemblyLoaderExtensionTests
{
    [Fact]
    public void LoadRecursivePeerDependencies()
    {
        var assemblies = ImmutableHashSet.Create(DemoAssemblyContent.Assembly1.GetName()).ToReadOnlyCollection();
        var result = assemblies
            .LoadRecursivePeerDependencies(
                new(nameof(LoadRecursivePeerDependencies)),
                maxDependencyDepth: 99);
        result.Should()
            .Contain(DemoAssemblyContent.Assembly1A);
    }

    [Fact]
    public void TryLoadAll()
    {
        var assemblies = ImmutableHashSet.Create([DemoAssemblyContent.Assembly1.GetName(), DemoAssemblyContent.Assembly2.GetName()]);
        var result = assemblies.TryLoadAll(
            new(nameof(TryLoadAll))
        );
        result
            .Should()
            .Contain([DemoAssemblyContent.Assembly1, DemoAssemblyContent.Assembly2]);
    }

    [Fact]
    public void TryLoad_ShouldThrowException()
    {
        var assembly = new AssemblyName("foo");
        var exceptions = new ExceptionBuilder(nameof(TryLoad_ShouldThrowException));
        var result = assembly.TryLoad(
            exceptions
        );

        result
            .Should()
            .BeNull();

        exceptions
            .GetException()
            .FlattenAll()
            .OfType<AssemblyLoadFailedException>()
            .Should()
            .HaveCount(1);
    }
}