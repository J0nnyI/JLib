using JLib.Exceptions;
using JLib.Helper;
using System.Reflection;
using static JLib.SourceCodeGenerator.SourceGeneratorValues;

namespace JLib.SourceCodeGenerator;

public static class Reflection

public interface ISourceCodeGeneratorElement
{
    void Validate(ExceptionBuilder errors);
    void Write(SourceCodeWriter writer);
}

public interface IMember
{
    /// <summary>
    /// a non distinct list of all required namespaces.
    /// </summary>
    public IEnumerable<Namespace> GetRequiredNamespaces();
    public IEnumerable<Assembly> GetRequiredAssemblies();
    public MemberName Name { get; }
    public AccessModifier AccessModifier { get; }
    public bool IsStatic { get; set; }
}

public interface INamespaceMember : IMember
{
    public Namespace? Namespace { get; }
}
public interface IProperty : IMember
{

}
public interface IType : INamespaceMember
{

}
public interface IClass : IType
{
    public bool IsPartial { get; }
}

public interface ISourceFile
{
    public AbsoluteFileName
}