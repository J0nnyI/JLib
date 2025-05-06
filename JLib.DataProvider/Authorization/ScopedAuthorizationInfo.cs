using System.Linq.Expressions;
using JLib.Exceptions;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;
using static JLib.DataProvider.DataProviderException.RuntimeException.DataObjectAccessFailedException;

namespace JLib.DataProvider.Authorization;

/// <summary>
/// can be injected by Data Providers to add Authorization
/// </summary>
public interface IAuthorizationInfo<TDataObject> : IAuthorizationInfo
    where TDataObject : class, IDataObject
{
    public Expression<Func<TDataObject, bool>> Expression();
    public bool DataObject(TDataObject dataObject);

    IReadOnlyCollection<TDataObject> AndRaiseException(IReadOnlyCollection<TDataObject> dataObjects)
    {
        dataObjects
            .Select(AndGetException)
            .WhereNotNull()
            .ThrowExceptionIfNotEmpty("some Data Objects are not Authorized");
        return dataObjects;
    }

    TDataObject AndRaiseException(TDataObject dataObject)
    {
        var ex = AndGetException(dataObject);
        if (ex is not null)
            throw ex;
        return dataObject;
    }

    Exception? AndGetException(TDataObject dataObject)
    {
        if (DataObject(dataObject))
            return null;
        return new DataProviderException.RuntimeException.DataObjectAccessFailedException<TDataObject>(
            null,dataObject.Id,FailureReason.AccessDenied);
    }
}

internal class AuthorizationInfo<TDataObject, TDependency1>(
    Func<TDependency1, Expression<Func<TDataObject, bool>>> authorizeQueryable,
    Func<TDependency1, TDataObject, bool> authorizeDataObject,
    DataObjectType target,
    IServiceScope scope)
    : IAuthorizationInfo<TDataObject>
    where TDataObject : class, IDataObject
    where TDependency1 : notnull
{
    public DataObjectType Target { get; } = target;

    public Expression<Func<TDataObject, bool>> Expression()
        => authorizeQueryable(scope.ServiceProvider.GetRequiredService<TDependency1>());

    /// <summary>
    /// returns true if the user is authorized to access the given entity
    /// </summary>
    public bool DataObject(TDataObject dataObject)
        => authorizeDataObject(scope.ServiceProvider.GetRequiredService<TDependency1>(), dataObject);
}