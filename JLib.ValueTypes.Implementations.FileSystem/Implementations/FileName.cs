namespace JLib.ValueTypes.Implementations.FileSystem;

public abstract record FileName(string Value) : StringValueType(Value), IPathSegment
{
    [Validation]
    private static void Validate(ValidationContext<string> must)
        => must
            .NotContainInvalidFileNameChars();
}