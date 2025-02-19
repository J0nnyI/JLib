using JLib.DataProvider.Authorization;
using Microsoft.EntityFrameworkCore;

namespace JLib.DataProvider.EfCore;

/// <summary>
/// a data provider which connects to the natively provided <see cref="DbContext"/> pulling it via dependency injection.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EfCoreDataProviderR<TEntity> : DataProviderRBase<TEntity>, ISourceDataProviderR<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;
    private readonly IAuthorizationInfo<TEntity> _authorize;

    public EfCoreDataProviderR(DbContext dbContext, IAuthorizationInfo<TEntity> authorize)
    {
        _dbContext = dbContext;
        _authorize = authorize;
    }

    public override IQueryable<TEntity> Get() => _dbContext.Set<TEntity>().Where(_authorize.Expression()).AsNoTracking();
}

/// <summary>
/// <inheritdoc cref="EfCoreDataProviderR{TEntity}"/>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EfCoreDataProviderRw<TEntity> : DataProviderRBase<TEntity>, ISourceDataProviderRw<TEntity>
    where TEntity : class, IEntity
{
    private readonly DbContext _dbContext;
    private readonly IAuthorizationInfo<TEntity> _authorize;

    public EfCoreDataProviderRw(DbContext dbContext, IAuthorizationInfo<TEntity> authorize)
    {
        _dbContext = dbContext;
        _authorize = authorize;
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderR{TEntity}.Get()"/>
    /// </summary>
    public override IQueryable<TEntity> Get()
        => _dbContext.Set<TEntity>().Where(_authorize.Expression());

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Add(TEntity)"/>
    /// </summary>
    public void Add(TEntity dataObject)
        => _dbContext.Set<TEntity>().Add(_authorize.AndRaiseException(dataObject));

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Add(IReadOnlyCollection{TEntity})"/>
    /// </summary>
    public void Add(IReadOnlyCollection<TEntity> dataObject)
        => _dbContext.Set<TEntity>().AddRange(_authorize.AndRaiseException(dataObject));

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(Guid)"/>
    /// </summary>
    public void Remove(Guid dataObjectId)
    {
        var set = _dbContext.Set<TEntity>();
        var item = set.Single(x => x.Id == dataObjectId);
        _authorize.AndRaiseException(item);
        _dbContext.Set<TEntity>().Remove(item);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(TEntity)"/>
    /// </summary>
    public void Remove(TEntity dataObject)
        => _dbContext.Remove(dataObject);

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(IReadOnlyCollection{Guid})"/>
    /// </summary>
    public void Remove(IReadOnlyCollection<Guid> dataObjectIds)
    {
        var set = _dbContext.Set<TEntity>();
        var items = set.Where(x => dataObjectIds.Contains(x.Id)).ToArray();
        _authorize.AndRaiseException(items);
        _dbContext.Set<TEntity>().RemoveRange(items);
    }

    /// <summary>
    /// <inheritdoc cref="IDataProviderRw{TEntity}.Remove(IReadOnlyCollection{TEntity})"/>
    /// </summary>
    public void Remove(IReadOnlyCollection<TEntity> dataObjects)
        => _dbContext.RemoveRange(dataObjects);
}