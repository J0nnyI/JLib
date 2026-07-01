using System.Reflection;
using JLib.Exceptions;
using JLib.Helper;
using JLib.Reflection;
using JLib.ValueTypes;
using static JLib.Reflection.TvtFactoryAttribute;

namespace JLib.Configuration;

/// <summary>
/// The Key of a config section.
/// </summary>
/// <param name="Value"></param>
public record ConfigSectionName(string Value) : StringValueType(Value)
{
    /// <summary>
    /// <inheritdoc cref="ConfigSectionName"/>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator ConfigSectionName(string value) => new(value);
}
/// <summary>
/// The key, under which this config section can be found in the config.
/// Can be added to the service collection via <see cref="ConfigurationServiceCollectionExtensions.AddAllConfigSections"/> or be directly accessed via <see cref="ConfigurationHelper.GetSectionObject{T}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigSectionNameAttribute : Attribute
{
    /// <summary>
    /// <inheritdoc cref="ConfigSectionNameAttribute"/>
    /// </summary>
    /// <param name="sectionName"></param>
    public ConfigSectionNameAttribute(string sectionName)
    {
        SectionName = sectionName;
    }

    /// <summary>
    /// The Key under which this config section can be found in the Configuration
    /// </summary>
    public ConfigSectionName SectionName { get; }

    /// <summary>
    /// Resolves the <see cref="ConfigSectionName"/> declared by the <see cref="ConfigSectionNameAttribute"/> on <paramref name="type"/>.
    /// </summary>
    /// <exception cref="InvalidSetupException">when <paramref name="type"/> is missing the <see cref="ConfigSectionNameAttribute"/></exception>
    public static ConfigSectionName ResolveSectionName(Type type)
        => type.GetCustomAttribute<ConfigSectionNameAttribute>()?.SectionName
           ?? throw new InvalidSetupException(
               $"missing {nameof(ConfigSectionNameAttribute)} on class {type.FullName()}");
}

/// <summary>
/// Defines a Type which contains a config section.
/// </summary>
/// <remarks>
/// Must be a non-abstract class with a <see cref="ConfigSectionNameAttribute"/>
/// </remarks>
/// <param name="Value"></param>
[HasAttribute(typeof(ConfigSectionNameAttribute)), IsClass, NotAbstract]
public sealed record ConfigurationSectionType(Type Value) : TypeValueType(Value), IPostNavigationInitializedType, IValidatedType
{
    /// <summary>
    /// The key under which this config section can be found
    /// </summary>
    public ConfigSectionName SectionName { get; private set; } = null!;

    /// <summary>
    /// used to initialize this valueType, should not be called manually
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="exceptions"></param>
    public void Initialize(ITypeCache cache, ExceptionBuilder exceptions)
        => SectionName = Value.GetCustomAttribute<ConfigSectionNameAttribute>()?.SectionName
                         ?? throw NewInvalidTypeException("sectionName not found");

    /// <summary>
    /// flags open generic config sections as invalid: they cannot be bound to a config section
    /// (configuration binding requires a closed type), so a <see cref="ConfigSectionNameAttribute"/>
    /// on a generic class is always a mistake and is surfaced as a type cache validation error.
    /// </summary>
    void IValidatedType.Validate(ITypeCache cache, IValidationContext<Type> value)
        => value.ShouldNotBeGeneric($"a config section bound via {nameof(ConfigSectionNameAttribute)} must be a closed type");
}