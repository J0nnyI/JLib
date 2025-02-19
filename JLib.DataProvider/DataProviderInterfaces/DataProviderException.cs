using JLib.Exceptions;
using JLib.Helper;

namespace JLib.DataProvider;

/// <summary>
/// Exceptions thrown by <see cref="IDataProviderR{TDataObject}"/>, <see cref="IDataProviderRw{TDataObject}"/>, <see cref="ISourceDataProviderR{TData}"/> amd <see cref="ISourceDataProviderRwObject{TData}"/> implementations.
/// </summary>
public abstract class DataProviderException : JLibException
{
    /// <summary>
    /// The TDataObject of the throwing DataProvider
    /// </summary>
    public Type DataObjectType { get; }
    /// <summary>
    /// The DataProvider Implementation which throws the exception
    /// </summary>
    public Type DataProviderType { get; }
    /// <summary>
    /// <inheritdoc cref="DataProviderException"/>
    /// </summary>
    /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
    /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    protected DataProviderException(Type dataProviderType, Type dataObjectType, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        DataObjectType = dataObjectType;
        DataProviderType = dataProviderType;
        Data[nameof(DataObjectType)] = dataObjectType;
        Data[nameof(DataProviderType)] = dataProviderType;
    }

    /// <summary>
    /// Thrown, when a data operation failed
    /// </summary>
    /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
    /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public abstract class DataException(
        Type dataProviderType,
        Type dataObjectType,
        string message,
        Exception? innerException = null)
        : DataProviderException(dataProviderType, dataObjectType, message, innerException)
    {
        /// <summary>
        /// Thrown, when a data object could not be found
        /// </summary>
        /// <seealso cref="DataObjectNotFoundException{TDataObject}"/>
        public abstract class DataObjectNotFoundException : DataProviderException
        {
            /// <summary>
            /// The <see cref="IDataObject.Id"/> of the data object
            /// </summary>
            public Guid Id { get; }
            /// <summary>
            /// <inheritdoc cref="DataObjectNotFoundException"/>
            /// </summary>
            /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
            /// <param name="dataObjectType">The TDataObject of the throwing DataProvider</param>
            /// <param name="id">The <see cref="IDataObject.Id"/> of the data object</param>
            protected DataObjectNotFoundException(Type dataProviderType, Type dataObjectType, Guid id) : base(dataProviderType, dataObjectType,
                $"{dataProviderType.FullName()} could not find {dataObjectType.FullName()} with id {id}.")
            {
                Id = id;
                Data[Id] = id;
            }
        }

        /// <summary>
        /// <inheritdoc cref="DataObjectNotFoundException"/>
        /// </summary>
        /// <typeparam name="TDataObject">The TDataObject of the throwing DataProvider</typeparam>
        /// <param name="dataProviderType">The DataProvider Implementation which throws the exception</param>
        /// <param name="id">The <see cref="IDataObject.Id"/> of the data object</param>
        /// <seealso cref="DataObjectNotFoundException"/>
        public sealed class DataObjectNotFoundException<TDataObject>(Type dataProviderType, Guid id)
            : DataObjectNotFoundException(dataProviderType, typeof(TDataObject), id);
    }

    public abstract class InvalidSetupException(
        Type dataProviderType,
        Type dataObjectType,
        string message,
        Exception? innerException = null)
        : DataProviderException(dataProviderType, dataObjectType, message, innerException), IInvalidSetupException
    {
        
    }
}