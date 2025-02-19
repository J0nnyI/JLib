namespace JLib.DataProvider;

/// <summary>
/// Provides a common way to read <see cref="IDataObject"/>s of any kind using <see cref="Guid"/> Ids.<br/>
/// Basically an automated Repository
/// </summary>
/// <typeparam name="TDataObject">The Data Object Type to be accessed</typeparam>
/// <seealso cref="IDataProviderRw{TDataObject}"/>
/// <seealso cref="ISourceDataProviderR{TData}"/>
/// <seealso cref="ISourceDataProviderRw{TData}"/>
public interface IDataProviderR<TDataObject>
    where TDataObject : IDataObject
{
    /// <returns>a queryable with all <see cref="TDataObject"/>s of this DataProvider</returns>
    public IQueryable<TDataObject> Get();

    /// <param name="id">the <see cref="IDataObject.Id"/> of the requested <typeparamref name="TDataObject"/> </param>
    /// <returns>the  requested <typeparamref name="TDataObject"/> with <see cref="IDataObject.Id"/> <paramref name="id"/>.
    /// Throws an <see cref="DataProviderException.DataException.DataObjectNotFoundException{TDataObject}"/> if it could not be found.</returns>
    /// <exception cref="DataProviderException.DataException.DataObjectNotFoundException{TDataObject}"/>
    public TDataObject Get(Guid id);

    /// <param name="ids">the <see cref="IDataObject.Id"/> of the requested <typeparamref name="TDataObject"/>s.</param>
    /// <returns>the  requested <typeparamref name="TDataObject"/>s per <see cref="IDataObject.Id"/></returns>
    /// <exception cref="DataProviderException.DataException.DataObjectNotFoundException{TDataObject}"/>
    public IReadOnlyDictionary<Guid, TDataObject> Get(IReadOnlyCollection<Guid> ids);

    /// <param name="id">the <see cref="IDataObject.Id"/> of the requested <typeparamref name="TDataObject"/> </param>
    /// <returns>The  requested <typeparamref name="TDataObject"/> with <see cref="IDataObject.Id"/> <paramref name="id"/> or null, if it could not be found</returns>
    /// <exception cref="DataProviderException.DataException.DataObjectNotFoundException{TDataObject}"/>
    public TDataObject? TryGet(Guid? id);
    /// <param name="id">the <see cref="IDataObject.Id"/> to look for.</param>
    /// <returns>true, if an entity of type <typeparamref name="TDataObject"/> could be found.</returns>
    public bool Contains(Guid? id);
}