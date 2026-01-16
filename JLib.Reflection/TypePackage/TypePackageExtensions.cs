using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Text.Json;

using JLib.Helper;
using JLib.ValueTypes;

namespace JLib.Reflection;

/// <summary>
/// Extension methods for <see cref="ITypePackage"/>
/// </summary>
public static class TypePackageExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions =
        new(JsonSerializerDefaults.General)
        { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    /// <summary>
    /// returns a json representation of the given <paramref name="typePackage"/>
    /// </summary>
    public static string ToJson(this ITypePackage typePackage, JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(typePackage.ToJsonObject(), options ?? DefaultOptions);

    /// <summary>
    /// returns a json optimized object representation of the given <paramref name="typePackage"/>
    /// </summary>
    public static object ToJsonObject(this ITypePackage typePackage)
    {
        var res = new Dictionary<string, object?>()
        {
            ["Description"] = typePackage.Name
                .Replace("{Children}", typePackage.Children.Count().ToString())
                .Replace("{Types}", typePackage.Types.Count().ToString()),
            ["Types"] = typePackage.Types.GroupBy(t => t.Namespace)
                .ToImmutableSortedDictionary(
                    kv => kv.Key ?? "-",
                    kv => kv.Select(t => t.FullName()).Order().ToReadOnlyCollection()
                ),
            ["Children"] = typePackage.Children.Select(ToJsonObject).ToReadOnlyCollection()
        };
        res.RemoveWhere(x => x.Value is null);
        return res;
    }
    
    
    /// <returns>combination of the given <paramref name="packages"/> into a new <see cref="ITypePackage"/></returns>
    public static ITypePackage Merge( string? name = null,params ITypePackage[] packages)
        => new TypePackageBuilder.TypePackageCollection(name?? $"{packages.Length} type packages", packages);
    
    /// <returns>combination of the given <paramref name="children"/> with the given <paramref name="parent"/> into a new <see cref="ITypePackage"/></returns>
    public static ITypePackage MergeWith(this ITypePackage parent, params ITypePackage[] children)
    => parent.MergeWith(null, children);
    /// <returns>combination of the given <paramref name="children"/> with the given <paramref name="parent"/> into a new <see cref="ITypePackage"/></returns>
    public static ITypePackage MergeWith(this ITypePackage parent, string? name, params ITypePackage[] children)
        => Merge(name, children.Append(parent).ToArray());
}