using System.Collections;

namespace JLib.ValueTypes;

public static class CollectionValidationContextHelperExtensions
{
    public static IValidationContext<IReadOnlyCollection<T>> HaveCount<T>(this IValidationContext<IReadOnlyCollection<T>> source, int count)
    {
        if(source.Value.Count != count)
            source.AddError($"Expected a count of {count} but was {source.Value.Count}.");
        return source;
    }
}