namespace JLib.Helper;

/// <summary>
/// Adds Extension Methods for <see cref="HashSet{T}"/>s 
/// </summary>
public static class HashSetExtensions
{
    /// <summary>
    /// Adds all <see cref="items"/> to the given <see cref="set"/>
    /// </summary>
    public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
            set.Add(item);
    }
    /// <summary>
    /// Removes all <see cref="items"/> from the given <see cref="set"/>
    /// </summary>
    public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
            set.Remove(item);
    }
}