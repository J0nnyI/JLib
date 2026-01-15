using JLib.Exceptions;
using JLib.Helper;

namespace JLib.DataGeneration;

/// <summary>
/// Defines the behavior of the <see cref="IIdRegistry"/>
/// </summary>
public class IdRegistryConfiguration
{
    private readonly IReadOnlyCollection<NamespaceAlias> _namespaceAliases = Array.Empty<NamespaceAlias>();

    /// <summary>
    /// Defines an alias for frequently used namespaces to improve readability.
    /// <remarks>
    ///<br/>
    /// The <see cref="NamespaceAliases"/> are applied sequentially to all <see cref="DataPackageValues.IdGroupName"/> and <see cref="DataPackageValues.IdName"/>.
    /// <br/>
    /// The <see cref="NamespaceAliases"/> are applied in sequence by replacing the <see cref="NamespaceAlias.Namespace"/> with "~<see cref="NamespaceAlias.Alias"/>~" if it has content or "~" if <see cref="NamespaceAlias.Alias"/> is null or whitespace. Whitespaces are trimmed.
    /// <br/>
    /// This means, that a later <see cref="NamespaceAlias"/> can override a previous one.
    /// </remarks>
    /// </summary>
    public IReadOnlyCollection<NamespaceAlias> NamespaceAliases
    {
        get => _namespaceAliases;
        init
        {
            NamespaceAliases
                .ToLookup(x => x.Alias)
                .Where(x => x.Count() > 1)
                .Select(invalidLookup =>
                    new Exception($"alias {invalidLookup.Key}, namespaces: [{string.Join(", ", invalidLookup)}]"))
                .ThrowExceptionIfNotEmpty("Some Namespace Aliases have been defined for multiple namespaces");

            _namespaceAliases = value;
        }
    }

    /// <summary>
    /// <inheritdoc cref="NamespaceAliases"/>
    /// </summary>
    internal string ApplyDefaultNamespace(string inputName) =>
        NamespaceAliases.Aggregate(inputName,
            (name, alias) => name.Replace(alias.Namespace,
                alias.Alias.IsNullOrWhitespace() ? "~" : $"~{alias.Alias.Trim()}~"));
}

/// <summary>
/// used by the <see cref="IdRegistryConfiguration"/> to define Namespace aliases for improved readability.
/// </summary>
/// <param name="Namespace"></param>
/// <param name="Alias"></param>
public record NamespaceAlias(string Namespace, string? Alias = null);
