using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.TypeSystem.Abstractions;
using static JLib.TypeSystem.Abstractions.TypeSystemValues;

namespace JLib.SourceCodeGenerator;

public sealed class GeneratedProperty : IProperty
{
    public GeneratedProperty(MemberName name, IType type)
    {
        Name = name;
        Type = type;
    }
    public FieldOrPropertyAccessModifier FieldOrPropertyAccessModifier { get; set; }
    public IType Type { get; }
    public bool IsRequired { get; set; }
    public bool IsVirtual { get; set; }
    public MemberName Name { get; }
    public PropertyGetter? Get { get; set; }
    public PropertySetter? Set { get; set; }
    public bool IsStatic { get; set; }

    IEnumerable<Assembly> IMember.GetRequiredAssemblies()
        => Type.GetRequiredAssemblies()
            .Concat(Get?.GetRequiredAssemblies()
                    ?? Enumerable.Empty<Assembly>())
            .Concat(Set?.GetRequiredAssemblies()
                    ?? Enumerable.Empty<Assembly>());
    IEnumerable<Namespace> IMember.GetRequiredNamespaces()
        => (Type.Namespace is null
                ? Array.Empty<Namespace>()
                : new[] { Type.Namespace })
            .Concat(Get?.GetRequiredNamespaces()
                    ?? Enumerable.Empty<Namespace>())
            .Concat(Set?.GetRequiredNamespaces()
                    ?? Enumerable.Empty<Namespace>());

    void INode.Validate(ExceptionBuilder parentErrors)
    {
        using var errors = parentErrors.CreateChild($"Property {Name.Value}");

        if (Get is null && Set is null)
            errors.Add("Property must have at least a getter or a setter");
    }

    void INode.Write(ISourceCodeWriter writer, ExceptionBuilder exceptions)
    {
    }
}
/// <summary>
/// wither a <see cref="PropertyGetter"/> or <see cref="PropertySetter"/>
/// </summary>
public abstract class PropertyMethod<TCode>
    where TCode : PropertyCode
{
    private protected PropertyMethod()
    {

    }

    public TCode? Code { get; set; }
    public List<Namespace> RequiredNamespaces { get; } = new();
    public IReadOnlyCollection<Namespace> GetRequiredNamespaces() => RequiredNamespaces;

    public List<Assembly> RequiredAssemblies { get; } = new();
    public IReadOnlyCollection<Assembly> GetRequiredAssemblies() => RequiredAssemblies;

}

public sealed class PropertyGetter : PropertyMethod<PropertyGetterCode>
{
}
public sealed class PropertySetter : PropertyMethod<PropertySetterCode>
{
}