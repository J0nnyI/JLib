using System.Reflection;

using Castle.Components.DictionaryAdapter;

using FluentAssertions;

using JLib.DependencyInjection;
using JLib.Helper;
using JLib.ValueTypes;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public abstract class TestingIdGeneratorTests : IDisposable
{
    void IDisposable.Dispose()
    {
        _disposables.DisposeAll();
        GC.SuppressFinalize(this);
    }

    class Derived<T>(TestingIdGenerator idGenerator) : Nested<T>(idGenerator);

    class Nested<T>(TestingIdGenerator idGenerator)
    {
        public Guid CreateId<T2, T3>()
            => CreateId<T2>(1);

        public Guid CreateId<T2>(int stackTraceFrameIndex = 0)
            => idGenerator.CreateGuid(stackTraceFrameIndex);

        public Guid CreateIdViaAnonymous<T2>(int stackTraceFrameIndex = 0)
        {
            var x = () => idGenerator.CreateGuid(stackTraceFrameIndex);
            return x();
        }

        public Guid CreateId<T2>(string stringParam)
            => CreateId<T2>(1);

        public Guid CreateIdNested<T2>()
            => CreateId<T2>(1);
    }

    private readonly List<IDisposable> _disposables = [];
    private readonly TestingIdGenerator _idGenerator;
    private readonly IIdRegistry _idRegistry;
    private readonly IServiceProvider _provider;


    protected TestingIdGeneratorTests(ITestOutputHelper toh)
    {
        // No .AddAutoMapper(...) here: TestingIdGenerator must resolve and create typed ids without AutoMapper.
        var provider = new ServiceCollection()
            .AddLogging(x=>x.AddXunit(toh))
            .AddTestingIdGenerator()
            .AddIdRegistry(new() { NamespaceAliases = [new("JLib.DataGeneration.Tests")] })
            .AddSingleton(typeof(Nested<>))
            .AddSingleton(typeof(Derived<>))
            .BuildServiceProvider();
        _disposables.Add(provider);
        provider.GetRequiredServices(out _idGenerator, out _idRegistry);
        _provider = provider;

    }

    public class CreateGuidTests : TestingIdGeneratorTests
    {
        public CreateGuidTests(ITestOutputHelper toh) : base(toh) { }

        [Fact]
        public void CreateGuid_AnonymousMethod()
        {
            var a = _provider.GetRequiredService<Nested<int>>()
                .CreateIdViaAnonymous<double>();
            a.Should().Be("a249b3a5-c3ed-4562-9e6a-23c8e2f27617");
        }
        [Fact]
        public void CreateGuid_ShouldReturnNewGuid()
        {
            _idGenerator.CreateGuid().Should().Be("44bece3b-3bf2-4487-8a12-e93a2fc23305");
        }

        [Fact]
        public void CreateGuid_ShouldReturnNewGuid2()
        {
            _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>()
                .Should().Be("75e01bcf-31cf-4601-80b3-bf7935278d54"
                );
            ;
        }

        [Fact]
        public void CreateGuid_ShouldReturnNewGuid3()
        {
            var a = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();
            var b = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();
            a.Should().Be("75e01bcf-31cf-4601-80b3-bf7935278d54");
            b.Should().Be("bb4eed1d-fa1b-47be-9726-cef1aea7c2b9");
            a.Should().NotBe(b);
        }

        [Fact]
        public void CreateGuid_ShouldReturnNewGuid4()
        {
            var a = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();
            var b = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();

            _idGenerator.SetIdScope(new("otherScope"));

            var c = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();

            a.Should().Be("75e01bcf-31cf-4601-80b3-bf7935278d54");
            b.Should().Be("bb4eed1d-fa1b-47be-9726-cef1aea7c2b9");
            c.Should().Be("e375ff91-129e-4cf1-8360-3306b9228c0a");
            a.Should().NotBe(b).And.NotBe(c);
        }

        [Fact]
        public void CreateGuid_ShouldReturnNewGuid5()
        {
            var a = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double>();
            var b = _provider.GetRequiredService<Nested<int>>()
                .CreateId<double, int>();
            a.Should().Be("75e01bcf-31cf-4601-80b3-bf7935278d54");
            b.Should().Be("c0614c1f-41c8-4188-bda7-04b2d348a24d");
            a.Should().NotBe(b);
        }
    }

    public class GetIdInfoTests : TestingIdGeneratorTests
    {
        public GetIdInfoTests(ITestOutputHelper toh) : base(toh) { }

        [Fact]
        public void ReverseLookupForStartTimeIntIdAsString()
        {
            2.IdInfo(_idRegistry)
                .Should().Be(
                    "Int32 [~.ValidDataPackages.Test3Dp].[~.ValidDataPackages.Base1Dp<~.DataPackageTestBase.TestTypeId>.Id2] = 2");
        }

        [Fact]
        public void ReverseLookupForStartTimeIntIdAsObject()
        {
            const int id = 2;
            var info = id
                .IdInfoObj(_idRegistry)
                .ToSnapshotInfo(true);

            info.IdGroupName
                .Should().Be("~.ValidDataPackages.Test3Dp");

            info.IdName
                .Should().Be("~.ValidDataPackages.Base1Dp<~.DataPackageTestBase.TestTypeId>.Id2");

            info.IdType
                .Should().Be("Int32");

            info.Value
                .Should().Be(id);
        }

        [Fact]
        public void ReverseLookupForStartTimeGuidAsString()
        {
            Guid.Parse("8ca6e4e4-8e69-4e80-b906-8609475aba84").IdInfo(_idRegistry)
                .Should().Be(
                    "Guid [~.ValidDataPackages.Test3Dp].[~.ValidDataPackages.Base1Dp<~.DataPackageTestBase.TestTypeId>.Id] = 8ca6e4e4-8e69-4e80-b906-8609475aba84");
        }

        [Fact]
        public void ReverseLookupForStartTimeGuidAsObject()
        {
            var id = Guid.Parse("8ca6e4e4-8e69-4e80-b906-8609475aba84");
            var info = id
                .IdInfoObj(_idRegistry)
                .ToSnapshotInfo(true);

            info.IdGroupName
                .Should().Be("~.ValidDataPackages.Test3Dp");

            info.IdName
                .Should().Be("~.ValidDataPackages.Base1Dp<~.DataPackageTestBase.TestTypeId>.Id");

            info.IdType
                .Should().Be("Guid");

            info.Value
                .Should().Be(id);
        }

        [Fact]
        public void ReverseLookupForRuntimeGuidAsString()
        {
            Guid.Parse("75e01bcf-31cf-4601-80b3-bf7935278d54").IdInfo(_idRegistry)
                .Should().Be(
                    "Guid [~.TestingIdGeneratorTests.Nested<T>].[[Default]CreateId<>(System.Int32)-1] = 75e01bcf-31cf-4601-80b3-bf7935278d54");
        }

        [Fact]
        public void ReverseLookupForRuntimeGuidAsObject()
        {
            var id = Guid.Parse("75e01bcf-31cf-4601-80b3-bf7935278d54");
            var info = id
                .IdInfoObj(_idRegistry)
                .ToSnapshotInfo(true);

            info.IdGroupName
                .Should().Be("~.TestingIdGeneratorTests.Nested<T>");

            info.IdName
                .Should().Be("[Default]CreateId<>(System.Int32)-1");

            info.IdType
                .Should().Be("Guid");

            info.Value
                .Should().Be(id);
        }
        [Fact]
        public void ReverseLookupForRuntimeGuidAsDerivedObject()
        {
            var id = Guid.Parse("75e01bcf-31cf-4601-80b3-bf7935278d54");
            var info = id
                .IdInfoObj(_idRegistry)
                .ToSnapshotInfo(true);

            info.IdGroupName
                .Should().Be("~.TestingIdGeneratorTests.Nested<T>");

            info.IdName
                .Should().Be("[Default]CreateId<>(System.Int32)-1");

            info.IdType
                .Should().Be("Guid");

            info.Value
                .Should().Be(id);
        }
    }

    public record TestGuidId(Guid Value) : GuidValueType(Value);

    public record TestStringId(string Value) : StringValueType(Value);

    public record TestIntId(int Value) : IntValueType(Value);

    /// <summary>
    /// Covers the typed value-type id overloads (<see cref="TestingIdGenerator.CreateGuid{TId}()"/>,
    /// <see cref="TestingIdGenerator.CreateStringId{TId}"/>, <see cref="TestingIdGenerator.CreateIntId{TId}"/>).
    /// These previously routed through AutoMapper and had no coverage; they now use <c>ValueType.Create</c>.
    /// </summary>
    public class TypedValueTypeIdTests : TestingIdGeneratorTests
    {
        public TypedValueTypeIdTests(ITestOutputHelper toh) : base(toh) { }

        [Fact]
        public void CreateGuid_Typed_WrapsRegisteredGuid()
        {
            var id = _idGenerator.CreateGuid<TestGuidId>();
            id.Should().NotBeNull();
            id.Value.Should().NotBe(Guid.Empty);
            // reverse lookup proves the wrapped value is the registry-generated id (same path the raw overload uses)
            id.Value.IdInfo(_idRegistry).Should().Contain("Guid").And.Contain(id.Value.ToString());
        }

        [Fact]
        public void CreateGuid_Typed_IsDistinctPerCall()
        {
            var a = _idGenerator.CreateGuid<TestGuidId>();
            var b = _idGenerator.CreateGuid<TestGuidId>();
            a.Should().NotBe(b);
        }

        [Fact]
        public void CreateIntId_Typed_WrapsRegisteredInt()
        {
            var id = _idGenerator.CreateIntId<TestIntId>();
            id.Should().NotBeNull();
            id.Value.IdInfo(_idRegistry).Should().Contain("Int32").And.Contain(id.Value.ToString());
        }

        [Fact]
        public void CreateIntId_Typed_IsDistinctPerCall()
        {
            var a = _idGenerator.CreateIntId<TestIntId>();
            var b = _idGenerator.CreateIntId<TestIntId>();
            a.Should().NotBe(b);
        }

        [Fact]
        public void CreateStringId_Typed_WrapsNonEmptyValue()
        {
            var id = _idGenerator.CreateStringId<TestStringId>();
            id.Should().NotBeNull();
            id.Value.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CreateStringId_Typed_IsDistinctPerCall()
        {
            var a = _idGenerator.CreateStringId<TestStringId>();
            var b = _idGenerator.CreateStringId<TestStringId>();
            a.Should().NotBe(b);
        }
    }

}
