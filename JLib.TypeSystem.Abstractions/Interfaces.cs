using JLib.Exceptions;
using JLib.Helper;
using JLib.ValueTypes;
using JLib.ValueTypes.Implementations.FileSystem;
using System.Reflection;
using static JLib.TypeSystem.Abstractions.TypeSystemValues;

namespace JLib.TypeSystem.Abstractions;

/// <summary>
/// Everything which is part of the syntax tree and can therefore be serialized to SourceCode
/// </summary>
public interface INode
{
    /// <summary>
    /// checks, whether this node is valid
    /// </summary>
    /// <param name="errors"></param>
    void Validate(ExceptionBuilder errors);
    /// <summary>
    /// writes this node to the given <paramref name="writer"/>
    /// </summary>
    void Write(ISourceCodeWriter writer, ExceptionBuilder exceptions);
}
/// <summary>
/// Members which may be used in classes
/// </summary>
public interface IClassMember : IMember { }
public interface IMember : INode
{
    /// <summary>
    /// a non-distinct list of all required namespaces.
    /// </summary>
    public IEnumerable<Namespace> GetRequiredNamespaces();
    /// <summary>
    /// a non-distinct list of all required assemblies.
    /// </summary>
    public IEnumerable<Assembly> GetRequiredAssemblies();
    public MemberName Name { get; }
    public FieldOrPropertyAccessModifier FieldOrPropertyAccessModifier { get; }
    public bool IsStatic { get; }
}

/// <summary>
/// everything which is compatible with the <see langword="using"/> directive.
/// </summary>
public interface IUsable : INode { }

public interface INamespaceMember : IMember
{
    public Namespace? Namespace { get; }
}

public interface IType : INamespaceMember, IClassMember
{
    public bool IsPartial { get; }

}

public interface GenericTypeArgument
{

}
public interface IClass : IType, IUsable
{
    public IClass? BaseClass { get; }
    public IReadOnlyCollection<GenericTypeArgument> GenericTypeArguments { get; }
    public IReadOnlyCollection<IType> GenericTypeParameters { get; }
}

public interface ISourceFile
{
    public AbsoluteFilePath Path { get; }
    public IReadOnlyCollection<INamespaceMember> Content { get; }
}