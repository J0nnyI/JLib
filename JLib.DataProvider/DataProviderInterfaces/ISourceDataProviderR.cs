namespace JLib.DataProvider;

/// <summary>
/// provides the data access for a custom <see cref="IDataProviderR{TDataObject}"/> implementation.
/// </summary>
/// <typeparam name="TDataObject">The Data Object Type to be accessed</typeparam>
/// <seealso cref="IDataProviderR{TDataObject}"/>
/// <seealso cref="IDataProviderRw{TDataObject}"/>
/// <seealso cref="ISourceDataProviderRw{TData}"/>
public interface ISourceDataProviderR<TDataObject> : IDataProviderR<TDataObject>
    where TDataObject : IDataObject
{
}