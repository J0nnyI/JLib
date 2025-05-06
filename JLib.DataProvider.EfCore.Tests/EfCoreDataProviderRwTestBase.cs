using FluentAssertions;
using JLib.Cqrs;
using JLib.DataProvider.Authorization;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.Reflection.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.DataProvider.EfCore.Tests;

public abstract class EfCoreDataProviderRwTestBase : IDisposable
{
    [IsClass, Implements<ICommandEntity>, NotAbstract, Priority(NextPriority)]
    public record MockEntityType(Type Value) : CommandEntityType(Value), IEfCoreEntityType
    {
        public new const int NextPriority = CommandEntityType.NextPriority - 1_000;
    }

    public class MockEntity : ICommandEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        public bool IsAuthorized { get; set; } = true;
    }

    public class MockAuthProfile : AuthorizationProfile
    {
        public MockAuthProfile(ITypeCache typeCache) : base(typeCache)
        {
            AddAuthorization<MockEntity, IServiceProvider>((s, e) => e.IsAuthorized);
        }
    }

    public static class MockData
    {
        public static Guid Authorized1 { get; } = Guid.NewGuid();
        public static Guid Authorized2 { get; } = Guid.NewGuid();
        public static Guid Unauthorized1 { get; } = Guid.NewGuid();
        public static Guid Unauthorized2 { get; } = Guid.NewGuid();
    }

    public virtual List<MockEntity> CreateData()
    {
        var data = new List<MockEntity>
        {
            new() { Id = MockData.Authorized1, Name = nameof(MockData.Authorized1), IsAuthorized = true },
            new() { Id = MockData.Authorized2, Name = nameof(MockData.Authorized2), IsAuthorized = true },
            new() { Id = MockData.Unauthorized1, Name = nameof(MockData.Unauthorized1), IsAuthorized = false },
            new() { Id = MockData.Unauthorized2, Name = nameof(MockData.Unauthorized2), IsAuthorized = false },
        };
        return data;
    }

    protected EfCoreDataProviderRw<MockEntity> DataProvider { get; }
    private readonly List<IDisposable> _disposables = new();
    protected AutoDbContext DbContext { get; }
    public ITypeCache TypeCache { get; }


    protected EfCoreDataProviderRwTestBase(ITestOutputHelper toh)
    {
        var packages = new TypePackageBuilder()
            .AddFromPath(null, ["JLib."])
            .Build();


        Logger = LoggerFactory.Create(c => c.AddXunit(toh));

        Exceptions = new ExceptionBuilder("startup");
        var services = new ServiceCollection()
            .AddTypeCache(out var typeCache, Exceptions, Logger, packages);
        TypeCache = typeCache;
        ModifyServices(services);


        var provider = services.BuildServiceProvider()
            .DisposeWith(_disposables)
            .CreateScope()
            .DisposeWith(_disposables)
            .ServiceProvider;

        Exceptions.ThrowIfNotEmpty();
        DbContext = provider.GetRequiredService<AutoDbContext>();
        DbContext.Database.EnsureCreated();
        DataProvider = provider.GetRequiredService<EfCoreDataProviderRw<MockEntity>>();
        DbContext.AddRange(CreateData());
        DbContext.SaveChanges();
    }

    public ILoggerFactory Logger { get; }

    protected ExceptionBuilder Exceptions { get; }

    protected virtual IServiceCollection ModifyServices(IServiceCollection services)
    {
        var root = new InMemoryDatabaseRoot();
        return services
            .AddDbContext<AutoDbContext>(c => c.UseInMemoryDatabase(GetType().Name, root))
            .AddScopedAlias<DbContext, AutoDbContext>()
            .AddDataProvider<CommandEntityType, EfCoreDataProviderRw<ICommandEntity>, ICommandEntity>(TypeCache, null,
                null, null, Exceptions, Logger);
    }

    [Fact]
    public void SetupWorks_TypeCache()
        => TypeCache.All<MockEntityType>().Should().HaveCount(1)
            .And.Contain(x => x.Value == typeof(MockEntity));

    [Fact]
    public void SetupWorks_Data()
        => DbContext.Set<MockEntity>().Should().NotBeEmpty();

    void IDisposable.Dispose()
    {
        DbContext.Database.EnsureDeleted();
        _disposables.DisposeAll();
    }
}