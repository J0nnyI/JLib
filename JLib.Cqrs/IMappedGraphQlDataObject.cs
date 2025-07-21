using JLib.DataProvider;
using JLib.DataProvider.Authorization;
using JLib.DataProvider.AutoMapper;
using JLib.DataProvider.EfCore;
using JLib.DependencyInjection;
using JLib.Exceptions;
using JLib.HotChocolate;
using JLib.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JLib.Cqrs;

public interface IGraphQlDataObject : IQueryDataObject
{
}

[IgnoreInCache()]
public interface IMappedGraphQlDataObject<TMapFrom> : IGraphQlDataObject
    where TMapFrom : IDataObject
{
}

public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// adds all services required for the cqrs framework
    /// </summary>
    /// <param name="services"></param>
    /// <param name="typeCache"></param>
    /// <param name="exceptions"></param>
    /// <param name="logger"></param>
    /// <param name="frameworkOptionsAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCqrsFramework(
        this IServiceCollection services,
        ITypeCache typeCache,
        ExceptionBuilder exceptions,
        ILoggerFactory logger,
        Action<CqrsFrameworkSetupBuilder> frameworkOptionsAction)
    {
        services
            .AddDataProvider<ReadDataObjectType, CastDataProviderR<IgnoredDataObject, IgnoredDataObject>,
                IgnoredDataObject>(
                typeCache, null, _ => true,
                [roe => roe.ReadWriteEntity, roe => roe],
                exceptions, logger)
            .AddMapDataProvider(typeCache, exceptions)
            .AddDataAuthorization(typeCache);
        var builderInstance =
            new CqrsFrameworkSetupBuilder(services, exceptions, logger, typeCache);

        frameworkOptionsAction(builderInstance);
        if (builderInstance.Initialized is false)
            exceptions.Add(new MissingFrameworkDataSourceException());

        return services;
    }
}

public class CqrsFrameworkSetupBuilder(
    IServiceCollection services,
    ExceptionBuilder exceptions,
    ILoggerFactory logger,
    ITypeCache typeCache)
{
    public bool Initialized { get; private set; }

    public void UseEfCore(
        Action<DbContextOptionsBuilder>? dbContextOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        => UseEfCore<AutoDbContext>(dbContextOptionsAction, contextLifetime, optionsLifetime);

    public void UseEfCore<TDbContext>(
        Action<DbContextOptionsBuilder>? dbContextOptionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        if (Initialized)
        {
            exceptions.Add(new DuplicateDataSourceInitializationException());
            return;
        }

        services
            .AddDataProvider<ReadWriteEntityType, EfCoreDataProviderRw<IgnoredEntity>, IgnoredEntity>(
                typeCache, null, null, null, exceptions, logger)
            .AddDbContext<TDbContext>(dbContextOptionsAction, contextLifetime, optionsLifetime);

        if (typeof(TDbContext) != typeof(DbContext))
            services.AddScopedAlias<DbContext, TDbContext>();

        Initialized = true;
    }
}

public class MissingFrameworkDataSourceException()
    : Exception("The CQRS Framework builder did not add any data sources.");

public class DuplicateDataSourceInitializationException()
    : Exception("The CQRS Framework Builder tried to add multiple data sources.");