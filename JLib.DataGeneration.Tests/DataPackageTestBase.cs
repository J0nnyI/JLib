using JLib.AutoMapper;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using JLib.ValueTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace JLib.DataGeneration.Tests;

public abstract class DataPackageTestBase : IDisposable
{
    public record TestTypeId(Guid Value) : GuidValueType(Value);

    private readonly List<IDisposable> _disposables = new();

    public void Dispose()
        => _disposables.DisposeAll();

    protected DataPackageTestBase(ITestOutputHelper toh, params ITypePackage[] additionalTypes)
    {
        using var exceptions = new ExceptionBuilder(GetType().FullName());
        var logger = LoggerFactory.Create(x => x.AddXunit(toh));
        var typePackage = new TypePackageBuilder(logger)
            .AddAssemblyOf<DataPackage>()
            .Build()
            .MergeWith(null, additionalTypes);
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, exceptions, logger, typePackage)
            .AddAutoMapper(x => x.AddProfiles(typeCache, logger))
            .AddLogging(x => x.AddXunit(toh))
            .AddDataPackages(typeCache, new() { NamespaceAliases = new[] { new NamespaceAlias("JLib.DataGeneration.Tests") } });
        Provider = services.BuildServiceProvider().DisposeWith(_disposables);
    }

    protected ServiceProvider Provider { get; }
}