using FluentAssertions;
using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public class DataPackageTests : IDisposable
{

    private readonly List<IDisposable> _disposables = new();
    private readonly TestingIdGenerator _idGenerator;
    private readonly IServiceProvider _provider;


    public DataPackageTests(ITestOutputHelper toh)
    {
        using var exceptions = new ExceptionBuilder(nameof(DataPackageTests));
        var logger = LoggerFactory.Create(builder => builder.AddXunit(toh));
        var provider = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger, JLibDataGenerationTestsTp.Instance)
            .AddAutoMapper(cfg => cfg.AddProfiles(typeCache, logger))
            .AddTestingIdGenerator()
            .AddDataPackages(typeCache)
            .BuildServiceProvider();
        _disposables.Add(provider);
        _idGenerator = provider.GetRequiredService<TestingIdGenerator>();
        _provider = provider;

    }

    public void Dispose()
    {
        _disposables.DisposeAll();
        GC.SuppressFinalize(this);
    }
    private abstract class TypeArgumentIdDp<TId> : DataPackage
        where TId : GuidValueType
    {
        protected TypeArgumentIdDp(IServiceProvider provider) : base(provider)
        {
        }

        public TId Id { get; init; } = null!;
    }

    private sealed class DemoIdDp : TypeArgumentIdDp<DemoId>
    {
        public DemoIdDp(IServiceProvider provider) : base(provider)
        {
        }
    }

    private record DemoId(Guid Value) : GuidValueType(Value);




    [Fact]
    public void Test()
    {
        _provider.IncludeDataPackages<DemoIdDp>();

        var dp = _provider.GetRequiredService<DemoIdDp>();
        dp.Id.Should().NotBeNull();
        dp.Id.Value.Should().NotBeEmpty();

    }
}