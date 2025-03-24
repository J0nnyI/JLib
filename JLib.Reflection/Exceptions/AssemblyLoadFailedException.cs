using System.Reflection;

using JLib.Exceptions;

namespace JLib.Reflection;

public abstract class TypePackageBuilderException : JLibException
{
    protected TypePackageBuilderException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public sealed class AssemblyLoadFailedBuilderException : TypePackageBuilderException
    {
        public AssemblyName? AssemblyName;

        public AssemblyLoadFailedBuilderException(AssemblyName assemblyName, Exception innerException) : base(
            $"Assembly {assemblyName} could not be loaded: {innerException.Message}", innerException)
        {
            Data[nameof(AssemblyName)] = assemblyName;
            AssemblyName = assemblyName;
        }
    }
}
