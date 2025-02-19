using FluentAssertions;

using Xunit;

namespace JLib.DataGeneration.Tests;

public class StartTimeIdGeneratorTests
{
    public class PropertyNotFoundException() : Exception("property not found");
    public class DerivedClassSecondDegree<T> : DerivedClassFirstDegree<T>
    {
        public Guid SecondDegreeId { get; init; }

    }
    public class DerivedClassFirstDegree<T> : BaseClass<T>
    {
        public Guid FirstDegreeId { get; init; }

    }
    public class BaseClass<T>
    {
        public Guid Id { get; init; }
    }


    public static readonly IdRegistryConfiguration Config = new()
    {
        NamespaceAliases =
        [
            new("JLib.DataGeneration.Tests.StartTimeIdGeneratorTests", "Test")
        ]
    };
    public StartTimeIdGeneratorTests()
    {

    }

    private void RunTest(Type type, string propertyName, string expectedIdGroupName, string expectedIdName)
    {
        var identifier = new DataPackageValues.IdIdentifier(
            type.GetProperty(propertyName)
            ?? throw new PropertyNotFoundException(),
            Config);
        identifier.IdGroupName.Value.Should().Be(expectedIdGroupName);
        identifier.IdName.Value.Should().Be(expectedIdName);

    }

    [Fact]
    public void BaseClass_BaseId() => RunTest(typeof(BaseClass<int>), nameof(BaseClass<int>.Id),
        "~Test~.BaseClass<System.Int32>", "Id");
    [Fact]
    public void FirstDegree_BaseId() => RunTest(typeof(DerivedClassFirstDegree<int>), nameof(DerivedClassFirstDegree<int>.Id),
        "~Test~.DerivedClassFirstDegree<System.Int32>", "~Test~.BaseClass<System.Int32>.Id");
    [Fact]
    public void FirstDegree_FirstDegreeId() => RunTest(typeof(DerivedClassFirstDegree<int>), nameof(DerivedClassFirstDegree<int>.FirstDegreeId),
        "~Test~.DerivedClassFirstDegree<System.Int32>", "FirstDegreeId");
    [Fact]
    public void SecondDegree_BaseId() => RunTest(typeof(DerivedClassSecondDegree<int>), nameof(DerivedClassSecondDegree<int>.Id),
        "~Test~.DerivedClassSecondDegree<System.Int32>", "~Test~.BaseClass<System.Int32>.Id");
    [Fact]
    public void SecondDegree_FirstDegreeId() => RunTest(typeof(DerivedClassSecondDegree<int>), nameof(DerivedClassSecondDegree<int>.FirstDegreeId),
        "~Test~.DerivedClassSecondDegree<System.Int32>", "~Test~.DerivedClassFirstDegree<System.Int32>.FirstDegreeId");
    [Fact]
    public void SecondDegree_SecondDegreeId() => RunTest(typeof(DerivedClassSecondDegree<int>), nameof(DerivedClassSecondDegree<int>.SecondDegreeId),
        "~Test~.DerivedClassSecondDegree<System.Int32>", "SecondDegreeId");
}