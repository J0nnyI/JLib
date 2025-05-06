using JLib.Exceptions;
using JLib.Helper;

namespace JLib.DataProvider;

/// <summary>
/// Exceptions thrown by <see cref="IDataProviderR{TDataObject}"/>, <see cref="IDataProviderRw{TDataObject}"/>, <see cref="ISourceDataProviderR{TData}"/> amd <see cref="ISourceDataProviderRw{TData}"/> implementations.
/// </summary>
public abstract class DataProviderException : JLibException
{
    /// <summary>
    /// The DataProvider Implementation which throws the exception
    /// </summary>
    public Type? DataProviderType { get; }
    /// <summary>
    /// <inheritdoc cref="DataProviderException"/>
    /// </summary>
    /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected DataProviderException(Type? dataProviderType, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DataProviderType = dataProviderType;
        Data[nameof(DataProviderType)] = dataProviderType;
    }

    /// <summary>
    /// Thrown, when the exception will be thrown during runtime
    /// </summary>
    public abstract class RuntimeException : DataProviderException
    {
        /// <summary>
        /// The TDataObject of the throwing DataProvider
        /// </summary>
        public Type DataObjectType { get; }
        /// <summary>
        /// Thrown, when a data operation failed
        /// </summary>
        /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
        /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        protected RuntimeException(Type? dataProviderType,
            Type dataObjectType,
            string message,
            Exception? innerException = null) : base(dataProviderType, message, innerException)
        {
            DataObjectType = dataObjectType;
            Data[nameof(DataObjectType)] = dataObjectType;
        }





        /// <summary>
        /// Thrown, when a data object could not be found
        /// </summary>
        /// <seealso cref="DataProviderException.RuntimeException.DataObjectAccessFailedException{TDataObject}"/>
        public abstract class DataObjectAccessFailedException : RuntimeException
        {
            /// <summary>
            /// the reason, why the access to the entity failed
            /// </summary>
            public enum FailureReason
            {
                /// <summary>
                /// the entity does not exist in the database
                /// </summary>
                NotFound,
                /// <summary>
                /// the entity is not accessible by the current user
                /// </summary>
                AccessDenied,
                /// <summary>
                /// it is not known why the entity could not be accessed, it could be either <see cref="NotFound"/> od <see cref="AccessDenied"/>.
                /// </summary>
                Unknown
            }
            /// <summary>
            /// The <see cref="IDataObject.Id"/> of the data object
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// The reason 
            /// </summary>
            public FailureReason Reason { get; }

            /// <summary>
            /// <inheritdoc cref="DataProviderException.RuntimeException.DataObjectAccessFailedException"/>
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
            /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
            /// <param name="id">The <see cref="IDataObject.Id"/> of the data object</param>
            /// <param name="reason">the reason why the access failed</param>
            protected DataObjectAccessFailedException(Type? dataProviderType, Type dataObjectType, Guid id, FailureReason reason)
                : base(dataProviderType, dataObjectType,
                $"{dataProviderType?.FullName()} could not access {dataObjectType.FullName()} with id {id}")
            {
                Id = id;
                Data[nameof(Id)] = id;
                Reason = reason;
                Data[nameof(Reason)] = reason;
            }
        }
        /// <summary>
        /// <inheritdoc cref="DataProviderException.RuntimeException.DataObjectAccessFailedException"/>
        /// </summary>
        /// <typeparam name="TDataObject">The TDataObject of the throwing DataProvider</typeparam>
        /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
        /// <param name="id">The <see cref="IDataObject.Id"/> of the data object</param>
        /// <param name="reason">the reason why the access failed</param>
        /// <seealso cref="DataProviderException.RuntimeException.DataObjectAccessFailedException"/>
        public sealed class DataObjectAccessFailedException<TDataObject>(Type? dataProviderType, Guid id, DataObjectAccessFailedException.FailureReason reason)
            : DataObjectAccessFailedException(dataProviderType, typeof(TDataObject), id, reason);


        /// <summary>
        /// Thrown, when a data object could be found but the <see cref="Authorization.AuthorizationProfile"/> denied access to it
        /// </summary>
        /// <seealso cref="DataProviderException.RuntimeException.DataObjectNotFoundException{TDataObject}"/>
        public abstract class DataObjectAccessRejectedException : RuntimeException
        {
            /// <summary>
            /// The <see cref="IDataObject.Id"/> of the data object
            /// </summary>
            public Guid Id { get; }
            /// <summary>
            /// <inheritdoc cref="DataProviderException.RuntimeException.DataObjectNotFoundException"/>
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
            /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
            /// <param name="id">The <see cref="IDataObject.Id"/> of the data object</param>
            protected DataObjectAccessRejectedException(Type dataProviderType, Type dataObjectType, Guid id) : base(dataProviderType, dataObjectType,
                $"{dataProviderType.FullName()} could not find {dataObjectType.FullName()} with id {id}.")
            {
                Id = id;
                Data[Id] = id;
            }
        }

    }

    /// <summary>
    /// thrown, when an exception will be thrown during application initialization (like configure services)
    /// </summary>
    /// <param name="dataProviderType">The DataProvider Implementation which failed to be set up</param>
    /// <param name="message"><inheritdoc cref="Exception.Message"/></param>
    /// <param name="innerException"><inheritdoc cref="Exception.InnerException"/></param>
    public abstract class InvalidSetupException(
        Type dataProviderType,
        string message,
        Exception? innerException = null)
        : DataProviderException(dataProviderType, message, innerException)
            , IInvalidSetupException
    {
        /// <summary>
        /// Thrown, when the DataProvider and Repository implementations are incompatible with one another
        /// </summary>
        public abstract class RepositoryDataAccessMismatchException : InvalidSetupException
        {
            /// <summary>
            /// The Repository Type which is being configured
            /// </summary>
            public RepositoryType RepositoryType { get; }

            /// <summary>
            /// <inheritdoc cref="DataProviderException.InvalidSetupException.RepositoryDataAccessMismatchException"/>
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which failed to be set up</param>
            /// <param name="repositoryType">The RepositoryType which failed to be set up</param>
            /// <param name="message"><inheritdoc cref="Exception.Message"/></param>
            /// <param name="innerException"><inheritdoc cref="Exception.InnerException"/></param>
            protected RepositoryDataAccessMismatchException(Type dataProviderType,
                RepositoryType repositoryType,
                string message,
                Exception? innerException = null) : base(dataProviderType, message, innerException)
            {
                RepositoryType = repositoryType;
                Data[nameof(RepositoryType)] = repositoryType;
            }
            /// <summary>
            /// The data provider Implementation <paramref name="dataProviderType"/> is forced read only but the Repository <paramref name="repositoryType"/> can write data.<br/>
            /// Not forcing the DataProvider to be read only or implementing <see cref="IDataProviderRw{TDataObject}"/> will solve this issue
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which failed to be set up</param>
            /// <param name="repositoryType">The RepositoryType which failed to be set up</param>
            public sealed class ImplementationForcedReadOnlyButWritableRepositoryException(
                Type dataProviderType,
                RepositoryType repositoryType)
                : RepositoryDataAccessMismatchException
                    (dataProviderType, repositoryType,
                        $"The data provider Implementation {dataProviderType.FullName(true)} is forced read only but the Repository {repositoryType.Value.FullName(true)} can write data. {Environment.NewLine}" +
                        $"Not forcing the DataProvider to be read only or implementing {nameof(IDataProviderRw<IEntity>)} will solve this issue");

            /// <summary>
            /// The data provider Implementation <paramref name="dataProviderType"/> is read only but the Repository <paramref name="repositoryType"/> can write data.<br/>
            /// You can resolve this issue by not implementing <see cref="IDataProviderRw{TDataObject}"/> with the Repository or using a data provider which implements <see cref="ISourceDataProviderRw{TDataObject}"/>
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which failed to be set up</param>
            /// <param name="repositoryType">The RepositoryType which failed to be set up</param>
            public sealed class ImplementationReadOnlyButWritableRepositoryException(
                Type dataProviderType,
                RepositoryType repositoryType)
                : RepositoryDataAccessMismatchException(dataProviderType, repositoryType,
                    $"The data provider Implementation {dataProviderType.FullName(true)} is read only but the Repository {repositoryType.Value.FullName(true)} can write data. {Environment.NewLine}" +
                    $"You can resolve this issue by not implementing {nameof(IDataProviderRw<IEntity>)} with the Repository or using a data provider which implements {nameof(ISourceDataProviderRw<IEntity>)}");

            /// <summary>
            /// The data provider Implementation <paramref name="dataProviderType"/> can write data but the Repository <paramref name="repositoryType"/> can not.<br/>
            /// Force the dataProvider to be ReadOnly or Implement <see cref="IDataProviderRw{TDataObject}"/> with the repository.
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which failed to be set up</param>
            /// <param name="repositoryType">The RepositoryType which failed to be set up</param>
            public sealed class ImplementationWritableButReadOnlyRepository(
                Type dataProviderType,
                RepositoryType repositoryType)
                : RepositoryDataAccessMismatchException(dataProviderType, repositoryType,
                    $"The data provider Implementation {dataProviderType.FullName(true)} can write data but the Repository {repositoryType.Value.FullName(true)} can not. {Environment.NewLine}" +
                    $"Force the dataProvider to be ReadOnly or Implement {nameof(IDataProviderRw<IEntity>)} with the repository.");
        }
    }
}