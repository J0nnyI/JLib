using JLib.Exceptions;
using JLib.Helper;
using JLib.TypeSystem.Abstractions;
using System.Reflection;
using static JLib.TypeSystem.Abstractions.TypeSystemValues;

namespace JLib.SourceCodeGenerator;

public class ExistingClass : IClass
{
    private readonly Type _type;

    public ExistingClass(Type type)
    {
        _type = type;
    }
    void INode.Validate(ExceptionBuilder errors) {}

    void INode.Write(ISourceCodeWriter writer, ExceptionBuilder exceptions) 
        => throw new InvalidOperationException($"The class {Name.Value} already exists");

    public IEnumerable<Namespace> GetRequiredNamespaces()
    {

    }

    public IEnumerable<Assembly> GetRequiredAssemblies()
    {

    }

    public MemberName Name => _type.Name;

    public AccessModifier AccessModifier => _type.getac

    public bool IsStatic
    {
        get => throw new NotImplementedException();
    }

    public Namespace? Namespace => throw new NotImplementedException();

    public bool IsPartial => throw new NotImplementedException();
}

public class GeneratedClass : IClass
{
    public GeneratedClass(MemberName name)
    {
        Name = name;
    }

    public MemberName Name { get; set; }
    public Namespace? Namespace { get; set; }

    private readonly Dictionary<MemberName, IMember> _member = new();

    public IEnumerable<IMember> Members
    {
        init => _member = value.ToDictionary(x => x.Name);
    }
    public void AddMember(IMember member)
        => _member.Add(member.Name, member);
    public IMember this[MemberName name]
        => _member[name];

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Private;

    public bool IsPartial { get; set; } = true;
    public bool IsStatic { get; set; }

    void INode.Write(ISourceCodeWriter writer, ExceptionBuilder exceptions)
    {

    }
    public void Validate(ExceptionBuilder errors) { }

    public IEnumerable<Namespace> GetRequiredNamespaces() => _member.SelectMany(x => x.Value.GetRequiredNamespaces());
    public IEnumerable<Assembly> GetRequiredAssemblies() => _member.SelectMany(x => x.Value.GetRequiredAssemblies());
}
