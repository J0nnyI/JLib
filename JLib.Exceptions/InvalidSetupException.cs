namespace JLib.Exceptions;

/// <summary>
/// May be implemented by Exceptions which are thrown during setup but require a base class other than <see cref="InvalidSetupException"/><br/>
/// <inheritdoc cref="InvalidSetupException"/>
/// </summary>
public interface IInvalidSetupException
{
    /// <summary>
    /// <inheritdoc cref="Exception.Message"/>
    /// </summary>
    public string Message { get; }
}

/// <summary>
/// Indicates, that the setup of the application is invalid. This could be caused a failed validation of types.<br/>
/// It is recommended to filter by <see cref="IInvalidSetupException"/>, as it also catches classes which are unable to derive from this type.
/// </summary>
public class InvalidSetupException : JLibException
{
    /// <summary>
    /// <inheritdoc cref="IInvalidSetupException"/>
    /// </summary>
    public InvalidSetupException(string message) : base(message)
    {
    }

    /// <summary>
    /// <inheritdoc cref="IInvalidSetupException"/>
    /// </summary>
    public InvalidSetupException(string message, Exception innerException) : base(message, innerException)
    {
    }
}