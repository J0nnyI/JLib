using FluentAssertions;
using Xunit;

namespace JLib.Helper.Tests;

public class TypeHelperTests
{
    public class SubA
    {
        public class SubAa
        {
        }
    }

    public class SubA<T1>
    {
        public class SubAa<T2>
        {
        }
    }

    [Fact]
    public void GetNestingParentsTest()
    {
        typeof(SubA.SubAa).GetDeclaringTypeTree().Should().ContainInOrder(new[]
        {
            typeof(TypeHelperTests), typeof(SubA), typeof(SubA.SubAa)
        });
    }

    [Fact]
    public void GetNestingParentsTest2()
    {
        typeof(SubA<int>.SubAa<string>).GetDeclaringTypeTree().Should().ContainInOrder(new[]
        {
            typeof(TypeHelperTests), typeof(SubA<int>), typeof(SubA<int>.SubAa<string>)
        });
    }


    [Fact]
    public void GetDeclaringTypeTree()
    {
        typeof(SubA.SubAa).GetDeclaringTypeTree().Should().ContainInOrder(new[]
        {
            typeof(TypeHelperTests), typeof(SubA), typeof(SubA.SubAa)
        });
    }
    [Fact]
    public void DefinedInNamespace()
    {
        typeof(SubA<int>.SubAa<string>).IsDefinedInNamespace("JLib", true).Should().BeTrue();
        typeof(SubA<int>.SubAa<string>).IsDefinedInNamespace("JLib", false).Should().BeFalse();
        typeof(SubA<int>.SubAa<string>).IsDefinedInNamespace("JLib.Helper.Tests", false).Should().BeTrue();
    }
}