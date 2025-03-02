using System.Reflection;

using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.DataGeneration;

/// <summary>
/// Provides a set of value types for data package properties.
/// </summary>
public static class DataPackageValues
{
    /// <summary>
    /// Runtime id method calls are counted per IdScopeName.
    /// </summary>
    /// <param name="Value">the name of the scope</param>
    public record IdScopeName(string Value) : StringValueType(Value);

    /// <summary>
    /// Represents the name of an id.
    /// </summary>
    public record IdName(string Value) : StringValueType(Value)
    {
        private static string ExtractIdName(PropertyInfo property)
            => $"{(property.DeclaringType != property.ReflectedType
                && property.DeclaringType is not null
                   ? $"{property.DeclaringType.FullName(true)}."
                   : ""
               )}{property.Name}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IdName"/> class with the specified property.
        /// </summary>
        /// <param name="property">The property to get the name from.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdName(PropertyInfo property, IdRegistryConfiguration idRegConfig) : this(ExtractIdName(property), idRegConfig)
        { }

        /// <summary>
        /// Converts the given <see cref="value"/> into a <see cref="IdName"/> while applying the provided <paramref name="idRegConfig"/>
        /// </summary>
        /// <param name="value">the Value of this <see cref="IdName"/></param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdName(string value, IdRegistryConfiguration idRegConfig) : this(idRegConfig.ApplyDefaultNamespace(value))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdName"/> class with the specified method and call number.
        /// </summary>
        /// <param name="scopeName"><inheritdoc cref="IdScopeName"/></param>
        /// <param name="method">The method to get the full name from.</param>
        /// <param name="callNumber">The call number.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> which should be applied to this <see cref="IdName"/></param>
        public IdName(IdScopeName? scopeName, MethodBase method, int callNumber, IdRegistryConfiguration idRegConfig)
            : this((scopeName is null
                       ? ""
                       : $"[{scopeName.Value}]"
                )
                   + $"{(method.DeclaringType != method.ReflectedType
                         && method.DeclaringType is not null
                       ? method.DeclaringType.FullName(true) + "."
                       : "")}{method.FullName(false, false, false, true)}-{callNumber}"
                , idRegConfig)
        { }
    }

    /// <summary>
    /// Represents the name of a data package.
    /// </summary>
    public record IdGroupName : StringValueType
    {
        /// <summary>
        /// Extracts the <see cref="IdGroupName"/> from the given <paramref name="type"/> and applies the <see cref="IdRegistryConfiguration.NamespaceAliases"/> of the given <paramref name="idRegConfig"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be the base of this <see cref="IdGroupName"/>.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdGroupName(Type type, IdRegistryConfiguration idRegConfig)
            : this(type.FullName(true), idRegConfig)
        {
        }

        /// <summary>
        /// Extracts the <see cref="IdGroupName"/> from the given <paramref name="dataPackage"/> and applies the <see cref="IdRegistryConfiguration.NamespaceAliases"/> of the given <paramref name="idRegConfig"/>
        /// </summary>
        /// <param name="dataPackage">The <see cref="DataPackage"/> to be the base of this <see cref="IdGroupName"/>.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdGroupName(DataPackage dataPackage, IdRegistryConfiguration idRegConfig)
            : this(dataPackage.GetType(), idRegConfig)
        {
        }

        private static string ExtractKey(PropertyInfo property)
            => property.ReflectedType?.FullName(true)
               ?? "No declaring type found";

        /// <summary>
        /// Converts the given <paramref name="value"/> into a <see cref="IdGroupName"/> while applying the provided <see cref="idRegConfig"/>
        /// </summary>
        /// <param name="value">The String which will become the value of this <see cref="IdGroupName"/></param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdGroupName(string value, IdRegistryConfiguration idRegConfig) : this(idRegConfig.ApplyDefaultNamespace(value))
        { }
        /// <summary>
        /// Extracts the <see cref="IdGroupName"/> from the given <paramref name="property"/> and applies the <see cref="IdRegistryConfiguration.NamespaceAliases"/> of the given <paramref name="idRegConfig"/>
        /// </summary>
        /// <param name="property">The <see cref="DataPackage"/>'s <see cref="PropertyInfo"/> to get the full name from.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdGroupName(PropertyInfo property, IdRegistryConfiguration idRegConfig) : this(ExtractKey(property), idRegConfig)
        {
        }

        /// <summary>
        /// Converts the given string into a <see cref="IdGroupName"/>. In most cases, <see cref="IdGroupName"/>
        /// </summary>
        internal IdGroupName(string Value) : base(Value)
        {
        }
    }

    /// <summary>
    /// Represents an identifier for a data package property.
    /// </summary>
    public record IdIdentifier(IdGroupName IdGroupName, IdName IdName)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdIdentifier"/> class with the specified property.
        /// </summary>
        /// <param name="property">The property to create the identifier from.</param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdIdentifier(PropertyInfo property, IdRegistryConfiguration idRegConfig)
            : this(new IdGroupName(property, idRegConfig), new(property, idRegConfig))
        {
        }

        /// <summary>
        /// Converts the given <paramref name="idGroup"/> and <paramref name="idName"/> into a <see cref="IdIdentifier"/> while applying the provided <paramref name="idRegConfig"/>
        /// </summary>
        /// <param name="idGroup">The <see cref="DataPackageValues.IdGroupName"/> of this <see cref="IdIdentifier"/></param>
        /// <param name="idName">The <see cref="DataPackageValues.IdName"/> of this <see cref="IdIdentifier"/></param>
        /// <param name="idRegConfig">The <see cref="IdRegistryConfiguration"/> to apply the default namespace from.</param>
        public IdIdentifier(string idGroup, string idName, IdRegistryConfiguration idRegConfig)
            : this(new IdGroupName(idGroup, idRegConfig), new(idName, idRegConfig))
        { }
        internal IdIdentifier(string idGroup, string idName) : this(new IdGroupName(idGroup), new(idName))
        {
        }

        /// <summary>
        /// Returns a string that represents the current identifier.
        /// </summary>
        /// <returns>A string that represents the current identifier.</returns>
        public override string ToString() => $"[{IdGroupName.Value}].[{IdName.Value}]";
    }
}
