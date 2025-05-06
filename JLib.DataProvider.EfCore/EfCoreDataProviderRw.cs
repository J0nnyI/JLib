using JLib.DataProvider.Authorization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.DataProvider.EfCore;

/// <summary>
/// a data provider which connects to the natively provided <see cref="DbContext"/> pulling it via dependency injection.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/**
 * Developers Note:
 * The Authorization efficiency could be improved by evaluating the authorization without do materialization.
 */
public class EfCoreDataProviderR<TEntity>(DbContext dbContext, IAuthorizationInfo<TEntity> authorize)
    : DataProviderRBase<TEntity>, ISourceDataProviderR<TEntity>
    where TEntity : class, IEntity
{
    public override IQueryable<TEntity> Get() => dbContext.Set<TEntity>().Where(authorize.Expression()).AsNoTracking();
}

/// <summary>
/// <inheritdoc cref="EfCoreDataProviderR{TEntity}"/>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/**
* Developers Note:
* The Authorization efficiency could be improved by evaluating the authorization without do materialization.
 * This would require breaking changes in the AuthorizationInfo Interface.
*/
public class EfCoreDataProviderRw<TEntity> : DataProviderRBase<TEntity>, ISourceDataProviderRw<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;
    private readonly IAuthorizationInfo<TEntity>? _authorize;

    /// <summary>
    /// <inheritdoc cref="EfCoreDataProviderR{TEntity}"/>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /**
* Developers Note:
* The Authorization efficiency could be improved by evaluating the authorization without do materialization.
 * This would require breaking changes in the AuthorizationInfo Interface.
*/
    public EfCoreDataProviderRw(DbContext dbContext, IServiceProvider provider)
    {
        _dbContext = dbContext;
        _authorize = provider.GetService<IAuthorizationInfo<TEntity>>();
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderR{TEntity}.Get()"/>
    /// </summary>
    public override IQueryable<TEntity> Get()
        => _authorize is null
            ? _dbContext.Set<TEntity>()
            : _dbContext.Set<TEntity>().Where(_authorize.Expression());

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Add(TEntity)"/>
    /// </summary>
    public void Add(TEntity dataObject)
    {
        _authorize?.AndRaiseException(dataObject);
        _dbContext.Set<TEntity>().Add(dataObject);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Add(IReadOnlyCollection{TEntity})"/>
    /// </summary>
    public void Add(IReadOnlyCollection<TEntity> dataObject)
    {
        _authorize?.AndRaiseException(dataObject);
        _dbContext.Set<TEntity>().AddRange(dataObject);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(Guid)"/>
    /// </summary>
    public void Remove(Guid dataObjectId)
    {
        var set = _dbContext.Set<TEntity>();
        var item = set.Single(x => x.Id == dataObjectId);
        _authorize?.AndRaiseException(item);
        _dbContext.Set<TEntity>().Remove(item);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(TEntity)"/>
    /// </summary>
    public void Remove(TEntity dataObject)
    {
        var set = _dbContext.Set<TEntity>();
        var item = set.Single(x => x.Id == dataObject.Id);
        _authorize?.AndRaiseException(item);
        _dbContext.Remove(dataObject);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(IReadOnlyCollection{Guid})"/>
    /// </summary>
    public void Remove(IReadOnlyCollection<Guid> dataObjectIds)
    {
        var set = _dbContext.Set<TEntity>();
        var items = set.Where(x => dataObjectIds.Contains(x.Id)).ToArray();
        _authorize?.AndRaiseException(items);
        _dbContext.Set<TEntity>().RemoveRange(items);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(IReadOnlyCollection{TEntity})"/>
    /// </summary>
    public void Remove(IReadOnlyCollection<TEntity> dataObjects)
    {
        var set = _dbContext.Set<TEntity>();
        var ids = dataObjects.Select(o => o.Id);
        var items = set.Where(x => ids.Contains(x.Id)).ToArray();
        _authorize?.AndRaiseException(items);
        _dbContext.RemoveRange(dataObjects);
    }
}