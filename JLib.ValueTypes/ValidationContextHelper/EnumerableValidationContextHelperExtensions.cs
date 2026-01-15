namespace JLib.ValueTypes;

/// <summary>
/// Adds Validation Methods to the <see cref="IValidationContext{TValue}"/> of <see cref="IReadOnlyCollection{T}"/>s
/// </summary>
public static class CollectionValidationContextHelperExtensions
{
    /// <summary>
    /// Ensures, that the <paramref name="source"/> has exactly <paramref name="count"/> elements in it.
    /// </summary>
    /// <typeparam name="T">The collection Type</typeparam>
    /// <param name="source">The <see cref="IValidationContext{TValue}"/> that is to be validated</param>
    /// <param name="count">The expected number of elements</param>
    /// <returns><paramref name="source"/></returns>
    public static IValidationContext<IReadOnlyCollection<T>> HaveCount<T>(this IValidationContext<IReadOnlyCollection<T>> source, int count)
    {
        if(source.Value.Count != count)
            source.AddError($"Expected a count of {count} but was {source.Value.Count}.");
        return source;
    }
}