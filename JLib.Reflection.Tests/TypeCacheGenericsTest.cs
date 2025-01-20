using FluentAssertions;

using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;


namespace JLib.Reflection.Tests;

public class TypeCacheGenericsTest : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly TypeCache _cache;
    public void Dispose() => _disposables.DisposeAll();

    public TypeCacheGenericsTest(ITestOutputHelper testOutputHelper)
    {
        var loggerFactory = new LoggerFactory().AddXunit(testOutputHelper);

        var package = TypePackage.GetNested(typeof(Common));
        var exceptions = new ExceptionBuilder(nameof(GetGenericTypeDefinition));
        _cache = new(package, exceptions, loggerFactory);
        _disposables.Add(loggerFactory);
    }

    public static class Common
    {
        public interface IDemoSelectorInterface { }
        [TvtFactoryAttribute.Implements(typeof(IDemoSelectorInterface))]
        public record DemoTypeValueType(Type Value) : TypeValueType(Value);

        public class GenericType<T> : IDemoSelectorInterface { }


        public class NavigatingGenericType<T> : IGenericDemoSelector<T> { }
        public interface IGenericDemoSelector<T>
        {
        }
        [TvtFactoryAttribute.BeGeneric, TvtFactoryAttribute.ImplementsAny(typeof(IGenericDemoSelector<>))]
        public record NavigatingDemoType(Type Value) : NavigatingTypeValueType(Value)
        {
            public DemoTypeValueType? ReferencedType => Navigate(cache
                => Value.IsGenericTypeDefinition
                    ? null
                    : cache.Get<DemoTypeValueType>(Value.GenericTypeArguments.First())
            );
        }
        public interface IInvalidMatch<T> { }
        public class InvalidType<T> : IInvalidMatch<T> { }
        [TvtFactoryAttribute.ImplementsAny(typeof(IInvalidMatch<>))]
        public record InvalidTvt(Type Value) : TypeValueType(Value), IValidatedType
        {
            public void Validate(ITypeCache cache, IValidationContext<Type> value)
            {
                if (value.Value is { IsGenericTypeDefinition: false, IsGenericType: true })
                    value.AddError("error");
            }
        }

        public class Generic1<T>
        {
            private Generic1()
            {

            }

            public class Generic2<T2> : IDemoSelectorInterface
            {
            }
        }

        public class Nested1
        {
            public class Nested2 : IDemoSelectorInterface { }
        }
    }


    [Fact]
    public void DoubleNested()
    {
        _cache.Get<Common.DemoTypeValueType>(typeof(Common.Nested1.Nested2))
            .Value.Should().Be(typeof(Common.Nested1.Nested2));
    }

    [Fact]
    public void GetGenericTypeDefinition()
    {
        _cache.Get<Common.DemoTypeValueType>(typeof(Common.GenericType<>))
            .Value.Should().Be(typeof(Common.GenericType<>));
    }
    [Fact]
    public void GetGenericType()
    {
        _cache.Get<Common.DemoTypeValueType>(typeof(Common.GenericType<int>))
            .Value.Should().Be(typeof(Common.GenericType<int>));
    }
    [Fact]
    public void GetNestedGenericType()
    {
        _cache.Get<Common.DemoTypeValueType>(typeof(Common.Generic1<int>.Generic2<string>))
            .Value.Should().Be(typeof(Common.Generic1<int>.Generic2<string>));
    }
    [Fact]
    public void Navigation()
    {
        var res = _cache.Get<Common.NavigatingDemoType>(typeof(Common.NavigatingGenericType<Common.GenericType<int>>));
        res.ReferencedType.Should().NotBeNull();
        res.ReferencedType?.Value.Should().Be(typeof(Common.GenericType<int>));
    }
    [Fact]
    public void InvalidNavigation()
    {
        var act = () =>
            _cache.Get<NavigatingTypeValueType>(typeof(Common.NavigatingGenericType<int>));
        act.Should().Throw<JLibAggregateException>();
    }

    [Fact]
    public void InvalidTypeArgument()
    {
        // this makes sure that the tvt validation is executed when the type is registered at runtime
        var act = () =>
            _cache.Get<Common.InvalidTvt>(typeof(Common.InvalidType<int>));
        act.Should().Throw<JLibAggregateException>();

    }
}