namespace JLib.DataProvider;

/// <summary>
/// Provides a common way to read or write <see cref="IDataObject"/>s of any kind using <see cref="Guid"/> Ids.<br/>
/// Basically an automated Repository
/// </summary>
/// <typeparam name="TDataObject">The Data Object Type to be accessed</typeparam>
/// <seealso cref="IDataProviderR{TDataObject}"/>
/// <seealso cref="ISourceDataProviderR{TData}"/>
/// <seealso cref="ISourceDataProviderRw{TData}"/>
public interface IDataProviderRw<TDataObject> : IDataProviderR<TDataObject>
    where TDataObject : IEntity
{
    /// <summary>
    /// Adds the <paramref name="dataObject"/> to the datasource.
    /// </summary>
    public void Add(TDataObject dataObject);
    /// <summary>
    /// Adds the <paramref name="dataObject"/> to the datasource.
    /// </summary>
    public void Add(IReadOnlyCollection<TDataObject> dataObject);
    /// <summary>
    /// removes the <typeparamref name="TDataObject"/> using the <paramref name="dataObjectId"/>.
    /// </summary>
    /// <seealso cref="Remove(TDataObject)"/>
    public void Remove(Guid dataObjectId);
    /// <summary>
    /// removes the <typeparamref name="TDataObject"/> with by reference.
    /// </summary>
    /// <seealso cref="Remove(Guid)"/>
    public void Remove(TDataObject dataObject);
    /// <summary>
    /// removes the <typeparamref name="TDataObject"/>s using <paramref name="dataObjectIds"/>.
    /// </summary>
    /// <seealso cref="Remove(IReadOnlyCollection{TDataObject})"/>
    public void Remove(IReadOnlyCollection<Guid> dataObjectIds);
    /// <summary>
    /// removes the <paramref name="dataObjects"/> by reference.
    /// </summary>
    /// <seealso cref="Remove(IReadOnlyCollection{Guid})"/>
    public void Remove(IReadOnlyCollection<TDataObject> dataObjects);
}