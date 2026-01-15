namespace JLib.DataProvider;

/// <summary>
/// provides the data access for a custom <see cref="IDataProviderRw{TDataObject}"/> implementation.
/// </summary>
/// <typeparam name="TDataObject">The Data Object Type to be accessed</typeparam>
/// <seealso cref="IDataProviderR{TDataObject}"/>
/// <seealso cref="IDataProviderRw{TDataObject}"/>
/// <seealso cref="ISourceDataProviderR{TDataObject}"/>
public interface ISourceDataProviderRw<TDataObject> : IDataProviderRw<TDataObject>, ISourceDataProviderR<TDataObject>
    where TDataObject : IEntity
{
}