using AutoMapper;

using FluentAssertions;

using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public class DataPackageTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private readonly IMapper _mapper;
    private readonly ITypeCache _typeCache;

    public void Dispose()
    {
        _disposables.DisposeAll();
    }

    public DataPackageTests(ITestOutputHelper toh)
    {
        var logger = LoggerFactory.Create(x => x.AddXunit(toh));
        using var exceptions = new ExceptionBuilder(nameof(DataPackageTests));

        var typePackage = TypePackage.Get(null, new[] { "JLib" })
            .ApplyFilter(x => x.Namespace != "JLib.DataGeneration.Tests", "no testing assemblies");

        var x = typePackage.ToJson();

        var provider = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger, typePackage)
            .AddAutoMapper(c => c.AddProfiles(typeCache, logger))
            .AddDataPackages(typeCache, new() { NamespaceAliases = new[] { new NamespaceAlias("JLib.DataGeneration.Tests") } })
            .BuildServiceProvider()
            .DisposeWith(_disposables);

        exceptions.ThrowIfNotEmpty();
        _mapper = provider.GetRequiredService<IMapper>();
        _typeCache = provider.GetRequiredService<ITypeCache>();
    }

    [Fact]
    public void SetupWorks()
    {
        _typeCache.KnownTypeValueTypes.Should().Contain(typeof(ValueTypeType));
        _typeCache.KnownTypes.Should().Contain(typeof(DataPackageValues.IdGroupName));
        _typeCache.KnownTypes.Should().Contain(typeof(DataPackageValues.IdName));
        _typeCache.KnownTypes.Should().Contain(typeof(DataPackageValues.IdIdentifier));
        _typeCache.Get<ValueTypeType>(typeof(DataPackageValues.IdGroupName)).Value.Should()
            .Be(typeof(DataPackageValues.IdGroupName));
        _typeCache.Get<ValueTypeType>(typeof(DataPackageValues.IdName)).Value.Should()
            .Be(typeof(DataPackageValues.IdName));
    }

    [Fact]
    public void Mapping()
    {
        var value = "test";
        _mapper.Map<DataPackageValues.IdGroupName>(value)
            .Value.Should().Be(value);
        _mapper.Map<DataPackageValues.IdName>(value)
            .Value.Should().Be(value);
    }
}