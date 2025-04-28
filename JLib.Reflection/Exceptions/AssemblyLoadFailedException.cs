using System.Reflection;

using JLib.Exceptions;

namespace JLib.Reflection;

/// <summary>
/// Thrown, when an assembly could not be loaded for any reason
/// </summary>
public sealed class AssemblyLoadFailedException : JLibException
{
    /// <summary>
    /// The <see cref="AssemblyName"/> of the assembly that could not be loaded
    /// </summary>
    public AssemblyName? AssemblyName;

    /// <summary>
    /// <inheritdoc cref="AssemblyLoadFailedException"/>
    /// </summary>
    public AssemblyLoadFailedException(AssemblyName assemblyName, Exception innerException) : base(
        $"Assembly {assemblyName} could not be loaded: {innerException.Message}", innerException)
    {
        Data[nameof(AssemblyName)] = assemblyName;
        AssemblyName = assemblyName;
    }
}