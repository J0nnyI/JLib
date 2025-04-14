using System.Reflection;

using JLib.Exceptions;

namespace JLib.Reflection;

public sealed class AssemblyLoadFailedBuilderException : JLibException
{
    public AssemblyName? AssemblyName;

    public AssemblyLoadFailedBuilderException(AssemblyName assemblyName, Exception innerException) : base(
        $"Assembly {assemblyName} could not be loaded: {innerException.Message}", innerException)
    {
        Data[nameof(AssemblyName)] = assemblyName;
        AssemblyName = assemblyName;
    }
}